using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Common;
using Common.Log;
using Lykke.Service.ArbitrageDetector.Core;
using Lykke.Service.ArbitrageDetector.Core.Utils;
using Lykke.Service.ArbitrageDetector.Core.Domain;
using Lykke.Service.ArbitrageDetector.Core.Services;
using Lykke.Service.ArbitrageDetector.Services.Models;
using MoreLinq;

namespace Lykke.Service.ArbitrageDetector.Services
{
    public class ArbitrageDetectorService : TimerPeriod, IArbitrageDetectorService
    {
        private readonly ConcurrentDictionary<AssetPairSource, OrderBook> _orderBooks;
        private readonly ConcurrentDictionary<AssetPairSource, CrossRate> _crossRates;
        private readonly ConcurrentDictionary<string, Arbitrage> _arbitrages;
        private ConcurrentDictionary<string, Arbitrage> _arbitrageHistory;
        private IEnumerable<string> _baseAssets;
        private IEnumerable<string> _intermediateAssets;
        private string _quoteAsset;
        private int _expirationTimeInSeconds;
        private readonly int _historyMaxSize;
        private bool _restartNeeded;
        private int _minSpread;
        
        private readonly ILog _log;

        public ArbitrageDetectorService(StartupSettings settings, ILog log, IShutdownManager shutdownManager)
            : base(settings.ExecutionDelayInMilliseconds, log)
        {
            if (settings == null)
                throw new ArgumentNullException(nameof(settings));

            _baseAssets = settings.BaseAssets;
            _intermediateAssets = settings.IntermediateAssets;
            _quoteAsset = settings.QuoteAsset;
            _expirationTimeInSeconds = settings.ExpirationTimeInSeconds;
            _historyMaxSize = settings.HistoryMaxSize;
            _minSpread = settings.MinSpread;

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

        public Arbitrage GetArbitrage(string conversionPath)
        {
            if (string.IsNullOrWhiteSpace(conversionPath))
                throw new ArgumentNullException(nameof(conversionPath));

            var bestArbitrage = _arbitrageHistory.FirstOrDefault(x => string.Equals(x.Value.ConversionPath, conversionPath, StringComparison.CurrentCultureIgnoreCase));

            return bestArbitrage.Value;
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
            return new Settings(_expirationTimeInSeconds, _baseAssets, _intermediateAssets, _quoteAsset, _minSpread);
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

            if (settings.IntermediateAssets != null)
            {
                _intermediateAssets = settings.IntermediateAssets;
                restartNeeded = true;
            }

            if (settings.BaseAssets != null)
            {
                _baseAssets = settings.BaseAssets;
                restartNeeded = true;
            }

            if (!string.IsNullOrWhiteSpace(settings.QuoteAsset))
            {
                _quoteAsset = settings.QuoteAsset;
                restartNeeded = true;
            }

            settings.MinSpread = settings.MinSpread >= 0 ? 0 : settings.MinSpread;
            settings.MinSpread = settings.MinSpread < -100 ? -100 : settings.MinSpread;
            if (_minSpread != settings.MinSpread)
            {
                _minSpread = settings.MinSpread;
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
            await CalculateCrossRates();
            await RefreshArbitrages();                                        

            RestartIfNeeded();
        }

        public async Task<IEnumerable<CrossRate>> CalculateCrossRates()
        {
            var watch = Stopwatch.StartNew();

            var newActualCrossRates = new Dictionary<AssetPairSource, CrossRate>();
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
                        ? wantedIntermediateAssetPair.Quote
                        : wantedIntermediateAssetPair.Base;

                    // If settings contains any and current intermediate not in the settings then ignore
                    if (_intermediateAssets.Any() && !_intermediateAssets.Contains(intermediateCurrency))
                        continue;

                    // If original wanted/base or base/wanted pair then just save it
                    if (intermediateCurrency == _quoteAsset)
                    {
                        var intermediateWantedCrossRate = CrossRate.FromOrderBook(wantedOrderBook, new AssetPair(wantedCurrency, _quoteAsset));

                        var key = new AssetPairSource(intermediateWantedCrossRate.ConversionPath, intermediateWantedCrossRate.AssetPair);
                        newActualCrossRates[key] = intermediateWantedCrossRate;

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
                        newActualCrossRates[key] = crossRate;
                    }
                }
            }

            _crossRates.AddOrUpdateRange(newActualCrossRates);

            watch.Stop();
            if (watch.ElapsedMilliseconds > 200)
                await _log.WriteInfoAsync(GetType().Name, nameof(CalculateCrossRates), $"{watch.ElapsedMilliseconds} ms, {_crossRates.Count} cross rates, {actualOrderBooks.Count} order books.");

            return _crossRates.Select(x => x.Value).ToList().AsReadOnly();
        }
        
        public async Task<ConcurrentDictionary<string, Arbitrage>> CalculateArbitrages()
        {
            var newArbitrages = new ConcurrentDictionary<string, Arbitrage>();
            var actualCrossRates = GetActualCrossRates();

            // For each asset pair - for each cross rate make one line for every ask and bid, order that lines and find intersections
            var uniqueAssetPairs = actualCrossRates.Select(x => x.AssetPair).Distinct().ToList();
            foreach (var assetPair in uniqueAssetPairs)
            {
                var watch = Stopwatch.StartNew();

                var assetPairCrossRates = actualCrossRates.Where(x => x.AssetPair.Equals(assetPair)).ToList();

                var asksAndBids = CalculateArbitragesLines(assetPairCrossRates);
                var asksAndBidsMs = watch.ElapsedMilliseconds;

                if (!asksAndBids.HasValue)
                    return newArbitrages;

                var totalItarations = 0;
                var possibleArbitrages = 0;
                var asks = asksAndBids.Value.asks;
                var asksCount = asks.Count;
                var bids = asksAndBids.Value.bids;
                var bidsCount = bids.Count;
                // Calculate arbitrage for every ask and every higher bid
                for (var a = 0; a < asksCount; a++)
                {
                    var ask = asks[a];
                    var askPrice = ask.Price;

                    for (var b = 0; b < bidsCount; b++)
                    {
                        totalItarations++;

                        var bid = bids[b];
                        var bidPrice = bid.Price;

                        if (askPrice >= bidPrice)
                            continue;

                        possibleArbitrages++;

                        var spread = Arbitrage.GetSpread(askPrice, bidPrice);
                        if (_minSpread >= 0 || spread < _minSpread)
                            continue;

                        var key = Arbitrage.FormatConversionPath(ask.CrossRate.ConversionPath, bid.CrossRate.ConversionPath);
                        if (newArbitrages.TryGetValue(key, out var existed))
                        {
                            var askVolume = ask.Volume;
                            var bidVolume = bid.Volume;
                            var volume = askVolume < bidVolume ? askVolume : bidVolume;
                            var pnL = Arbitrage.GetPnL(askPrice, bidPrice, volume);
                            if (pnL <= existed.PnL)
                                continue;

                            var arbitrage = new Arbitrage(assetPair, ask.CrossRate, new VolumePrice(ask.Price, ask.Volume), bid.CrossRate, new VolumePrice(bid.Price, bid.Volume));
                            newArbitrages.AddOrUpdate(key, arbitrage);
                        }
                        else
                        {
                            var arbitrage = new Arbitrage(assetPair, ask.CrossRate, new VolumePrice(ask.Price, ask.Volume), bid.CrossRate, new VolumePrice(bid.Price, bid.Volume));
                            newArbitrages.Add(key, arbitrage);
                        }
                    }
                }

                watch.Stop();
                if (watch.ElapsedMilliseconds > 1000)
                    await _log.WriteInfoAsync(GetType().Name, nameof(CalculateArbitrages), $"{watch.ElapsedMilliseconds} ms, {newArbitrages.Count} arbitrages, {actualCrossRates.Count} actual cross rates, {asksAndBidsMs} for asks and bids, {asks.Count} asks, {bids.Count} bids, {totalItarations} iterations, {possibleArbitrages} possible arbitrages.");
            }

            return newArbitrages;
        }

        public async Task RefreshArbitrages()
        {
            var watch = Stopwatch.StartNew();

            var newArbitrages = await CalculateArbitrages(); // One per conversion path (with best PnL)
            var calculateArbitragesMs = watch.ElapsedMilliseconds;

            var removed = 0;
            // Remove every ended arbitrage and move it to the history
            foreach (var oldArbitrage in _arbitrages)
            {
                // If still present then do nothing
                if (newArbitrages.Keys.Contains(oldArbitrage.Key))
                    continue;

                removed++;
                MoveFromActualToHistory(oldArbitrage.Value);
            }

            // Add new arbitrages, and replace existed if new PnL is better
            var added = 0;
            foreach (var newArbitrage in newArbitrages)
            {
                // New
                if (!_arbitrages.TryGetValue(newArbitrage.Key, out var oldArbitrage))
                {
                    added++;
                    _arbitrages.Add(newArbitrage.Key, newArbitrage.Value);
                }
                // Already existed
                else
                {
                    if (newArbitrage.Value.PnL > oldArbitrage.PnL)
                    {
                        removed++;
                        MoveFromActualToHistory(oldArbitrage);

                        _arbitrages.Add(newArbitrage.Key, newArbitrage.Value);
                    }
                }
            }

            var beforeCleaning = _arbitrageHistory.Count;
            CleanHistory();

            watch.Stop();
            if (watch.ElapsedMilliseconds - calculateArbitragesMs > 100)
                await _log.WriteInfoAsync(GetType().Name, nameof(RefreshArbitrages), $"{watch.ElapsedMilliseconds} ms, new {newArbitrages.Count} arbitrages, {removed} removed, {added} added, {beforeCleaning - _arbitrageHistory.Count} cleaned, {_arbitrages.Count} active, {_arbitrageHistory.Count} in history.");
        }

        

        private void CleanHistory()
        {
            // TODO: can be improved.
            var remained = new ConcurrentDictionary<string, Arbitrage>();

            // Get distinct paths and for each path remain only %_historyMaxSize% of the best
            var uniqueAssetPairs = _arbitrageHistory.Values.Select(x => x.AssetPair).Distinct().ToList();
            foreach (var assetPair in uniqueAssetPairs)
            {
                _arbitrageHistory.Where(x => x.Value.AssetPair.Equals(assetPair))
                    .OrderByDescending(x => x.Value.PnL)
                    .Take(_historyMaxSize)
                    .ForEach(x => remained.Add(x.Key, x.Value));
            }

            _arbitrageHistory = remained;
        }

        private (IList<ArbitrageLine> asks, IList<ArbitrageLine> bids)? CalculateArbitragesLines(IList<CrossRate> crossRates)
        {
            // If no asks or bids then return empty list
            if (!crossRates.SelectMany(x => x.Asks).Any() || !crossRates.SelectMany(x => x.Bids).Any())
                return null;

            // 1. Calculate minAsk and maxBid
            var minAsk = crossRates.SelectMany(x => x.Asks).Min(x => x.Price);
            var maxBid = crossRates.SelectMany(x => x.Bids).Max(x => x.Price);

            // No arbitrages
            if (minAsk >= maxBid)
                return null;

            // 2. Collect only arbitrages lines
            var asks = new List<ArbitrageLine>();
            var bids = new List<ArbitrageLine>();
            foreach (var crossRate in crossRates)
            {
                crossRate.Asks.Where(x => x.Price < maxBid)
                    .ForEach(x => asks.Add(new ArbitrageLine(crossRate, x)));

                crossRate.Bids.Where(x => x.Price > minAsk)
                    .ForEach(x => bids.Add(new ArbitrageLine(crossRate, x)));
            }

            // 3. Order by Price
            asks = asks.OrderByDescending(x => x.Price).ToList();
            bids = bids.OrderByDescending(x => x.Price).ToList();

            return (asks, bids);
        }

        private void MoveFromActualToHistory(Arbitrage arbitrage)
        {
            var key = arbitrage.ToString();

            // Remove from actual arbitrages
            arbitrage.EndedAt = DateTime.UtcNow;
            _arbitrages.Remove(key);

            // Add it to the history
            _arbitrageHistory.AddOrUpdate(key, arbitrage);
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

        private async void RestartIfNeeded()
        {
            if (_restartNeeded)
            {
                _restartNeeded = false;

                _crossRates.Clear();
                _arbitrages.Clear();
                _arbitrageHistory.Clear();

                await _log.WriteInfoAsync(GetType().Name, nameof(RestartIfNeeded), $"Restarted");
            }
        }
    }
}
