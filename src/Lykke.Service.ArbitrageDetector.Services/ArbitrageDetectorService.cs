using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Common;
using Common.Log;
using Lykke.Service.ArbitrageDetector.Core;
using Lykke.Service.ArbitrageDetector.Core.Utils;
using Lykke.Service.ArbitrageDetector.Core.Domain;
using Lykke.Service.ArbitrageDetector.Core.Services;
using Lykke.Service.ArbitrageDetector.Services.Model;

namespace Lykke.Service.ArbitrageDetector.Services
{
    public class ArbitrageDetectorService : TimerPeriod, IArbitrageDetectorService
    {
        private readonly ConcurrentDictionary<AssetPairSource, OrderBook> _orderBooks;
        private readonly ConcurrentDictionary<AssetPairSource, CrossRate> _crossRates;
        private readonly ConcurrentDictionary<string, Arbitrage> _arbitrages;
        private readonly ConcurrentDictionary<string, Arbitrage> _arbitrageHistory;
        private IEnumerable<string> _baseAssets;
        private string _quoteAsset;
        private int _expirationTimeInSeconds;
        private readonly int _historyMaxSize;
        private bool _restartNeeded;
        
        private readonly ILog _log;

        public ArbitrageDetectorService(StartupSettings settings, ILog log, IShutdownManager shutdownManager)
            : base(settings.ExecutionDelayInMilliseconds, log)
        {
            if (settings == null)
                throw new ArgumentNullException(nameof(settings));

            _baseAssets = settings.BaseAssets;
            _quoteAsset = settings.QuoteAsset;
            _expirationTimeInSeconds = settings.ExpirationTimeInSeconds;
            _historyMaxSize = settings.HistoryMaxSize;

            _log = log;
            shutdownManager?.Register(this);

            _orderBooks = new ConcurrentDictionary<AssetPairSource, OrderBook>();
            _crossRates = new ConcurrentDictionary<AssetPairSource, CrossRate>();
            _arbitrages = new ConcurrentDictionary<string, Arbitrage>();
            _arbitrageHistory = new ConcurrentDictionary<string, Arbitrage>();
        }

        public IEnumerable<OrderBook> GetOrderBooks()
        {
            if (!_orderBooks.Any())
                return new List<OrderBook>();

            return _orderBooks.Select(x => x.Value)
                .OrderByDescending(x => x.Timestamp)
                .ToList();
        }

        public IEnumerable<OrderBook> GetOrderBooks(string exchange, string instrument)
        {
            if (!_orderBooks.Any())
                return new List<OrderBook>();

            var result = _orderBooks.Select(x => x.Value).ToList();

            if (!string.IsNullOrWhiteSpace(exchange))
                result = result.Where(x => x.Source.ToUpper().Trim().Contains(exchange.ToUpper().Trim())).ToList();

            if (!string.IsNullOrWhiteSpace(instrument))
                result = result.Where(x => x.AssetPairStr.ToUpper().Trim().Contains(instrument.ToUpper().Trim())).ToList();

            return result.OrderByDescending(x => x.Timestamp).ToList();
        }

        public IEnumerable<CrossRate> GetCrossRates()
        {
            if (!_crossRates.Any())
                return new List<CrossRate>();

            var result = _crossRates.Select(x => x.Value)
                .OrderByDescending(x => x.Timestamp)
                .ToList();

            return result;
        }

        public IEnumerable<Arbitrage> GetArbitrages()
        {
            if (!_arbitrages.Any())
                return new List<Arbitrage>();

            return _arbitrages.Select(x => x.Value)
                .OrderByDescending(x => x.PnL)
                .ToList();
        }

        public Arbitrage GetArbitrage(Guid id)
        {
            return _arbitrageHistory.FirstOrDefault(x => x.Value.Id == id).Value;
        }

        public IEnumerable<Arbitrage> GetArbitrageHistory(DateTime since, int take)
        {
            if (!_arbitrageHistory.Any())
                return new List<Arbitrage>();

            var result = new List<Arbitrage>();

            var arbitrages = _arbitrageHistory.Select(x => x.Value).ToList();
            var uniqueConversionPaths = arbitrages.Select(x => x.ConversionPath).Distinct().ToList();

            // Find only best arbitrage for path
            foreach (var conversionPath in uniqueConversionPaths)
            {
                var pathBestArbitrage = arbitrages.OrderByDescending(x => x.PnL).First(x => x.ConversionPath == conversionPath);
                result.Add(pathBestArbitrage);
            }

            return result
                .Where(x => x.EndedAt > since)
                .OrderByDescending(x => x.PnL)
                .Take(take)
                .ToList();
        }

        public Settings GetSettings()
        {
            return new Settings(_expirationTimeInSeconds, _baseAssets, _quoteAsset);
        }

        public void SetSettings(Settings settings)
        {
            if (settings == null)
                throw new ArgumentNullException(nameof(settings));

            var restartNeeded = false;

            if (settings.ExpirationTimeInSeconds > 0)
            {
                _expirationTimeInSeconds = settings.ExpirationTimeInSeconds;
                restartNeeded = true;
            }

            if (settings.BaseAssets != null && settings.BaseAssets.Any())
            {
                _baseAssets = settings.BaseAssets;
                restartNeeded = true;
            }

            if (!string.IsNullOrWhiteSpace(settings.QuoteAsset))
            {
                _quoteAsset = settings.QuoteAsset;
                restartNeeded = true;
            }

            _restartNeeded = restartNeeded;
        }



        public void Process(OrderBook orderBook)
        {
            // Update if contains base currency
            CheckForCurrencyAndUpdateOrderBooks(_quoteAsset, orderBook);

            // Update if contains wanted currency
            foreach (var wantedCurrency in _baseAssets)
            {
                CheckForCurrencyAndUpdateOrderBooks(wantedCurrency, orderBook);
            }
        }

        public override async Task Execute()
        {
            var watch = System.Diagnostics.Stopwatch.StartNew();

            IEnumerable<CrossRate> crossRates = null;
            try
            {
                crossRates = CalculateCrossRates();
                RefreshArbitrages();

                RestartIfNeeded();
            }
            catch (Exception ex)
            {
                await _log.WriteErrorAsync(GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
            }

            watch.Stop();
            if (watch.ElapsedMilliseconds > 3000)
            {
                await _log.WriteInfoAsync(GetType().Name, MethodBase.GetCurrentMethod().Name, $"Execute() took {watch.ElapsedMilliseconds} ms to execute for {crossRates?.Count()} cross rates.");
            }
        }

        public IEnumerable<CrossRate> CalculateCrossRates()
        {
            var newActualCrossRates = new SortedDictionary<AssetPairSource, CrossRate>();
            var actualOrderBooks = GetActualOrderBooks();

            foreach (var wantedCurrency in _baseAssets)
            {
                var wantedCurrencyKeys = actualOrderBooks.Keys.Where(x => x.AssetPair.ContainsAsset(wantedCurrency)).ToList();
                foreach (var wantedCurrencykey in wantedCurrencyKeys)
                {
                    var wantedOrderBook = actualOrderBooks[wantedCurrencykey];

                    // Trying to find wanted asset in current orderBook's asset pair
                    var wantedIntermediateAssetPair = AssetPair.FromString(wantedOrderBook.AssetPairStr, wantedCurrency);

                    // Get intermediate currency
                    var intermediateCurrency = wantedIntermediateAssetPair.Base == wantedCurrency
                        ? wantedIntermediateAssetPair.Quoting
                        : wantedIntermediateAssetPair.Base;

                    // If original wanted/base or base/wanted pair then just save it
                    if (intermediateCurrency == _quoteAsset)
                    {
                        var intermediateWantedCrossRate = CrossRate.FromOrderBook(wantedOrderBook, new AssetPair(wantedCurrency, _quoteAsset));

                        var key = new AssetPairSource(intermediateWantedCrossRate.ConversionPath, intermediateWantedCrossRate.AssetPair);
                        newActualCrossRates.AddOrUpdate(key, intermediateWantedCrossRate);

                        continue;
                    }

                    // Trying to find intermediate/base or base/intermediate pair from any exchange
                    var intermediateBaseCurrencyKeys = actualOrderBooks.Keys
                        .Where(x => x.AssetPair.ContainsAsset(intermediateCurrency) && x.AssetPair.ContainsAsset(_quoteAsset))
                        .ToList();

                    foreach (var intermediateBaseCurrencyKey in intermediateBaseCurrencyKeys)
                    {
                        // Calculating cross rate for base/wanted pair
                        var wantedIntermediateOrderBook = wantedOrderBook;
                        var intermediateBaseOrderBook = actualOrderBooks[intermediateBaseCurrencyKey];

                        var targetBaseAssetPair = new AssetPair(wantedCurrency, _quoteAsset);
                        var crossRate = CrossRate.FromOrderBooks(wantedIntermediateOrderBook, intermediateBaseOrderBook, targetBaseAssetPair);

                        var key = new AssetPairSource(crossRate.ConversionPath, crossRate.AssetPair);
                        newActualCrossRates.AddOrUpdate(key, crossRate);
                    }
                }
            }

            _crossRates.AddOrUpdateRange(newActualCrossRates);

            return _crossRates.Values.ToList().AsReadOnly();
        }

        public IEnumerable<Arbitrage> CalculateArbitrages()
        {
            var newArbitrages = new SortedDictionary<string, Arbitrage>();
            var actualCrossRates = GetActualCrossRates();

            // For each asset pair - for each cross rate make one line for every ask and bid, order that lines and find intersections
            var uniqueAssetPairs = actualCrossRates.Select(x => x.AssetPair).Distinct().ToList();
            foreach (var assetPair in uniqueAssetPairs)
            {
                var lines = new List<ArbitrageLine>();
                var assetPairCrossRates = actualCrossRates.Where(x => x.AssetPair.Equals(assetPair)).ToList();

                // Add all asks and bids
                foreach (var crossRate in assetPairCrossRates)
                {
                    foreach (var crossRateAsk in crossRate.Asks)
                    {
                        lines.Add(new ArbitrageLine
                        {
                            CrossRate = crossRate,
                            AskPrice = crossRateAsk.Price,
                            Volume = crossRateAsk.Volume
                        });
                    }

                    foreach (var crossRateBid in crossRate.Bids)
                    {
                        lines.Add(new ArbitrageLine
                        {
                            CrossRate = crossRate,
                            BidPrice = crossRateBid.Price,
                            Volume = crossRateBid.Volume
                        });
                    }
                }

                // Order by Price
                lines = lines.OrderBy(x => x.Price).ThenBy(x => x.AskPrice).ToList();

                // Calculate arbitrage for every ask and every higher bid
                for (var a = 0; a < lines.Count; a++)
                {
                    var askLine = lines[a];
                    if (askLine.AskPrice != 0)
                        for (var b = a + 1; b < lines.Count; b++)
                        {
                            var bidLine = lines[b];
                            if (bidLine.BidPrice != 0)
                            {
                                var key = "(" + askLine.CrossRate.ConversionPath + ") * (" + bidLine.CrossRate.ConversionPath + ")";

                                if (newArbitrages.TryGetValue(key, out var existed))
                                {
                                    var spread = (askLine.Price - bidLine.Price) / bidLine.Price * 100;
                                    var volume = askLine.Volume < bidLine.Volume ? askLine.Volume : bidLine.Volume;
                                    var pnL = Math.Abs(spread * volume);

                                    if (pnL > existed.PnL)
                                    {
                                        var arbitrage = new Arbitrage(assetPair, askLine.CrossRate, askLine.VolumePrice, bidLine.CrossRate, bidLine.VolumePrice);
                                        newArbitrages[key] = arbitrage;
                                    }
                                }
                                else
                                {
                                    var arbitrage = new Arbitrage(assetPair, askLine.CrossRate, askLine.VolumePrice, bidLine.CrossRate, bidLine.VolumePrice);
                                    newArbitrages.Add(key, arbitrage);
                                }
                            }
                        }
                }
            }

            return newArbitrages.Values;
        }

        public void RefreshArbitrages()
        {
            var newArbitragesList = CalculateArbitrages();
            var newArbitrages = new ConcurrentDictionary<string, Arbitrage>();
            
            foreach (var newArbitrage in newArbitragesList)
            {
                // Key must be unique for arbitrage in order to find when it started
                var key = newArbitrage.ToString();
                newArbitrages.AddOrUpdate(key, newArbitrage);
            }

            // Remove every ended arbitrage and move it to the history
            foreach (var oldArbitrage in _arbitrages)
            {
                if (!newArbitrages.Keys.Contains(oldArbitrage.Key))
                {
                    // Remove from actual arbitrages
                    oldArbitrage.Value.EndedAt = DateTime.UtcNow;
                    _arbitrages.Remove(oldArbitrage.Key);

                    // Add it to the history
                    _arbitrageHistory.AddOrUpdate(oldArbitrage.Key, oldArbitrage.Value);
                }
            }

            // Add only new arbitrages, don't update existed to not change the StartedAt
            foreach (var newArbitrage in newArbitrages)
            {
                if (!_arbitrages.Keys.Contains(newArbitrage.Key))
                {
                    _arbitrages.Add(newArbitrage.Key, newArbitrage.Value);
                }
            }

            // If there are too many items
            CleanHistory();
        }


        private void CleanHistory()
        {
            var arbitrageHistory = _arbitrageHistory.Values;
            var extraCount = _arbitrageHistory.Count - _historyMaxSize;
            if (extraCount > 0)
            {
                // First try to delete extra arbitrages with the same conversion path
                var uniqueConversionPaths = arbitrageHistory.Select(x => x.ConversionPath).Distinct().ToList();
                foreach (var conversionPath in uniqueConversionPaths)
                {
                    var pathArbitrages = arbitrageHistory.OrderByDescending(x => x.PnL)
                        .Where(x => x.ConversionPath == conversionPath)
                        .Skip(1); // Leave 1 best for path
                    foreach (var arbitrage in pathArbitrages)
                        _arbitrageHistory.Remove(arbitrage.ToString());
                }
            }

            // If didn't help then delete extra with the oldest conversion path
            extraCount = _arbitrageHistory.Count - _historyMaxSize;
            if (extraCount > 0)
            {
                var arbitrages = arbitrageHistory.Take(extraCount).ToList();
                foreach (var arbitrage in arbitrages)
                {
                    _arbitrageHistory.Remove(arbitrage.ToString());
                }
            }
        }

        private void CheckForCurrencyAndUpdateOrderBooks(string currency, OrderBook orderBook)
        {
            if (!orderBook.AssetPairStr.Contains(currency))
                return;

            orderBook.SetAssetPair(currency);

            var key = new AssetPairSource(orderBook.Source, orderBook.AssetPair);
            _orderBooks.AddOrUpdate(key, orderBook);
        }

        private ConcurrentDictionary<AssetPairSource, OrderBook> GetActualOrderBooks()
        {
            var result = new ConcurrentDictionary<AssetPairSource, OrderBook>();

            foreach (var keyValue in _orderBooks)
            {
                if (DateTime.UtcNow - keyValue.Value.Timestamp < new TimeSpan(0, 0, 0, _expirationTimeInSeconds))
                {
                    result.Add(keyValue.Key, keyValue.Value);
                }
            }

            return result;
        }

        private IList<CrossRate> GetActualCrossRates()
        {
            var result = new List<CrossRate>();

            foreach (var crossRate in _crossRates)
            {
                if (DateTime.UtcNow - crossRate.Value.Timestamp < new TimeSpan(0, 0, 0, _expirationTimeInSeconds))
                {
                    result.Add(crossRate.Value);
                }
            }

            return result;
        }

        private void RestartIfNeeded()
        {
            if (_restartNeeded)
            {
                _restartNeeded = false;

                _orderBooks.Clear();
                _crossRates.Clear();
                _arbitrages.Clear();
                _arbitrageHistory.Clear();
            }
        }
    }
}
