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

namespace Lykke.Service.ArbitrageDetector.Services
{
    public class ArbitrageDetectorService : TimerPeriod, IArbitrageDetectorService
    {
        private readonly ConcurrentDictionary<ExchangeAssetPair, OrderBook> _orderBooks;
        private readonly HashSet<CrossRate> _crossRates;
        private readonly IReadOnlyCollection<string> _wantedCurrencies;
        private readonly string _baseCurrency;
        private readonly int _expirationTimeInSeconds;
        private readonly ILog _log;

        public ArbitrageDetectorService(ILog log, IShutdownManager shutdownManager, IReadOnlyCollection<string> wantedCurrencies, string baseCurrency, int executionDelay, int expirationTimeInSeconds)
            : base((int)TimeSpan.FromSeconds(executionDelay).TotalMilliseconds, log)
        {
            _log = log;
            _orderBooks = new ConcurrentDictionary<ExchangeAssetPair, OrderBook>();
            _crossRates = new HashSet<CrossRate>();
            _wantedCurrencies = wantedCurrencies;
            _baseCurrency = baseCurrency;
            _expirationTimeInSeconds = expirationTimeInSeconds;
            shutdownManager?.Register(this);
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

        public IEnumerable<CrossRate> CalculateCrossRates()
        {
            RemoveExpiredOrderBooks();

            foreach (var wantedCurrency in _wantedCurrencies)
            {
                var wantedCurrencyKeys = _orderBooks.Keys.Where(x => x.AssetPair.Contains(wantedCurrency)).ToList();
                foreach (var wantedCurrencykey in wantedCurrencyKeys)
                {
                    var wantedOrderBook = _orderBooks[wantedCurrencykey];
                    var currentExchange = wantedOrderBook.Source;

                    // Trying to find wanted asset in current orderBook's asset pair
                    var wantedIntermediateAssetPair = wantedOrderBook.GetAssetPairIfContains(wantedCurrency);
                    var wantedIntermediatePair = wantedIntermediateAssetPair.Value;

                    // If something wrong with orderBook then continue
                    if (!wantedOrderBook.Asks.Any() || wantedOrderBook.GetBestAsk() == 0 ||
                        !wantedOrderBook.Bids.Any() || wantedOrderBook.GetBestBid() == 0)
                    {
                        _log?.WriteInfoAsync(GetType().Name, MethodBase.GetCurrentMethod().Name, $"Skip {currentExchange}, {wantedOrderBook.AssetPairId}, bids.Count: {wantedOrderBook.Bids.Count}, asks.Count: {wantedOrderBook.Asks.Count}");
                        continue;
                    }

                    // Get intermediate currency
                    var intermediateCurrency = wantedIntermediatePair.Base == wantedCurrency
                        ? wantedIntermediatePair.Quote
                        : wantedIntermediatePair.Base;

                    // If original wanted/base or base/wanted rate then just save it
                    if (intermediateCurrency == _baseCurrency)
                    {
                        CrossRate intermediateWantedCrossRate = null;

                        // Straight pair (wanted/base)
                        if (wantedIntermediatePair.Base == wantedCurrency && wantedIntermediatePair.Quote == intermediateCurrency)
                        {
                            intermediateWantedCrossRate = new CrossRate(
                                currentExchange,
                                wantedCurrency + intermediateCurrency,
                                wantedOrderBook.GetBestBid(),
                                wantedOrderBook.GetBestAsk(),
                                currentExchange,
                                new List<OrderBook> { wantedOrderBook }
                            );
                        }

                        // Reversed pair (base/wanted)
                        if (wantedIntermediatePair.Base == intermediateCurrency && wantedIntermediatePair.Quote == wantedCurrency)
                        {
                            intermediateWantedCrossRate = new CrossRate(
                                currentExchange,
                                intermediateCurrency + wantedCurrency,
                                1 / wantedOrderBook.GetBestAsk(), // reversed
                                1 / wantedOrderBook.GetBestBid(), // reversed
                                wantedOrderBook.AssetPairId,
                                new List<OrderBook> { wantedOrderBook }
                            );
                        }

                        _crossRates.AddOrUpdate(intermediateWantedCrossRate);

                        continue;
                    }

                    // Trying to find intermediate/base or base/intermediate pair from any exchange
                    var intermediateBaseCurrencyKeys = _orderBooks.Keys
                        .Where(x => x.AssetPair.Contains(intermediateCurrency) && x.AssetPair.Contains(_baseCurrency)).ToList();

                    foreach (var intermediateBaseCurrencyKey in intermediateBaseCurrencyKeys)
                    {
                        // Calculating cross rate for base/wanted pair
                        var wantedIntermediateOrderBook = wantedOrderBook;
                        var intermediateBaseOrderBook = _orderBooks[intermediateBaseCurrencyKey];

                        var intermediateBasePair = intermediateBaseOrderBook.GetAssetPairIfContains(_baseCurrency).Value;

                        // Getting wanted/intermediate and intermediate/base bid and ask
                        var wantedIntermediateBidAsk = GetBidAndAsk(wantedIntermediatePair, wantedCurrency, intermediateCurrency, wantedIntermediateOrderBook);
                        var intermediateBaseBidAsk = GetBidAndAsk(intermediateBasePair, intermediateCurrency, _baseCurrency, intermediateBaseOrderBook);

                        // Calculating wanted/base bid and ask
                        var wantedBaseBid = wantedIntermediateBidAsk.Bid * intermediateBaseBidAsk.Bid;
                        var wantedBaseAsk = wantedIntermediateBidAsk.Ask * intermediateBaseBidAsk.Ask;

                        // Saving to CrossRate collection
                        var wantedBasePairStr = wantedCurrency + _baseCurrency;
                        var wantedBaseCrossRateInfo = new CrossRate(
                            $"{currentExchange}-{intermediateBaseOrderBook.Source}",
                            wantedBasePairStr,
                            wantedBaseBid,
                            wantedBaseAsk,
                            $"({wantedIntermediateOrderBook.Source}-{wantedIntermediateOrderBook.AssetPairId}*{intermediateBaseOrderBook.Source}-{intermediateBaseOrderBook.AssetPairId})",
                            new List<OrderBook> { wantedIntermediateOrderBook, intermediateBaseOrderBook }
                        );


                        _crossRates.AddOrUpdate(wantedBaseCrossRateInfo);
                    }
                }
            }

            return _crossRates.ToList().AsReadOnly();
        }

        public IEnumerable<Arbitrage> GetArbitrages()
        {
            var result = new List<Arbitrage>();

            RemoveExpiredCrossRates();

            for (var i = 0; i < _crossRates.Count; i++)
            {
                for (var j = i + 1; j < _crossRates.Count; j++)
                {
                    var crossRate1 = _crossRates.ElementAt(i);
                    var crossRate2 = _crossRates.ElementAt(j);

                    if (crossRate1.Ask < crossRate2.Bid)
                        result.Add(new Arbitrage(crossRate1, crossRate2));

                    if (crossRate2.Ask < crossRate1.Bid)
                        result.Add(new Arbitrage(crossRate2, crossRate1));
                }
            }

            return result;
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

        public IEnumerable<OrderBook> GetOrderBooks()
        {
            return _orderBooks.Select(x => x.Value).ToList();
        }

        public IEnumerable<CrossRate> GetCrossRates()
        {
            return _crossRates.ToList().AsReadOnly();
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
                    lowAsk.AssetPair,
                    lowAsk.ConversionPath,
                    lowAsk.Ask.ToString("0.#####"),
                    highBid.ConversionPath,
                    highBid.Bid.ToString("0.#####"),
                    lowAsk.OriginalOrderBooks.Count == 1 ? lowAsk.OriginalOrderBooks.First().Timestamp : lowAsk.Timestamp,
                    highBid.OriginalOrderBooks.Count == 1 ? highBid.OriginalOrderBooks.First().Timestamp : highBid.Timestamp
                ));
            }

            return result;
        }

        private void RemoveExpiredOrderBooks()
        {
            foreach (var keyValue in _orderBooks)
            {
                if (DateTime.UtcNow - keyValue.Value.Timestamp > new TimeSpan(0, 0, 0, _expirationTimeInSeconds))
                {
                    _orderBooks.Remove(keyValue.Key);
                }
            }
        }

        private void CheckForCurrencyAndUpdateOrderBooks(string currency, OrderBook orderBook)
        {
            var baseAssetPair = orderBook.GetAssetPairIfContains(currency);
            if (baseAssetPair.HasValue)
            {
                var key = new ExchangeAssetPair(orderBook.Source, orderBook.AssetPairId);
                _orderBooks.AddOrUpdate(key, orderBook);
            }
        }

        private BidAsk GetBidAndAsk(AssetPair pair, string _base, string quote, OrderBook orderBook)
        {
            #region Argument checking

            if (pair.Base == null || pair.Quote == null)
                throw new ArgumentNullException(nameof(pair));

            if (string.IsNullOrWhiteSpace(_base))
                throw new ArgumentOutOfRangeException(nameof(_base));

            if (string.IsNullOrWhiteSpace(quote))
                throw new ArgumentOutOfRangeException(nameof(quote));

            #endregion

            decimal intermediateBaseBid;
            decimal intermediateBaseAsk;
            if (pair.Base == _base && pair.Quote == quote)
            {
                intermediateBaseBid = orderBook.GetBestBid();
                intermediateBaseAsk = orderBook.GetBestAsk();
            }
            else if (pair.Base == quote && pair.Quote == _base)
            {
                intermediateBaseBid = 1 / orderBook.GetBestAsk();
                intermediateBaseAsk = 1 / orderBook.GetBestBid();
            }
            else
            {
                throw new InvalidOperationException("Assets must be the same");
            }

            return new BidAsk(intermediateBaseBid, intermediateBaseAsk);
        }

        private void RemoveExpiredCrossRates()
        {
            foreach (var crossRate in _crossRates.ToList())
            {
                var isExpired = crossRate.OriginalOrderBooks.Any(x => DateTime.UtcNow - x.Timestamp > new TimeSpan(0, 0, 0, _expirationTimeInSeconds));
                if (isExpired)
                    _crossRates.Remove(crossRate);
            }
        }
    }
}
