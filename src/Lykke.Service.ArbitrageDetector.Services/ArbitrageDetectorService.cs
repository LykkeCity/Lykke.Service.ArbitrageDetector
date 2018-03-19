using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Common;
using Common.Log;
using Lykke.Service.ArbitrageDetector.Core.Utils;
using Lykke.Service.ArbitrageDetector.Core.Domain;
using Lykke.Service.ArbitrageDetector.Core.Services;
using MoreLinq;
using AssetPair = Lykke.Service.ArbitrageDetector.Core.Domain.AssetPair;

namespace Lykke.Service.ArbitrageDetector.Services
{
    public class ArbitrageDetectorService : TimerPeriod, IArbitrageDetectorService
    {
        private readonly SortedDictionary<ExchangeAssetPair, OrderBook> _orderBooks;
        private readonly SortedDictionary<ExchangeAssetPair, CrossRate> _crossRates;
        private readonly SortedDictionary<string, Arbitrage> _arbitrages;
        private readonly SortedDictionary<DateTime, ArbitrageHistory> _arbitrageHistory;
        private readonly IReadOnlyCollection<string> _wantedCurrencies;
        private readonly string _baseCurrency;
        private readonly int _expirationTimeInSeconds;
        private readonly ILog _log;

        public ArbitrageDetectorService(IReadOnlyCollection<string> wantedCurrencies, string baseCurrency, int executionDelay, int expirationTimeInSeconds,
            ILog log, IShutdownManager shutdownManager)
            : base((int) TimeSpan.FromSeconds(executionDelay).TotalMilliseconds, log)
        {
            _log = log;
            _orderBooks = new SortedDictionary<ExchangeAssetPair, OrderBook>();
            _crossRates = new SortedDictionary<ExchangeAssetPair, CrossRate>();
            _arbitrages = new SortedDictionary<string, Arbitrage>();
            _arbitrageHistory = new SortedDictionary<DateTime, ArbitrageHistory>();
            _wantedCurrencies = wantedCurrencies;
            _baseCurrency = baseCurrency;
            _expirationTimeInSeconds = expirationTimeInSeconds;
            shutdownManager?.Register(this);
        }

        public IEnumerable<OrderBook> GetOrderBooks()
        {
            return _orderBooks.Select(x => x.Value)
                .OrderByDescending(x => x.Timestamp)
                .ToList()
                .AsReadOnly();
        }

        public IEnumerable<OrderBook> GetOrderBooks(string exchange, string instrument)
        {
            var result = _orderBooks.Select(x => x.Value).ToList();

            if (!string.IsNullOrWhiteSpace(exchange))
                result = result.Where(x => x.Source.ToUpper().Trim().Contains(exchange.ToUpper().Trim())).ToList();

            if (!string.IsNullOrWhiteSpace(instrument))
                result = result.Where(x => x.AssetPairStr.ToUpper().Trim().Contains(instrument.ToUpper().Trim())).ToList();

            return result.OrderByDescending(x => x.Timestamp).ToList();
        }

        public IEnumerable<CrossRate> GetCrossRates()
        {
            return _crossRates.Values
                .OrderByDescending(x => x.Timestamp)
                .ToList()
                .AsReadOnly();
        }

        public IEnumerable<Arbitrage> GetArbitrages()
        {
            var result = new List<Arbitrage>();

            var actualCrossRates = GetActualCrossRates();

            // For each asset - for each cross rate make two lines, order that lines and find intersection
            var uniqueAssetPairs = actualCrossRates.Select(x => x.AssetPairStr).Distinct().ToList();            
            foreach (var asset in uniqueAssetPairs)
            {
                var lines = new List<ArbitrageLine>();
                var assetCrossRates = actualCrossRates.Where(x => x.AssetPairStr == asset).ToList();

                // Add one cross rate as two lines (bid and ask)
                foreach (var crossRate in assetCrossRates)
                {
                    lines.Add(new ArbitrageLine
                    {
                        CrossRate = crossRate,
                        Bid = Math.Round(crossRate.BestBidPrice, 8),
                        Volume = crossRate.BestAskVolume < crossRate.BestBidVolume ? crossRate.BestAskVolume : crossRate.BestBidVolume
                    });

                    lines.Add(new ArbitrageLine
                    {
                        CrossRate = crossRate,
                        Ask = Math.Round(crossRate.BestAskPrice, 8),
                        Volume = crossRate.BestAskVolume < crossRate.BestBidVolume ? crossRate.BestAskVolume : crossRate.BestBidVolume
                    });
                }

                // Order by Price
                lines = lines.OrderBy(x => x.Price).ThenBy(x => x.Ask).ToList();

                // Calculate arbitrage for every ask and every higher bid
                for (var a = 0; a < lines.Count; a++)
                {
                    var askLine = lines[a];
                    if (askLine.Ask != 0)
                        for (var b = a + 1; b < lines.Count; b++)
                        {
                            var bidLine = lines[b];
                            if (bidLine.Bid != 0)
                            {
                                var arbitrage = new Arbitrage(askLine.CrossRate, askLine.VolumePrice, bidLine.CrossRate, bidLine.VolumePrice);
                                result.Add(arbitrage);
                            }
                        }
                }
            }

            return result;
        }

        public IEnumerable<ArbitrageHistory> GetArbitrageHistory(DateTime since)
        {
            return _arbitrageHistory
                .Where(x => x.Key > since)
                .Select(x => x.Value)
                .OrderByDescending(x => x.Timestamp)
                .ToList();
        }


        public void Process(OrderBook orderBook)
        {
            // Update if contains base currency
            CheckForCurrencyAndUpdateOrderBooks(_baseCurrency, orderBook);

            // Update if contains wanted currency
            foreach (var wantedCurrency in _wantedCurrencies)
            {
                CheckForCurrencyAndUpdateOrderBooks(wantedCurrency, orderBook);
            }
        }

        public override async Task Execute()
        {
            await CalculateCrossRates();
            await ProcessArbitrages();
        }

        public async Task<IEnumerable<CrossRate>> CalculateCrossRates()
        {
            var actualOrderBooks = GetActualOrderBooks();

            foreach (var wantedCurrency in _wantedCurrencies)
            {
                var wantedCurrencyKeys = actualOrderBooks.Keys.Where(x => x.AssetPair.ContainsAsset(wantedCurrency)).ToList();
                foreach (var wantedCurrencykey in wantedCurrencyKeys)
                {
                    var wantedOrderBook = actualOrderBooks[wantedCurrencykey];
                    var currentExchange = wantedOrderBook.Source;

                    // Trying to find wanted asset in current orderBook's asset pair
                    var wantedIntermediateAssetPair = AssetPair.FromString(wantedOrderBook.AssetPairStr, wantedCurrency);

                    // If something wrong with orderBook then continue
                    if (!wantedOrderBook.Asks.Any() || wantedOrderBook.BestAskPrice == 0 ||
                        !wantedOrderBook.Bids.Any() || wantedOrderBook.BestBidPrice == 0)
                    {
                        await _log?.WriteInfoAsync(GetType().Name, MethodBase.GetCurrentMethod().Name,
                            $"Skip {currentExchange}, {wantedOrderBook.AssetPairStr}, bids.Count: {wantedOrderBook.Bids.Count}, asks.Count: {wantedOrderBook.Asks.Count}");
                        continue;
                    }

                    // Get intermediate currency
                    var intermediateCurrency = wantedIntermediateAssetPair.Base == wantedCurrency
                        ? wantedIntermediateAssetPair.Quoting
                        : wantedIntermediateAssetPair.Base;

                    // If original wanted/base or base/wanted rate then just save it
                    if (intermediateCurrency == _baseCurrency)
                    {
                        var intermediateWantedCrossRate = CrossRate.FromOrderBook(wantedOrderBook, new AssetPair(wantedCurrency, _baseCurrency));

                        var key = new ExchangeAssetPair(intermediateWantedCrossRate.ConversionPath, intermediateWantedCrossRate.AssetPair);
                        _crossRates.AddOrUpdate(key, intermediateWantedCrossRate);

                        continue;
                    }

                    // Trying to find intermediate/base or base/intermediate pair from any exchange
                    var intermediateBaseCurrencyKeys = actualOrderBooks.Keys
                        .Where(x => x.AssetPair.ContainsAsset(intermediateCurrency) && x.AssetPair.ContainsAsset(_baseCurrency))
                        .ToList();

                    foreach (var intermediateBaseCurrencyKey in intermediateBaseCurrencyKeys)
                    {
                        // Calculating cross rate for base/wanted pair
                        var wantedIntermediateOrderBook = wantedOrderBook;
                        var intermediateBaseOrderBook = actualOrderBooks[intermediateBaseCurrencyKey];

                        var targetBaseAssetPair = new AssetPair(wantedCurrency, _baseCurrency);
                        var crossRate = CrossRate.FromOrderBooks(wantedIntermediateOrderBook, intermediateBaseOrderBook, targetBaseAssetPair);

                        var key = new ExchangeAssetPair(crossRate.ConversionPath, crossRate.AssetPair);
                        _crossRates.AddOrUpdate(key, crossRate);
                    }
                }
            }

            return _crossRates.Values.ToList().AsReadOnly();
        }

        public async Task ProcessArbitrages()
        {
            var newArbitragesList = GetArbitrages();
            var newArbitrages = new SortedDictionary<string, Arbitrage>();
            foreach (var newArbitrage in newArbitragesList)
            {
                var key = newArbitrage.ToString();
                newArbitrages.AddOrUpdate(key, newArbitrage);
            }

            // Remove old
            foreach (var oldArbitrage in _arbitrages)
            {
                if (!newArbitrages.Keys.Contains(oldArbitrage.Key))
                {
                    _arbitrages.Remove(oldArbitrage.Key);

                    // Write to history and log
                    _arbitrageHistory.Add(DateTime.UtcNow, new ArbitrageHistory(oldArbitrage.Value, ArbitrageHistoryType.Ended));
                    if (_log != null)
                        await _log.WriteInfoAsync(GetType().Name, MethodBase.GetCurrentMethod().Name, $"Ended: {oldArbitrage.Value}, was available for {(DateTime.UtcNow - oldArbitrage.Value.StartedTimestamp).Seconds} seconds");
                }
            }

            // Update or Add new
            foreach (var newArbitrage in newArbitrages)
            {
                if (!_arbitrages.Keys.Contains(newArbitrage.Key))
                {
                    _arbitrages.Add(newArbitrage.Key, newArbitrage.Value);

                    // Write to history and log
                    _arbitrageHistory.Add(DateTime.UtcNow, new ArbitrageHistory(newArbitrage.Value, ArbitrageHistoryType.Started));
                    if (_log != null)
                        await _log.WriteInfoAsync(GetType().Name, MethodBase.GetCurrentMethod().Name, $"Started: {newArbitrage.Value}");
                }
            }
        }


        private void CheckForCurrencyAndUpdateOrderBooks(string currency, OrderBook orderBook)
        {
            if (!orderBook.AssetPairStr.Contains(currency))
                return;

            orderBook.SetAssetPair(currency);

            var key = new ExchangeAssetPair(orderBook.Source, orderBook.AssetPair);
            _orderBooks.AddOrUpdate(key, orderBook);
        }

        private Dictionary<ExchangeAssetPair, OrderBook> GetActualOrderBooks()
        {
            var result = new Dictionary<ExchangeAssetPair, OrderBook>();

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
    }
}
