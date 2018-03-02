using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Common;
using Common.Log;
using Lykke.Service.ArbitrageDetector.Core.Domain;
using Lykke.Service.ArbitrageDetector.Core.Services;

namespace Lykke.Service.ArbitrageDetector.Services
{
    public class ArbitrageCalculator : TimerPeriod, IArbitrageCalculator
    {
        private readonly ILog _log;
        private readonly IDictionary<(string source, string assetPair), OrderBook> _orderBooks;
        private readonly IDictionary<(string Source, string AssetPair), CrossRate> _crossRates;
        private readonly IReadOnlyCollection<string> _wantedCurrencies;
        private readonly string _baseCurrency;
        private readonly int _expirationTimeInSeconds;

        public ArbitrageCalculator(ILog log, IReadOnlyCollection<string> wantedCurrencies, string baseCurrency, int executionDelay, int expirationTimeInSeconds)
            : base((int)TimeSpan.FromSeconds(executionDelay).TotalMilliseconds, log)
        {
            _log = log;
            _orderBooks = new ConcurrentDictionary<(string, string), OrderBook>();
            _crossRates = new ConcurrentDictionary<(string, string), CrossRate>();
            _wantedCurrencies = wantedCurrencies;
            _baseCurrency = baseCurrency;
            _expirationTimeInSeconds = expirationTimeInSeconds;
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

        private void CheckForCurrencyAndUpdateOrderBooks(string currency, OrderBook orderBook)
        {
            var baseAssetPair = orderBook.GetAssetPairIfContains(currency);
            if (baseAssetPair.HasValue)
            {
                AddOrUpdateOrderBooks(orderBook);
            }
        }

        private void AddOrUpdateOrderBooks(OrderBook orderBook)
        {
            var key = (orderBook.Source, orderBook.AssetPairId);
            if (_orderBooks.ContainsKey(key))
            {
                _orderBooks.Remove(key);
            }
            _orderBooks.Add(key, orderBook);
        }

        public override async Task Execute()
        {
            RemoveExpiredOrderBooks();
            CalculateCrossRates();
            var arbitrages = FindArbitrage();
            foreach (var arbitrage in arbitrages)
            {
                await _log?.WriteMonitorAsync(GetType().Name, MethodBase.GetCurrentMethod().Name, $"arbitrage: {arbitrage}");
            }
        }

        private void RemoveExpiredOrderBooks()
        {
            foreach (var keyValue in _orderBooks)
            {
                if (DateTime.UtcNow - keyValue.Value.Timestamp > new TimeSpan(0, 0, 0, _expirationTimeInSeconds))
                    _orderBooks.Remove(keyValue.Key);
            }
        }

        public IDictionary<(string Source, string AssetPair), CrossRate> CalculateCrossRates()
        {
            foreach (var wantedCurrency in _wantedCurrencies)
            {
                var wantedCurrencyKeys = _orderBooks.Keys.Where(x => x.assetPair.Contains(wantedCurrency)).ToList();
                foreach (var key in wantedCurrencyKeys)
                {
                    var wantedOrderBook = _orderBooks[key];
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
                    var intermediateCurrency = wantedIntermediatePair.fromAsset == wantedCurrency
                        ? wantedIntermediatePair.toAsset
                        : wantedIntermediatePair.fromAsset;

                    // If original wanted/base or base/wanted rate then just save it
                    if (intermediateCurrency == _baseCurrency)
                    {
                        CrossRate intermediateWantedCrossRate = null;

                        // Straight pair (wanted/base)
                        if (wantedIntermediatePair.fromAsset == wantedCurrency && wantedIntermediatePair.toAsset == intermediateCurrency)
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
                        if (wantedIntermediatePair.fromAsset == intermediateCurrency && wantedIntermediatePair.toAsset == wantedCurrency)
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

                        if (intermediateWantedCrossRate == null)
                        {
                            throw new NullReferenceException($"intermediateWantedCrossRate is null");
                        }

                        AddOrUpdateCrossRate((currentExchange, intermediateWantedCrossRate.AssetPair), intermediateWantedCrossRate);

                        continue;
                    }

                    // Trying to find intermediate/base or base/intermediate pair from any exchange
                    var intermediateBaseCurrencyKeys = _orderBooks.Keys
                        .Where(x => x.assetPair.Contains(intermediateCurrency) && x.assetPair.Contains(_baseCurrency)).ToList();

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
                            $"{wantedBasePairStr} = {wantedIntermediateOrderBook.Source}-{wantedIntermediateOrderBook.AssetPairId} * {intermediateBaseOrderBook.Source}-{intermediateBaseOrderBook.AssetPairId}",
                            new List<OrderBook> { wantedIntermediateOrderBook, intermediateBaseOrderBook }
                        );

                        AddOrUpdateCrossRate((currentExchange, wantedBasePairStr), wantedBaseCrossRateInfo);
                    }
                }
            }

            return _crossRates;
        }

        private void AddOrUpdateCrossRate((string exchange, string wantedBasePair) key, CrossRate crossRate)
        {
            if (_crossRates.ContainsKey(key))
            {
                _crossRates.Remove(key);
            }
            _crossRates.Add(key, crossRate);
        }

        public IList<string> FindArbitrage()
        {
            var result = new List<string>();

            foreach (var crossRateInfo1 in _crossRates)
                foreach (var crossRateInfo2 in _crossRates)
                {
                    if (crossRateInfo1.Value == crossRateInfo2.Value)
                        continue;

                    var crossRate1 = crossRateInfo1.Value;
                    var crossRate2 = crossRateInfo2.Value;

                    if (crossRateInfo1.Value.BestAsk < crossRateInfo2.Value.BestBid)
                        result.Add(string.Format("{0}: {1}.ask={2} < {3}.bid={4}, {5}, {6}",
                            crossRate1.AssetPair,
                            crossRate1.ConversionPath,
                            crossRate1.BestAsk,
                            crossRate2.ConversionPath,
                            crossRate2.BestBid,
                            crossRate1.OriginalOrderBooks.Count == 1 ? crossRate1.OriginalOrderBooks.First().Timestamp : crossRate1.Timestamp,
                            crossRate2.OriginalOrderBooks.Count == 1 ? crossRate2.OriginalOrderBooks.First().Timestamp : crossRate2.Timestamp
                            ));
                }

            return result;
        }

        private (decimal Bid, decimal Ask) GetBidAndAsk((string fromAsset, string toAsset) pair, string fromAsset, string toAsset, OrderBook orderBook)
        {
            #region Argument checking

            if (pair.fromAsset == null || pair.toAsset == null)
                throw new ArgumentNullException(nameof(pair));

            if (string.IsNullOrWhiteSpace(fromAsset))
                throw new ArgumentOutOfRangeException(nameof(fromAsset));

            if (string.IsNullOrWhiteSpace(toAsset))
                throw new ArgumentOutOfRangeException(nameof(toAsset));

            #endregion

            decimal intermediateBaseBid;
            decimal intermediateBaseAsk;
            if (pair.fromAsset == fromAsset && pair.toAsset == toAsset)
            {
                intermediateBaseBid = orderBook.GetBestBid();
                intermediateBaseAsk = orderBook.GetBestAsk();
            }
            else if (pair.fromAsset == toAsset && pair.toAsset == fromAsset)
            {
                intermediateBaseBid = 1 / orderBook.GetBestAsk();
                intermediateBaseAsk = 1 / orderBook.GetBestBid();
            }
            else
            {
                throw new InvalidOperationException("Assets must be the same");
            }

            return (intermediateBaseBid, intermediateBaseAsk);
        }
    }
}
