using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
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
        private readonly IReadOnlyCollection<string> _wantedCurrencies;
        private readonly string _baseCurrency;
        private readonly int _expirationTimeInSeconds;

        public ArbitrageCalculator(ILog log, IReadOnlyCollection<string> wantedCurrencies, string baseCurrency, int executionDelay, int expirationTimeInSeconds)
            : base((int)TimeSpan.FromSeconds(executionDelay).TotalMilliseconds, log)
        {
            _log = log;
            _orderBooks = new ConcurrentDictionary<(string, string), OrderBook>();
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

            _log?.WriteMonitor(GetType().Name, MethodBase.GetCurrentMethod().Name, $"Exchange: {orderBook.Source}, assetPair: {orderBook.AssetPairId}");
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
            await _log?.WriteMonitorAsync(GetType().Name, MethodBase.GetCurrentMethod().Name, string.Empty);

            RemoveExpiredOrderBooks();
            CalculateCrossRates();
        }

        private void RemoveExpiredOrderBooks()
        {
            foreach (var keyValue in _orderBooks)
            {
                if (DateTime.UtcNow - keyValue.Value.Timestamp > new TimeSpan(0, 0, 0, _expirationTimeInSeconds))
                    _orderBooks.Remove(keyValue.Key);
            }
        }

        public Dictionary<(string Source, string AssetPair), CrossRateInfo> CalculateCrossRates()
        {
            var crossRates = new Dictionary<(string Source, string AssetPair), CrossRateInfo>();

            foreach (var wantedCurrency in _wantedCurrencies)
            {
                var wantedCurrencyKeys = _orderBooks.Keys.Where(x => x.assetPair.Contains(wantedCurrency)).ToList();
                foreach (var key in wantedCurrencyKeys)
                {
                    var orderBook = _orderBooks[key];
                    var currentExchange = orderBook.Source;

                    var wantedIntermediateAssetPair = orderBook.GetAssetPairIfContains(wantedCurrency);
                    if (wantedIntermediateAssetPair == null)
                    {
                        continue;
                    }
                    if (!orderBook.Asks.Any() || orderBook.GetBestAsk() == 0 || !orderBook.Bids.Any() || orderBook.GetBestBid() == 0)
                    {
                        continue;
                    }

                    var wantedIntermediatePair = wantedIntermediateAssetPair.Value;

                    var intermediateCurrency = wantedIntermediatePair.fromAsset == wantedCurrency
                        ? wantedIntermediatePair.toAsset
                        : wantedIntermediatePair.fromAsset;

                    // Original wanted/base or base/wanted rate
                    if (intermediateCurrency == _baseCurrency)
                    {
                        CrossRateInfo intermediateWantedCrossRateInfo = null;

                        if (wantedIntermediatePair.fromAsset == wantedCurrency &&
                            wantedIntermediatePair.toAsset == intermediateCurrency)
                        {
                            intermediateWantedCrossRateInfo = new CrossRateInfo(
                                currentExchange,
                                wantedCurrency + intermediateCurrency,
                                orderBook.GetBestBid(),
                                orderBook.GetBestAsk(),
                                "",
                                new List<OrderBook> { orderBook }
                            );
                        }

                        if (wantedIntermediatePair.fromAsset == wantedCurrency &&
                            wantedIntermediatePair.toAsset == intermediateCurrency)
                        {
                            intermediateWantedCrossRateInfo = new CrossRateInfo(
                                currentExchange,
                                intermediateCurrency + wantedCurrency,
                                1 / orderBook.GetBestAsk(), // reversed
                                1 / orderBook.GetBestBid(), // reversed
                                orderBook.AssetPairId,
                                new List<OrderBook> { orderBook }
                            );
                        }

                        if (intermediateWantedCrossRateInfo == null)
                        {
                            throw new NullReferenceException($"intermediateWantedCrossRateInfo is null");
                        }

                        crossRates.Add((currentExchange, intermediateWantedCrossRateInfo.AssetPair), intermediateWantedCrossRateInfo);
                    }

                    // Trying to find intermediate/_base or _base/intermediate pair within the same exchange
                    var intermediateBaseCurrencyKey = _orderBooks.Keys
                        .FirstOrDefault(x => x.source == currentExchange &&
                                    x.assetPair.Contains(intermediateCurrency) &&
                                    x.assetPair.Contains(_baseCurrency));

                    if (intermediateBaseCurrencyKey.assetPair == null || intermediateBaseCurrencyKey.source == null)
                        continue;

                    // Calculating cross rate for base/wanted pair

                    var wantedIntermediateOrderBook = orderBook;
                    var intermediateBaseOrderBook = _orderBooks[intermediateBaseCurrencyKey];

                    var intermediateBasePairTuple = intermediateBaseOrderBook.GetAssetPairIfContains(_baseCurrency);
                    if (!intermediateBasePairTuple.HasValue)
                        continue;

                    var wantedIntermediateBidAsk = GetBidAndAsk(wantedIntermediatePair, wantedCurrency, intermediateCurrency, wantedIntermediateOrderBook);
                    var intermediateBaseBidAsk = GetBidAndAsk(intermediateBasePairTuple.Value, intermediateCurrency, _baseCurrency, intermediateBaseOrderBook);

                    var wantedBaseBid = wantedIntermediateBidAsk.Bid * intermediateBaseBidAsk.Bid;
                    var wantedBaseAsk = wantedIntermediateBidAsk.Ask * intermediateBaseBidAsk.Ask;

                    var wantedBasePairStr = wantedCurrency + _baseCurrency;
                    var wantedBaseCrossRateInfo = new CrossRateInfo(
                        currentExchange,
                        wantedBasePairStr,
                        wantedBaseBid,
                        wantedBaseAsk,
                        $"{wantedBasePairStr} = {wantedIntermediateOrderBook.AssetPairId} + {intermediateBaseOrderBook.AssetPairId}",
                        new List<OrderBook> { wantedIntermediateOrderBook, intermediateBaseOrderBook }
                    );

                    crossRates.Add((currentExchange, wantedBasePairStr), wantedBaseCrossRateInfo);
                }
            }

            return crossRates;
        }
        
        //TODO: !!!
        public void GetCrossRatesWithTheLargestSpread()
        {    
            throw new NotImplementedException();
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
