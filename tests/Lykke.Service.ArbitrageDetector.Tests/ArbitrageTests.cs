using System;
using System.Collections.Generic;
using System.Linq;
using Lykke.Service.ArbitrageDetector.Core.Domain;
using MoreLinq;
using Xunit;

namespace Lykke.Service.ArbitrageDetector.Tests
{
    public class ArbitrageTests
    {
        [Fact]
        public void ArbitrageVolume_NoArbitrage_EmptyOrderBooks_Test()
        {
            const string exchangeName = "FE";
            const string assetPair = "BTCUSD";
            var timestamp = DateTime.UtcNow;

            var asks = new List<VolumePrice>();
            var bids = new List<VolumePrice>();

            var orderBook1 = new OrderBook(exchangeName, assetPair, bids, asks, timestamp);
            var orderBook2 = new OrderBook(exchangeName, assetPair, bids, asks, timestamp);

            var volume = Arbitrage.GetArbitrageVolume(orderBook1.Bids, orderBook2.Asks);
            Assert.Null(volume);
        }

        [Fact]
        public void ArbitrageVolume_NoArbitrage_TheSameOrderBook_Test()
        {
            const string exchangeName = "FE";
            const string assetPair = "BTCUSD";
            var timestamp = DateTime.UtcNow;

            var asks = new List<VolumePrice>
            {
                new VolumePrice(9000, 10),
                new VolumePrice(8999.95m, 7),
                new VolumePrice(8900.12345677m, 3)
            };
            var bids = new List<VolumePrice>
            {
                new VolumePrice(8825, 9),
                new VolumePrice(8823, 5)
            };

            var orderBook1 = new OrderBook(exchangeName, assetPair, bids, asks, timestamp);
            var orderBook2 = new OrderBook(exchangeName, assetPair, bids, asks, timestamp);

            var volume = Arbitrage.GetArbitrageVolume(orderBook1.Bids, orderBook2.Asks);
            Assert.Null(volume);
        }

        [Fact]
        public void ArbitrageVolume_Simple1_Test()
        {
            const string exchangeName = "FE";
            const string assetPair = "BTCUSD";
            var timestamp = DateTime.UtcNow;
            
            var asks = new List<VolumePrice>
            {
                new VolumePrice(9000, 10),
                new VolumePrice(8999.95m, 7), // <-
                new VolumePrice(8900.12345677m, 3) // <-
            };

            var bids = new List<VolumePrice>
            {
                new VolumePrice(9000, 9), // <-
                new VolumePrice(8900, 5)
            };

            var bidsOrderBook = new OrderBook(exchangeName, assetPair, bids, new List<VolumePrice>(), timestamp);
            var asksOrderBook = new OrderBook(exchangeName, assetPair, new List<VolumePrice>(), asks, timestamp);

            var volume = Arbitrage.GetArbitrageVolume(bidsOrderBook.Bids, asksOrderBook.Asks);
            Assert.Equal(9, volume);
        }

        [Fact]
        public void ArbitrageVolume_Complex1_Test()
        {
            // https://docs.google.com/spreadsheets/d/1plnbQSS-WP6ykTv8wIi_hbAhk_aSz_tllXFIE3jhFpU/edit#gid=0

            const string exchangeName = "FE";
            const string assetPair = "BTCUSD";
            var timestamp = DateTime.UtcNow;

            var asks = new List<VolumePrice>
            {
                new VolumePrice(1000, 10),
                new VolumePrice(950, 10),
                new VolumePrice(850, 10), // <-
                new VolumePrice(800, 10), // <-
                new VolumePrice(700, 10), // <-
                new VolumePrice(650, 10), // <-
                new VolumePrice(600, 10), // <-
                new VolumePrice(550, 1),  // <-
                new VolumePrice(500, 10)  // <-
            };

            var bids = new List<VolumePrice>
            {
                new VolumePrice(900, 5),   // <-
                new VolumePrice(750, 100), // <-
                new VolumePrice(550, 1)    // <-
            };

            var bidsOrderBook = new OrderBook(exchangeName, assetPair, bids, new List<VolumePrice>(), timestamp);
            var asksOrderBook = new OrderBook(exchangeName, assetPair, new List<VolumePrice>(), asks, timestamp);

            var volume = Arbitrage.GetArbitrageVolume(bidsOrderBook.Bids, asksOrderBook.Asks);
            Assert.Equal(41, volume);
        }

        [Fact]
        public void ArbitrageVolume_Complex2_Test()
        {
            // https://docs.google.com/spreadsheets/d/1plnbQSS-WP6ykTv8wIi_hbAhk_aSz_tllXFIE3jhFpU/edit#gid=2011486790
            const string exchangeName = "FE";
            const string assetPair = "BTCUSD";
            var timestamp = DateTime.UtcNow;

            var asks = new List<VolumePrice>
            {
                new VolumePrice(2.9m, 10),
                new VolumePrice(2.8m, 10),
                new VolumePrice(2.7m, 10),
                new VolumePrice(2.6m, 10),
                new VolumePrice(2.4m, 10),
                new VolumePrice(2.3m, 10),
                new VolumePrice(2.2m, 10),
                new VolumePrice(2.0m, 10),
                new VolumePrice(1.9m, 10),
                new VolumePrice(1.8m, 10),
                new VolumePrice(1.7m, 10),
                new VolumePrice(1.3m, 10),
                new VolumePrice(1.2m, 10),
                new VolumePrice(1.1m, 10),
            };

            var bids = new List<VolumePrice>
            {
                new VolumePrice(3.2m, 1),
                new VolumePrice(3.1m, 1),
                new VolumePrice(3m, 1),
                new VolumePrice(2.5m, 1),
                new VolumePrice(2.1m, 100),
                new VolumePrice(1.6m, 5),
                new VolumePrice(1.5m, 5),
                new VolumePrice(1.4m, 5)
            };

            var bidsOrderBook = new OrderBook(exchangeName, assetPair, bids, new List<VolumePrice>(), timestamp);
            var asksOrderBook = new OrderBook(exchangeName, assetPair, new List<VolumePrice>(), asks, timestamp);

            var volume = Arbitrage.GetArbitrageVolume(bidsOrderBook.Bids, asksOrderBook.Asks);
            Assert.Equal(70, volume);
        }


        [Fact]
        public void OrderBooks_ChainedOrderBooks_0_Test()
        {
            var assetPair = new AssetPair("BTC", "USD");
            var orderBook = new OrderBook("FE", assetPair.Name, new List<VolumePrice>(), new List<VolumePrice>(), DateTime.UtcNow);
            orderBook.SetAssetPair(assetPair);
            var crossRate = new CrossRate("FE", assetPair, new List<VolumePrice>(), new List<VolumePrice>(), "None", new List<OrderBook> {orderBook}, DateTime.UtcNow);

            var result = Arbitrage.GetChainedOrderBooks(crossRate, assetPair);
            Assert.Single(result);
            Assert.True(result.Single().AssetPair.Equals(assetPair));
        }

        [Fact]
        public void OrderBooks_ChainedOrderBooks_1_Test()
        {
            var assetPair = new AssetPair("BTC", "USD");
            var reversed = assetPair.Reverse();
            var orderBook = new OrderBook("FE", reversed.Name, new List<VolumePrice>(), new List<VolumePrice>(), DateTime.UtcNow);
            orderBook.SetAssetPair(reversed);
            var crossRate = new CrossRate("FE", assetPair, new List<VolumePrice>(), new List<VolumePrice>(), "None", new List<OrderBook> { orderBook }, DateTime.UtcNow);

            var result = Arbitrage.GetChainedOrderBooks(crossRate, assetPair);
            Assert.Single(result);
            Assert.True(result.Single().AssetPair.Equals(assetPair));
        }


        [Fact]
        public void OrderBooks_ChainedOrderBooks_0_0_Test()
        {
            var target = new AssetPair("BTC", "USD");
            var assetPair1 = new AssetPair("BTC", "EUR");
            var assetPair2 = new AssetPair("EUR", "USD");
            // result = EUR/USD, BTC/EUR

            var crossRate = GetCrossRate(assetPair1, assetPair2, target);

            var result = Arbitrage.GetChainedOrderBooks(crossRate, target);

            AssertChained2(result);
        }

        [Fact]
        public void OrderBooks_ChainedOrderBooks_0_1_Test()
        {
            var target = new AssetPair("BTC", "USD");
            var assetPair1 = new AssetPair("BTC", "EUR");
            var assetPair2 = new AssetPair("USD", "EUR");
            // result = EUR/USD, BTC/EUR

            var crossRate = GetCrossRate(assetPair1, assetPair2, target);

            var result = Arbitrage.GetChainedOrderBooks(crossRate, target);

            AssertChained2(result);
        }

        [Fact]
        public void OrderBooks_ChainedOrderBooks_1_0_Test()
        {
            var target = new AssetPair("BTC", "USD");
            var assetPair1 = new AssetPair("EUR", "BTC");
            var assetPair2 = new AssetPair("EUR", "USD");
            // result = EUR/USD, BTC/EUR

            var crossRate = GetCrossRate(assetPair1, assetPair2, target);

            var result = Arbitrage.GetChainedOrderBooks(crossRate, target);

            AssertChained2(result);
        }

        [Fact]
        public void OrderBooks_ChainedOrderBooks_1_1_Test()
        {
            var target = new AssetPair("BTC", "USD");
            var assetPair1 = new AssetPair("EUR", "BTC");
            var assetPair2 = new AssetPair("USD", "EUR");
            // result = EUR/USD, BTC/EUR

            var crossRate = GetCrossRate(assetPair1, assetPair2, target);

            var result = Arbitrage.GetChainedOrderBooks(crossRate, target);

            AssertChained2(result);
        }

        
        [Fact]
        public void OrderBooks_ChainedOrderBooks_0_0_0_Test()
        {
            var target = new AssetPair("BTC", "USD");
            var assetPair1 = new AssetPair("BTC", "EUR");
            var assetPair2 = new AssetPair("EUR", "CHF");
            var assetPair3 = new AssetPair("CHF", "USD");
            // result = CHF/USD, EUR/CHF, BTC/EUR

            var crossRate = GetCrossRate(assetPair1, assetPair2, assetPair3, target);

            var result = Arbitrage.GetChainedOrderBooks(crossRate, target);

            AssertChained3(result);
        }

        [Fact]
        public void OrderBooks_ChainedOrderBooks_0_0_1_Test()
        {
            var target = new AssetPair("BTC", "USD");
            var assetPair1 = new AssetPair("BTC", "EUR");
            var assetPair2 = new AssetPair("EUR", "CHF");
            var assetPair3 = new AssetPair("USD", "CHF");
            // result = CHF/USD, EUR/CHF, BTC/EUR

            var crossRate = GetCrossRate(assetPair1, assetPair2, assetPair3, target);

            var result = Arbitrage.GetChainedOrderBooks(crossRate, target);

            AssertChained3(result);
        }

        [Fact]
        public void OrderBooks_ChainedOrderBooks_0_1_0_Test()
        {
            var target = new AssetPair("BTC", "USD");
            var assetPair1 = new AssetPair("BTC", "EUR");
            var assetPair2 = new AssetPair("CHF", "EUR");
            var assetPair3 = new AssetPair("CHF", "USD");
            // result = CHF/USD, EUR/CHF, BTC/EUR

            var crossRate = GetCrossRate(assetPair1, assetPair2, assetPair3, target);

            var result = Arbitrage.GetChainedOrderBooks(crossRate, target);

            AssertChained3(result);
        }

        [Fact]
        public void OrderBooks_ChainedOrderBooks_1_0_0_Test()
        {
            var target = new AssetPair("BTC", "USD");
            var assetPair1 = new AssetPair("EUR", "BTC");
            var assetPair2 = new AssetPair("EUR", "CHF");
            var assetPair3 = new AssetPair("CHF", "USD");
            // result = CHF/USD, EUR/CHF, BTC/EUR

            var crossRate = GetCrossRate(assetPair1, assetPair2, assetPair3, target);

            var result = Arbitrage.GetChainedOrderBooks(crossRate, target);

            AssertChained3(result);
        }

        [Fact]
        public void OrderBooks_ChainedOrderBooks_0_1_1_Test()
        {
            var target = new AssetPair("BTC", "USD");
            var assetPair1 = new AssetPair("BTC", "EUR");
            var assetPair2 = new AssetPair("CHF", "EUR");
            var assetPair3 = new AssetPair("USD", "CHF");
            // result = CHF/USD, EUR/CHF, BTC/EUR

            var crossRate = GetCrossRate(assetPair1, assetPair2, assetPair3, target);

            var result = Arbitrage.GetChainedOrderBooks(crossRate, target);

            AssertChained3(result);
        }

        [Fact]
        public void OrderBooks_ChainedOrderBooks_1_1_0_Test()
        {
            var target = new AssetPair("BTC", "USD");
            var assetPair1 = new AssetPair("EUR", "BTC");
            var assetPair2 = new AssetPair("CHF", "EUR");
            var assetPair3 = new AssetPair("CHF", "USD");
            // result = CHF/USD, EUR/CHF, BTC/EUR

            var crossRate = GetCrossRate(assetPair1, assetPair2, assetPair3, target);

            var result = Arbitrage.GetChainedOrderBooks(crossRate, target);

            AssertChained3(result);
        }

        [Fact]
        public void OrderBooks_ChainedOrderBooks_1_0_1_Test()
        {
            var target = new AssetPair("BTC", "USD");
            var assetPair1 = new AssetPair("EUR", "BTC");
            var assetPair2 = new AssetPair("EUR", "CHF");
            var assetPair3 = new AssetPair("USD", "CHF");
            // result = CHF/USD, EUR/CHF, BTC/EUR

            var crossRate = GetCrossRate(assetPair1, assetPair2, assetPair3, target);

            var result = Arbitrage.GetChainedOrderBooks(crossRate, target);

            AssertChained3(result);
        }

        [Fact]
        public void OrderBooks_ChainedOrderBooks_1_1_1_Test()
        {
            var target = new AssetPair("BTC", "USD");
            var assetPair1 = new AssetPair("EUR", "BTC");
            var assetPair2 = new AssetPair("CHF", "EUR");
            var assetPair3 = new AssetPair("USD", "CHF");
            // result = CHF/USD, EUR/CHF, BTC/EUR

            var crossRate = GetCrossRate(assetPair1, assetPair2, assetPair3, target);

            var result = Arbitrage.GetChainedOrderBooks(crossRate, target);

            AssertChained3(result);
        }


        private CrossRate GetCrossRate(AssetPair assetPair1, AssetPair assetPair2, AssetPair target)
        {
            var orderBook1 = new OrderBook("FE", assetPair1.Name, new List<VolumePrice>(), new List<VolumePrice>(), DateTime.UtcNow);
            orderBook1.SetAssetPair(assetPair1);
            var orderBook2 = new OrderBook("FE", assetPair2.Name, new List<VolumePrice>(), new List<VolumePrice>(), DateTime.UtcNow);
            orderBook2.SetAssetPair(assetPair2);

            return new CrossRate("FE", target, new List<VolumePrice>(), new List<VolumePrice>(), "None", new List<OrderBook> { orderBook1, orderBook2 }, DateTime.UtcNow);
        }

        private CrossRate GetCrossRate(AssetPair assetPair1, AssetPair assetPair2, AssetPair assetPair3, AssetPair target)
        {
            var orderBook1 = new OrderBook("FE", assetPair1.Name, new List<VolumePrice>(), new List<VolumePrice>(), DateTime.UtcNow);
            orderBook1.SetAssetPair(assetPair1);
            var orderBook2 = new OrderBook("FE", assetPair2.Name, new List<VolumePrice>(), new List<VolumePrice>(), DateTime.UtcNow);
            orderBook2.SetAssetPair(assetPair2);
            var orderBook3 = new OrderBook("FE", assetPair3.Name, new List<VolumePrice>(), new List<VolumePrice>(), DateTime.UtcNow);
            orderBook3.SetAssetPair(assetPair3);

            return new CrossRate("FE", target, new List<VolumePrice>(), new List<VolumePrice>(), "None", new List<OrderBook> { orderBook1, orderBook2, orderBook3 }, DateTime.UtcNow);
        }

        private void AssertChained2(IReadOnlyCollection<OrderBook> result)
        {
            Assert.Equal(2, result.Count);
            Assert.Equal("EUR", result.ElementAt(0).AssetPair.Base);
            Assert.Equal("USD", result.ElementAt(0).AssetPair.Quote);
            Assert.Equal("BTC", result.ElementAt(1).AssetPair.Base);
            Assert.Equal("EUR", result.ElementAt(1).AssetPair.Quote);
        }

        private void AssertChained3(IReadOnlyCollection<OrderBook> result)
        {
            Assert.Equal(3, result.Count);
            Assert.Equal("CHF", result.ElementAt(0).AssetPair.Base);
            Assert.Equal("USD", result.ElementAt(0).AssetPair.Quote);
            Assert.Equal("EUR", result.ElementAt(1).AssetPair.Base);
            Assert.Equal("CHF", result.ElementAt(1).AssetPair.Quote);
            Assert.Equal("BTC", result.ElementAt(2).AssetPair.Base);
            Assert.Equal("EUR", result.ElementAt(2).AssetPair.Quote);
        }
    }
}
