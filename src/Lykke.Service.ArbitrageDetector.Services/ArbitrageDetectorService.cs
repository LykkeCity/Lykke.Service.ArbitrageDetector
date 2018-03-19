using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Common;
using Common.Log;
using Lykke.Service.ArbitrageDetector.Core.Utils;
using Lykke.Service.ArbitrageDetector.Core.Domain;
using Lykke.Service.ArbitrageDetector.Core.Services;
using AssetPair = Lykke.Service.ArbitrageDetector.Core.Domain.AssetPair;

namespace Lykke.Service.ArbitrageDetector.Services
{
    public class ArbitrageDetectorService : TimerPeriod, IArbitrageDetectorService
    {
        private readonly ConcurrentDictionary<ExchangeAssetPair, OrderBook> _orderBooks;
        private readonly ConcurrentDictionary<ExchangeAssetPair, CrossRate> _crossRates;
        private readonly IReadOnlyCollection<string> _wantedCurrencies;
        private readonly string _baseCurrency;
        private readonly int _expirationTimeInSeconds;
        private readonly ILog _log;

        public ArbitrageDetectorService(ILog log, IShutdownManager shutdownManager,
            IReadOnlyCollection<string> wantedCurrencies, string baseCurrency, int executionDelay,
            int expirationTimeInSeconds)
            : base((int) TimeSpan.FromSeconds(executionDelay).TotalMilliseconds, log)
        {
            _log = log;
            _orderBooks = new ConcurrentDictionary<ExchangeAssetPair, OrderBook>();
            _crossRates = new ConcurrentDictionary<ExchangeAssetPair, CrossRate>();
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

        public IEnumerable<OrderBook> GetOrderBooksByExchange(string exchange)
        {
            var result = _orderBooks.Select(x => x.Value).ToList();

            if (!string.IsNullOrWhiteSpace(exchange))
                result = result.Where(x => x.Source.ToUpper().Trim().Contains(exchange.ToUpper().Trim())).ToList();

            return result.OrderByDescending(x => x.Timestamp).ToList();
        }

        public IEnumerable<OrderBook> GetOrderBooksByInstrument(string instrument)
        {
            var result = _orderBooks.Select(x => x.Value).ToList();

            if (!string.IsNullOrWhiteSpace(instrument))
                result = result.Where(x => x.AssetPairStr.ToUpper().Trim().Contains(instrument.ToUpper().Trim())).ToList();

            return result.OrderByDescending(x => x.Timestamp).ToList();
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

            // TODO: To list, sort and one iteration (as in BO.AE)
            for (var i = 0; i < actualCrossRates.Count; i++)
            {
                for (var j = i + 1; j < actualCrossRates.Count; j++)
                {
                    var crossRate1 = actualCrossRates.ElementAt(i);
                    var crossRate2 = actualCrossRates.ElementAt(j);

                    if (crossRate1.BestAskPrice < crossRate2.BestBidPrice)
                        result.Add(new Arbitrage(crossRate1, crossRate2));

                    if (crossRate2.BestAskPrice < crossRate1.BestBidPrice)
                        result.Add(new Arbitrage(crossRate2, crossRate1));
                }
            }

            return result;
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
            CalculateCrossRates();
            var arbitrages = GetArbitragesStrings();
            foreach (var arbitrage in arbitrages)
            {
                await _log?.WriteInfoAsync(GetType().Name, MethodBase.GetCurrentMethod().Name, $"{arbitrage}");
            }
        }

        public IEnumerable<CrossRate> CalculateCrossRates()
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
                        _log?.WriteInfoAsync(GetType().Name, MethodBase.GetCurrentMethod().Name,
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


        private IEnumerable<string> GetArbitragesStrings()
        {
            var result = new List<string>();

            var arbitrages = GetArbitrages();

            foreach (var arbitrage in arbitrages)
            {
                var lowAsk = arbitrage.LowAsk;
                var highBid = arbitrage.HighBid;

                result.Add(string.Format("{0}: {1}.ask={2} < {3}.bid={4}, {5}, {6}",
                    lowAsk.AssetPairStr,
                    lowAsk.ConversionPath,
                    lowAsk.BestAskPrice.ToString("0.#####"),
                    highBid.ConversionPath,
                    highBid.BestBidPrice.ToString("0.#####"),
                    lowAsk.OriginalOrderBooks.Count == 1
                        ? lowAsk.OriginalOrderBooks.First().Timestamp
                        : lowAsk.Timestamp,
                    highBid.OriginalOrderBooks.Count == 1
                        ? highBid.OriginalOrderBooks.First().Timestamp
                        : highBid.Timestamp
                ));
            }

            return result;
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
