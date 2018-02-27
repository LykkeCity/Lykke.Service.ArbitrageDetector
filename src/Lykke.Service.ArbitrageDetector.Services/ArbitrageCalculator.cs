using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Reflection;
using Common.Log;
using Lykke.Service.ArbitrageDetector.Core.Domain;
using Lykke.Service.ArbitrageDetector.Core.Services;
using MoreLinq;

namespace Lykke.Service.ArbitrageDetector.Services
{
    public class ArbitrageCalculator : IArbitrageCalculator
    {
        private readonly ILog log;
        private static readonly ConcurrentDictionary<string, CrossRateInfo> orderBooks = new ConcurrentDictionary<string, CrossRateInfo>();

        public ArbitrageCalculator(ILog log)
        {
            this.log = log;
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
                    var btcusd = orderBooks.ContainsKey(sameExchangeBtcUsdKey) ? orderBooks[sameExchangeBtcUsdKey] : (CrossRateInfo?)null;
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
                    if (orderBooks.ContainsKey(key))
                        orderBooks.Remove(key, out temp);

                    orderBooks.TryAdd(key, crossRateInfo.Value);
                }
            }

            log.WriteMonitor(GetType().Name, MethodBase.GetCurrentMethod().Name, $"Exchange: {orderBook.Source}, assetPair: {orderBook.AssetPairId}");
        }
    }
}
