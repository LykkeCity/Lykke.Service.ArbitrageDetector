using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
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
        private static readonly ConcurrentDictionary<string, CrossRateInfo> _orderBooks = new ConcurrentDictionary<string, CrossRateInfo>();
        private readonly IReadOnlyCollection<string> _wantedCurrencies;

        // Периодичность запуска Execute() надо брать из конфига
        public ArbitrageCalculator(ILog log, IReadOnlyCollection<string> wantedCurrencies, int executionDelay = 10)
            : base((int)TimeSpan.FromSeconds(executionDelay).TotalMilliseconds, log)
        {
            _log = log;
            _wantedCurrencies = wantedCurrencies;
        }

        public void Process(OrderBook orderBook)
        {
            var assetPair = orderBook.GetAssetPairIfContainsUSD();
            if (assetPair.HasValue)
            {
                var key = $"{orderBook.Source}-{orderBook.AssetPairId}";
                CrossRateInfo? crossRateInfo = null;
                var pair = assetPair.Value;
                if (pair.fromAsset == "BTC" && pair.toAsset == "USD")
                {
                    crossRateInfo = new CrossRateInfo(
                        orderBook.Source,
                        orderBook.AssetPairId,
                        orderBook.GetBestBid(),
                        orderBook.GetBestAsk(),
                        string.Empty,
                        orderBook);
                }
                else
                {
                    var sameExchangeBtcUsdKey = $"{orderBook.Source}-BTCUSD";
                    var btcusd = _orderBooks.ContainsKey(sameExchangeBtcUsdKey) ? _orderBooks[sameExchangeBtcUsdKey] : (CrossRateInfo?)null;
                    if (btcusd.HasValue)
                    {
                        // Calculating cross rate
                        // USD/XXX
                        if (pair.fromAsset == "USD")
                        {
                            //1. BTC/XXX
                            var bid = btcusd.Value.BestBid * orderBook.GetBestBid();
                            var ask = btcusd.Value.BestAsk * orderBook.GetBestAsk();

                            //2. BTC/USD
                            //var finalBid = bid / 
                        }
                        // XXX/USD
                        if (pair.toAsset == "USD")
                        {


                        }
                    }
                }

                if (crossRateInfo.HasValue)
                {
                    
                    CrossRateInfo temp;
                    if (_orderBooks.ContainsKey(key))
                        _orderBooks.Remove(key, out temp);

                    _orderBooks.TryAdd(key, crossRateInfo.Value);
                }
            }

            _log.WriteMonitor(GetType().Name, MethodBase.GetCurrentMethod().Name, $"Exchange: {orderBook.Source}, assetPair: {orderBook.AssetPairId}");
        }

        public override async Task Execute()
        {
            await _log.WriteMonitorAsync(GetType().Name, MethodBase.GetCurrentMethod().Name, $"ArbitrageCalculator.Execute()");
        }
    }
}
