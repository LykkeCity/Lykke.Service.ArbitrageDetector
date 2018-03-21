using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Common;
using Common.Log;
using Lykke.Service.ArbitrageDetector.Core.Utils;
using Lykke.Service.ArbitrageDetector.Core.Domain;
using Lykke.Service.ArbitrageDetector.Core.Services;
using AssetPair = Lykke.Service.ArbitrageDetector.Core.Domain.AssetPair;
using DataCrossRate = Lykke.Service.ArbitrageDetector.Core.DataModel.CrossRate;
using DataArbitrage = Lykke.Service.ArbitrageDetector.Core.DataModel.Arbitrage;

namespace Lykke.Service.ArbitrageDetector.Services
{
    public class ArbitrageDetectorService : TimerPeriod, IArbitrageDetectorService
    {
        private readonly ConcurrentDictionary<ExchangeAssetPair, OrderBook> _orderBooks;
        private readonly ConcurrentDictionary<ExchangeAssetPair, CrossRate> _crossRates;
        private readonly ConcurrentDictionary<string, Arbitrage> _arbitrages;
        private readonly ConcurrentDictionary<DateTime, Arbitrage> _arbitrageHistory;
        private readonly IReadOnlyCollection<string> _wantedCurrencies;
        private readonly string _baseCurrency;
        private readonly int _expirationTimeInSeconds;
        private readonly int _historyMaxSize;
        private readonly ILog _log;

        public ArbitrageDetectorService(IReadOnlyCollection<string> wantedCurrencies, string baseCurrency, int executionDelay, int expirationTimeInSeconds, int historyMaxSize,
            ILog log, IShutdownManager shutdownManager)
            : base((int) TimeSpan.FromSeconds(executionDelay).TotalMilliseconds, log)
        {
            _log = log;
            _orderBooks = new ConcurrentDictionary<ExchangeAssetPair, OrderBook>();
            _crossRates = new ConcurrentDictionary<ExchangeAssetPair, CrossRate>();
            _arbitrages = new ConcurrentDictionary<string, Arbitrage>();
            _arbitrageHistory = new ConcurrentDictionary<DateTime, Arbitrage>();
            _wantedCurrencies = wantedCurrencies;
            _baseCurrency = baseCurrency;
            _expirationTimeInSeconds = expirationTimeInSeconds;
            _historyMaxSize = historyMaxSize;
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

        public IEnumerable<DataCrossRate> GetCrossRates()
        {
            return _crossRates.Values
                .OrderByDescending(x => x.Timestamp)
                .Select(x => new DataCrossRate(x))
                .ToList()
                .AsReadOnly();
        }

        public IEnumerable<Arbitrage> GetArbitrages()
        {
            var result = new List<Arbitrage>();

            var actualCrossRates = GetActualCrossRates();

            // For each asset - for each cross rate make one line for every ask and bid, order that lines and find intersection
            var uniqueAssetPairs = actualCrossRates.Select(x => x.AssetPairStr).Distinct().ToList();            
            foreach (var asset in uniqueAssetPairs)
            {
                var lines = new List<ArbitrageLine>();
                var assetCrossRates = actualCrossRates.Where(x => x.AssetPairStr == asset).ToList();

                // Add all asks and bids
                foreach (var crossRate in assetCrossRates)
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
                                var arbitrage = new Arbitrage(askLine.CrossRate, askLine.VolumePrice, bidLine.CrossRate, bidLine.VolumePrice);
                                result.Add(arbitrage);
                            }
                        }
                }
            }

            return result;
        }

        public IEnumerable<DataArbitrage> GetArbitragesData()
        {
            return GetArbitrages()
                .OrderByDescending(x => x.EndedAt)
                .Select(x => new DataArbitrage(x))
                .ToList()
                .AsReadOnly();
        }

        public IEnumerable<DataArbitrage> GetArbitrageHistory(DateTime since, int take)
        {
            return _arbitrageHistory
                .Where(x => x.Key > since)
                .Select(x => new DataArbitrage(x.Value))
                .OrderByDescending(x => x.EndedAt)
                .Take(take)
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
            await CalculateArbitrages();
        }

        public async Task<IEnumerable<CrossRate>> CalculateCrossRates()
        {
            var newActualCrossRates = new ConcurrentDictionary<ExchangeAssetPair, CrossRate>();
            var actualOrderBooks = GetActualOrderBooks();

            foreach (var wantedCurrency in _wantedCurrencies)
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

                    // If original wanted/base or base/wanted rate then just save it
                    if (intermediateCurrency == _baseCurrency)
                    {
                        var intermediateWantedCrossRate = CrossRate.FromOrderBook(wantedOrderBook, new AssetPair(wantedCurrency, _baseCurrency));

                        var key = new ExchangeAssetPair(intermediateWantedCrossRate.ConversionPath, intermediateWantedCrossRate.AssetPair);
                        newActualCrossRates.AddOrUpdate(key, intermediateWantedCrossRate);

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
                        newActualCrossRates.AddOrUpdate(key, crossRate);
                    }
                }
            }

            _crossRates.AddOrUpdateRange(newActualCrossRates);

            return _crossRates.Values.ToList().AsReadOnly();
        }

        public async Task CalculateArbitrages()
        {
            var newArbitragesList = GetArbitrages();
            var newArbitrages = new ConcurrentDictionary<string, Arbitrage>();
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
                    oldArbitrage.Value.EndedAt = DateTime.UtcNow;
                    _arbitrages.Remove(oldArbitrage.Key);

                    // History
                    _arbitrageHistory.Add(DateTime.UtcNow, oldArbitrage.Value);
                }
            }

            // Add new
            foreach (var newArbitrage in newArbitrages)
            {
                if (!_arbitrages.Keys.Contains(newArbitrage.Key))
                {
                    RemoveFromHistoryIfItsTooLong();
                    _arbitrages.Add(newArbitrage.Key, newArbitrage.Value);
                }
            }
        }


        private void RemoveFromHistoryIfItsTooLong()
        {
            var extraCount = _arbitrageHistory.Count - _historyMaxSize;
            if (extraCount > 0)
            {
                var keys = _arbitrageHistory.Keys.Take(extraCount).ToList();
                foreach (var key in keys)
                {
                    _arbitrageHistory.Remove(key);
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

        private ConcurrentDictionary<ExchangeAssetPair, OrderBook> GetActualOrderBooks()
        {
            var result = new ConcurrentDictionary<ExchangeAssetPair, OrderBook>();

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
