using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Castle.Core.Logging;
using Lykke.Service.ArbitrageDetector.Core;
using Lykke.Service.ArbitrageDetector.Core.Domain;
using Lykke.Service.ArbitrageDetector.Services;
using MoreLinq;
using Xunit;

namespace Lykke.Service.ArbitrageDetector.Tests
{
    public class ArbitrageDetectorServiceTests
    {
        [Fact]
        public async Task StraightConversionTest()
        {
            // BTCEUR * EURUSD
            var baseAssets = new List<string> { "BTC" };
            const string quoteAsset = "USD";
            const string exchange = "Lykke";
            const string btcusd = "BTCUSD";

            var settings = new StartupSettings(10, 10, 1000, -20, baseAssets, quoteAsset);
            var arbitrageCalculator = new ArbitrageDetectorService(settings, null, null);

            var btcEurOrderBook = new OrderBook(exchange, "BTCEUR",
                new List<VolumePrice> // asks
                {
                    new VolumePrice(8999.95m, 10), new VolumePrice(9000, 10), new VolumePrice(9100, 10)
                },
                new List<VolumePrice> // bids
                {
                    new VolumePrice(8825, 10), new VolumePrice(8823, 10)
                },
                DateTime.UtcNow);

            var eurUsdOrderBook = new OrderBook(exchange, "EURUSD",
                new List<VolumePrice> // asks
                {
                    new VolumePrice(1.22033m, 10), new VolumePrice(1.22035m, 10), new VolumePrice(1.22040m, 10)
                },
                new List<VolumePrice> // bids
                {
                    new VolumePrice(1.2203m, 10), new VolumePrice(1.2201m, 10)
                },
                DateTime.UtcNow);

            arbitrageCalculator.Process(btcEurOrderBook);
            arbitrageCalculator.Process(eurUsdOrderBook);

            var crossRates = arbitrageCalculator.CalculateCrossRates();
            Assert.Single(crossRates);
            var crossRate = crossRates.First();
            Assert.Equal($"{exchange}-{exchange}", crossRate.Source);
            Assert.Equal("Lykke-BTCEUR & Lykke-EURUSD", crossRate.ConversionPath);
            Assert.Equal(btcusd, crossRate.AssetPairStr);
            Assert.Equal(10769.1475m, crossRate.Bids.Max(x => x.Price), 8);
            Assert.Equal(10982.9089835m, crossRate.Asks.Min(x => x.Price), 8);
            Assert.Equal(9, crossRate.Asks.Count);
            Assert.Equal(4, crossRate.Bids.Count);
            Assert.Equal(2, crossRate.OriginalOrderBooks.Count);
        }

        [Fact]
        public async Task ReverseConversionFirstPairTest()
        {
            // BTCEUR * USDEUR
            var baseAssets = new List<string> { "BTC" };
            const string quoteAsset = "USD";
            const string exchange = "Lykke";
            const string btcusd = "BTCUSD";

            var settings = new StartupSettings(10, 10, 1000, -20, baseAssets, quoteAsset);
            var arbitrageCalculator = new ArbitrageDetectorService(settings, null, null);

            var btcEurOrderBook = new OrderBook(exchange, "BTCEUR",
                new List<VolumePrice> // asks
                {
                    new VolumePrice(8999.95m, 10),
                    new VolumePrice(9000, 10),
                    new VolumePrice(9100, 10)
                },
                new List<VolumePrice> // bids
                {
                    new VolumePrice(8825, 10),
                    new VolumePrice(8823, 10)
                },
                DateTime.UtcNow);

            var usdEurOrderBook = new OrderBook(exchange, "USDEUR",
                new List<VolumePrice> // ask
                {
                    new VolumePrice(1/1.2203m, 10),
                    new VolumePrice(1/1.2201m, 10)
                },
                new List<VolumePrice> // bids
                {
                    new VolumePrice(1/1.22033m, 10),
                    new VolumePrice(1/1.22035m, 10)
                },
                DateTime.UtcNow);

            arbitrageCalculator.Process(btcEurOrderBook);
            arbitrageCalculator.Process(usdEurOrderBook);

            var crossRates = arbitrageCalculator.CalculateCrossRates();
            Assert.Single(crossRates);
            var crossRate = crossRates.First();
            Assert.Equal($"{exchange}-{exchange}", crossRate.Source);
            Assert.Equal("Lykke-BTCEUR & Lykke-USDEUR", crossRate.ConversionPath);
            Assert.Equal(btcusd, crossRate.AssetPairStr);
            Assert.Equal(10769.1475m, crossRate.Bids.Max(x => x.Price), 8);
            Assert.Equal(10982.9089835m, crossRate.Asks.Min(x => x.Price), 8);
            Assert.Equal(6, crossRate.Asks.Count);
            Assert.Equal(4, crossRate.Bids.Count);
            Assert.Equal(2, crossRate.OriginalOrderBooks.Count);
        }

        [Fact]
        public async Task ReverseConversionSecondPairTest()
        {
            // EURBTC * EURUSD
            var baseAssets = new List<string> { "BTC" };
            const string quoteAsset = "USD";
            const string exchange = "Lykke";
            const string btcusd = "BTCUSD";

            var settings = new StartupSettings(10, 10, 1000, -20, baseAssets, quoteAsset);
            var arbitrageCalculator = new ArbitrageDetectorService(settings, null, null);

            var btcEurOrderBook = new OrderBook(exchange, "EURBTC",
                new List<VolumePrice> // bids
                {
                    new VolumePrice(1/8825m, 10),
                    new VolumePrice(1/8823m, 10),
                    new VolumePrice(9100, 10)
                },
                new List<VolumePrice> // asks
                {
                    new VolumePrice(1/8999.95m, 10),
                    new VolumePrice(1/9000m, 10)
                },
                DateTime.UtcNow);

            var eurUsdOrderBook = new OrderBook(exchange, "EURUSD",
                new List<VolumePrice> // asks
                {
                    new VolumePrice(1.22033m, 10),
                    new VolumePrice(1.22035m, 10)
                },
                new List<VolumePrice> // bids
                {
                    new VolumePrice(1.2203m, 10),
                    new VolumePrice(1.2201m, 10)
                },
                DateTime.UtcNow);

            arbitrageCalculator.Process(btcEurOrderBook);
            arbitrageCalculator.Process(eurUsdOrderBook);

            var crossRates = arbitrageCalculator.CalculateCrossRates();
            Assert.Single(crossRates);
            var crossRate = crossRates.First();
            Assert.Equal($"{exchange}-{exchange}", crossRate.Source);
            Assert.Equal("Lykke-EURBTC & Lykke-EURUSD", crossRate.ConversionPath);
            Assert.Equal(btcusd, crossRate.AssetPairStr);
            Assert.Equal(10769.1475m, crossRate.Bids.Max(x => x.Price), 8);
            Assert.Equal(10982.9089835m, crossRate.Asks.Min(x => x.Price), 8);
            Assert.Equal(4, crossRate.Asks.Count);
            Assert.Equal(6, crossRate.Bids.Count);
            Assert.Equal(2, crossRate.OriginalOrderBooks.Count);
        }

        [Fact]
        public async Task ReverseConversionBothPairsTest()
        {
            // EURBTC * USDEUR
            var baseAssets = new List<string> { "BTC" };
            const string quoteAsset = "USD";
            const string exchange = "Lykke";
            const string btcusd = "BTCUSD";

            var settings = new StartupSettings(10, 10, 1000, -20, baseAssets, quoteAsset);
            var arbitrageCalculator = new ArbitrageDetectorService(settings, null, null);

            var eurBtcOrderBook = new OrderBook(exchange, "EURBTC",
                new List<VolumePrice> // asks
                {
                    new VolumePrice(1/8825m, 10),
                    new VolumePrice(1/8823m, 10),
                    new VolumePrice(9100, 10)
                },
                new List<VolumePrice> // bids
                {
                    new VolumePrice(1/8999.95m, 10),
                    new VolumePrice(1/9000m, 10)
                },
                DateTime.UtcNow);

            var usdEurOrderBook = new OrderBook(exchange, "USDEUR",
                new List<VolumePrice> // ask
                {
                    new VolumePrice(1/1.2203m, 10),
                    new VolumePrice(1/1.2201m, 10)
                },
                new List<VolumePrice> // bids
                {
                    new VolumePrice(1/1.22033m, 10),
                    new VolumePrice(1/1.22035m, 10)
                },
                DateTime.UtcNow);

            arbitrageCalculator.Process(eurBtcOrderBook);
            arbitrageCalculator.Process(usdEurOrderBook);

            var crossRates = arbitrageCalculator.CalculateCrossRates();
            Assert.Single(crossRates);
            var crossRate = crossRates.First();
            Assert.Equal($"{exchange}-{exchange}", crossRate.Source);
            Assert.Equal("Lykke-EURBTC & Lykke-USDEUR", crossRate.ConversionPath);
            Assert.Equal(btcusd, crossRate.AssetPairStr);
            Assert.Equal(10769.1475m, crossRate.Bids.Max(x => x.Price), 8);
            Assert.Equal(10982.9089835m, crossRate.Asks.Min(x => x.Price), 8);
            Assert.Equal(4, crossRate.Asks.Count);
            Assert.Equal(6, crossRate.Bids.Count);
            Assert.Equal(2, crossRate.OriginalOrderBooks.Count);
        }

        [Fact]
        public async Task ArbitrageTest()
        {
            var baseAssets = new List<string> { "BTC" };
            const string quoteAsset = "USD";

            var settings = new StartupSettings(10, 10, 1000, -20, baseAssets, quoteAsset);
            var arbitrageDetector = new ArbitrageDetectorService(settings, null, null);

            var btcUsdOrderBook1 = new OrderBook("GDAX", "BTCUSD",
                new List<VolumePrice> { new VolumePrice(11050, 10) }, // asks
                new List<VolumePrice> { new VolumePrice(11000, 10) }, // bids
                DateTime.UtcNow);

            var btcUsdOrderBook2 = new OrderBook("Bitfinex", "BTCUSD",
                new List<VolumePrice> { new VolumePrice(11300, 10) }, // asks
                new List<VolumePrice> { new VolumePrice(11100, 10) }, // bids
                DateTime.UtcNow);

            var btcEurOrderBook = new OrderBook("Quoine", "BTCEUR",
                new List<VolumePrice> { new VolumePrice(8999.95m, 10) }, // asks
                new List<VolumePrice> { new VolumePrice(8825, 10) }, // bids
                DateTime.UtcNow);

            var eurUsdOrderBook = new OrderBook("Binance", "EURUSD",
                new List<VolumePrice> { new VolumePrice(1.22033m, 10) }, // asks
                new List<VolumePrice> { new VolumePrice(1.2203m, 10) }, // bids
                DateTime.UtcNow);

            arbitrageDetector.Process(btcUsdOrderBook1);
            arbitrageDetector.Process(btcUsdOrderBook2);
            arbitrageDetector.Process(btcEurOrderBook);
            arbitrageDetector.Process(eurUsdOrderBook);

            await arbitrageDetector.Execute();

            var crossRates = arbitrageDetector.GetCrossRates().ToList();
            var arbitrages = arbitrageDetector.GetArbitrages().ToList();

            Assert.Equal(3, crossRates.Count);
            Assert.Equal(3, arbitrages.Count);

            var arbitrage1 = arbitrages.First(x => x.BidCrossRate.Source == "GDAX" && x.AskCrossRate.Source == "Quoine-Binance");
            Assert.Equal(11000, arbitrage1.BidCrossRate.Bids.Max(x => x.Price));
            Assert.Equal(10982.9089835m, arbitrage1.AskCrossRate.Asks.Max(x => x.Price), 8);
            Assert.Equal(arbitrage1.PnL, (arbitrage1.Bid.Price - arbitrage1.Ask.Price) * arbitrage1.Volume);

            var arbitrage2 = arbitrages.First(x => x.BidCrossRate.Source == "Bitfinex" && x.AskCrossRate.Source == "Quoine-Binance");
            Assert.Equal(11100, arbitrage2.BidCrossRate.Bids.Max(x => x.Price));
            Assert.Equal(10982.9089835m, arbitrage2.AskCrossRate.Asks.Max(x => x.Price), 8);
            Assert.Equal(arbitrage2.PnL, (arbitrage2.Bid.Price - arbitrage2.Ask.Price) * arbitrage2.Volume);

            var arbitrage3 = arbitrages.First(x => x.BidCrossRate.Source == "Bitfinex" && x.AskCrossRate.Source == "GDAX");
            Assert.Equal(11100, arbitrage3.BidCrossRate.Bids.Max(x => x.Price));
            Assert.Equal(11050m, arbitrage3.AskCrossRate.Asks.Max(x => x.Price));
            Assert.Equal(arbitrage3.PnL, (arbitrage3.Bid.Price - arbitrage3.Ask.Price) * arbitrage3.Volume);
        }

        [Fact]
        public async Task ArbitragePerformanceTest()
        {
            var baseAssets = new List<string> { "BTC" };
            const string quoteAsset = "USD";

            var settings = new StartupSettings(10, 10, 1000, -20, baseAssets, quoteAsset);
            var arbitrageDetector = new ArbitrageDetectorService(settings, null, null);

            var orderBooks = new List<OrderBook>();
            orderBooks.AddRange(GenerateOrderBooks(7, "GDAX", new AssetPair("BTC", "USD"), 10, 11500, 11000, 10, 11000, 10000));
            orderBooks.AddRange(GenerateOrderBooks(7, "Bitfinex", new AssetPair("BTC", "USD"), 10, 11600, 10900, 10, 10900, 10200));
            orderBooks.AddRange(GenerateOrderBooks(7, "Quoine", new AssetPair("BTC", "EUR"), 10, 9000, 8980.95m, 10, 8980.95m, 8825));
            orderBooks.AddRange(GenerateOrderBooks(7, "Binance", new AssetPair("EUR", "USD"), 10, 1.2205m, 1.2200m, 10, 1.2200m, 1.2190m));

            foreach (var orderBook in orderBooks)
                arbitrageDetector.Process(orderBook);

            var watch = System.Diagnostics.Stopwatch.StartNew();
            await arbitrageDetector.Execute();
            watch.Stop();
            Assert.True(watch.ElapsedMilliseconds < 1000);

            var crossRates = arbitrageDetector.GetCrossRates().ToList();
            var arbitrages = arbitrageDetector.GetArbitrages().ToList();

            Assert.Equal(63, crossRates.Count);
            Assert.Equal(343, arbitrages.Count);
        }

        [Fact]
        public async Task ArbitrageHistoryTest()
        {
            var baseAssets = new List<string> { "BTC" };
            const string quoteAsset = "USD";

            var settings = new StartupSettings(1, 1, 1000, -20, baseAssets, quoteAsset);
            var arbitrageDetector = new ArbitrageDetectorService(settings, null, null);

            var btcUsdOrderBook1 = new OrderBook("GDAX", "BTCUSD",
                new List<VolumePrice> { new VolumePrice(11050, 10) }, // asks
                new List<VolumePrice> { new VolumePrice(11000, 10) }, // bids
                DateTime.UtcNow);

            var btcUsdOrderBook2 = new OrderBook("Bitfinex", "BTCUSD",
                new List<VolumePrice> { new VolumePrice(11300, 10) }, // asks
                new List<VolumePrice> { new VolumePrice(11100, 10) }, // bids
                DateTime.UtcNow);

            var btcEurOrderBook = new OrderBook("Quoine", "BTCEUR",
                new List<VolumePrice> { new VolumePrice(8999.95m, 10) }, // asks
                new List<VolumePrice> { new VolumePrice(8825, 10) }, // bids
                DateTime.UtcNow);

            var eurUsdOrderBook = new OrderBook("Binance", "EURUSD",
                new List<VolumePrice> { new VolumePrice(1.22033m, 10) }, // asks
                new List<VolumePrice> { new VolumePrice(1.2203m, 10) }, // bids
                DateTime.UtcNow);

            arbitrageDetector.Process(btcUsdOrderBook1);
            arbitrageDetector.Process(btcUsdOrderBook2);
            arbitrageDetector.Process(btcEurOrderBook);
            arbitrageDetector.Process(eurUsdOrderBook);

            await arbitrageDetector.Execute();
            Thread.Sleep(1000); // Wait until cross rate expire and arbitrage appears in history
            await arbitrageDetector.Execute();

            var arbitrageHistory = arbitrageDetector.GetArbitrageHistory(DateTime.MinValue, short.MaxValue);

            Assert.Equal(3, arbitrageHistory.Count());
        }

        private IEnumerable<OrderBook> GenerateOrderBooks(int count, string source, AssetPair assetPair, int askCount, decimal maxAsk, decimal minAsk, int bidCount, decimal maxBid, decimal minBid)
        {
            if (minAsk > maxAsk)
                throw new Exception("minAsk > maxAsk");

            if (minBid > maxBid)
                throw new Exception("minBid > maxBid");

            var result = new List<OrderBook>();

            for (var i = 0; i < count; i++)
            {
                var asks = GenerateVolumePrices(askCount, minAsk, maxAsk);
                var bids = GenerateVolumePrices(bidCount, minBid, maxBid);

                var orderBook = new OrderBook(source + i, assetPair.Name, asks, bids, DateTime.UtcNow);
                orderBook.SetAssetPair(assetPair.Base);

                result.Add(orderBook);
            }

            Assert.Equal(result.Count, count);
            Assert.True(result.TrueForAll(x => x.Source.Contains(source)));
            Assert.True(result.TrueForAll(x => x.AssetPair.Equals(assetPair)));

            return result;
        }

        private IReadOnlyCollection<VolumePrice> GenerateVolumePrices(int count, decimal min, decimal max)
        {
            var result = new List<VolumePrice>();

            var askStep = (max - min) / count;
            for (var i = 0; i < count; i++)
            {
                var ask = new VolumePrice(min + i * askStep, new Random().Next(1, 10));
                result.Add(ask);
            }

            Assert.Equal(result.Count, count);
            Assert.True(result.TrueForAll(x => min <= x.Price && x.Price <= max));

            return result;
        }
    }
}
