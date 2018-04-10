using System;
using System.Collections.Generic;
using Lykke.Service.ArbitrageDetector.Core.Domain;
using Xunit;

namespace Lykke.Service.ArbitrageDetector.Tests
{
    public class CrossRateTests
    {
        [Fact]
        public void ConstructorTest()
        {
            const string exchangeName = "FakeExchange";
            const string conversionPath = "FakeExchange-BTCUSD";
            var assetPair = new AssetPair("BTC", "USD");
            var timestamp = DateTime.UtcNow;

            var bids = new List<VolumePrice>
            {
                new VolumePrice(8825, 9),
                new VolumePrice(8823, 5)
            };
            var asks = new List<VolumePrice>
            {
                new VolumePrice(9000, 10),
                new VolumePrice(8999.95m, 7),
                new VolumePrice(8900.12345677m, 3)
            };

            void Construct1() => new CrossRate("", assetPair, bids, asks, conversionPath, new List<OrderBook>(), timestamp);
            Assert.Throws<ArgumentException>((Action)Construct1);

            void Construct2() => new CrossRate(null, assetPair, bids, asks, conversionPath, new List<OrderBook>(), timestamp);
            Assert.Throws<ArgumentException>((Action)Construct2);

            void Construct3() => new CrossRate(exchangeName, assetPair, bids, asks, "", new List<OrderBook>(), timestamp);
            Assert.Throws<ArgumentException>((Action)Construct3);

            void Construct4() => new CrossRate(exchangeName, assetPair, bids, asks, null, new List<OrderBook>(), timestamp);
            Assert.Throws<ArgumentException>((Action)Construct4);

            void Construct5() => new CrossRate(exchangeName, assetPair, bids, asks, conversionPath, null, timestamp);
            Assert.Throws<ArgumentNullException>((Action)Construct5);
        }

        [Fact]
        public void FromOrderBookExceptionTest()
        {
            const string exchange = "FakeExchange";
            const string btceur = "BTCEUR";
            var timestamp = DateTime.UtcNow;
            var assetPair = new AssetPair("BTC", "EUR");

            var btcEurOrderBook = new OrderBook(exchange, btceur, new List<VolumePrice>(), new List<VolumePrice>(), timestamp);

            // AssetPair is not set in order book
            void FromOrderBook() => CrossRate.FromOrderBook(btcEurOrderBook, assetPair);
            Assert.Throws<ArgumentException>((Action) FromOrderBook);

            btcEurOrderBook.SetAssetPair("EUR");

            void FromOrderBook2() => CrossRate.FromOrderBook(btcEurOrderBook, new AssetPair());
            Assert.Throws<ArgumentException>((Action) FromOrderBook2);
        }

        [Fact]
        public void FromOrderBookStreightTest()
        {
            const string exchange = "FakeExchange";
            const string btceur = "BTCEUR";
            var timestamp = DateTime.UtcNow;
            var assetPair = new AssetPair("BTC", "EUR");

            var btcEurOrderBook = new OrderBook(exchange, btceur,
                new List<VolumePrice> // bids
                {
                    new VolumePrice(8825, 9), new VolumePrice(8823, 5)
                },
                new List<VolumePrice> // asks
                {
                    new VolumePrice(9000, 10), new VolumePrice(8999.95m, 7), new VolumePrice(8900.12345677m, 3)
                },
                timestamp);
            btcEurOrderBook.SetAssetPair("EUR");

            var crossRate = CrossRate.FromOrderBook(btcEurOrderBook, assetPair);
            Assert.Equal(exchange, crossRate.Source);
            Assert.Equal(btceur, crossRate.AssetPairStr);
            Assert.Equal(assetPair, crossRate.AssetPair);
            Assert.Equal(OrderBook.FormatSourceAssetPair(exchange, btceur), crossRate.ConversionPath);
            Assert.Equal(2, crossRate.Bids.Count);
            Assert.Equal(3, crossRate.Asks.Count);
            Assert.Equal(timestamp, crossRate.Timestamp);
            Assert.Equal(1, crossRate.OriginalOrderBooks.Count);
        }

        [Fact]
        public void FromOrderBookReversedTest()
        {
            const string exchange = "FakeExchange";
            const string btcusd = "BTCUSD";
            var timestamp = DateTime.UtcNow;
            var assetPair = new AssetPair("BTC", "USD");
            var reversed = assetPair.Reverse();

            var btcUsdOrderBook = new OrderBook(exchange, btcusd,
                new List<VolumePrice> // bids
                {
                    new VolumePrice(8825, 9), new VolumePrice(8823, 5)
                },
                new List<VolumePrice> // asks
                {
                    new VolumePrice(9000, 10), new VolumePrice(8999.95m, 7), new VolumePrice(8900.12345677m, 3)
                },
                timestamp);
            btcUsdOrderBook.SetAssetPair("USD");

            var crossRate = CrossRate.FromOrderBook(btcUsdOrderBook, reversed);
            Assert.Equal(exchange, crossRate.Source);
            Assert.Equal(reversed.Name, crossRate.AssetPairStr);
            Assert.Equal(reversed, crossRate.AssetPair);
            Assert.Equal(OrderBook.FormatSourceAssetPair(exchange, btcusd), crossRate.ConversionPath);
            Assert.Equal(3, crossRate.Bids.Count);
            Assert.Equal(2, crossRate.Asks.Count);
            Assert.Equal(timestamp, crossRate.Timestamp);
            Assert.Equal(1, crossRate.OriginalOrderBooks.Count);
        }

        [Fact]
        public void FromOrderBooksExceptionTest()
        {   
            const string exchange = "FakeExchange";
            var timestamp = DateTime.UtcNow;
            var target = new AssetPair("BTC", "USD");

            var btcEurOrderBook = new OrderBook(exchange, "BTCEUR", new List<VolumePrice>(), new List<VolumePrice>(), timestamp);
            var eurUsdOrderBook = new OrderBook(exchange, "EURUSD", new List<VolumePrice>(), new List<VolumePrice>(), timestamp);

            // AssetPair is not set in order book
            void FromOrderBook() => CrossRate.FromOrderBooks(btcEurOrderBook, eurUsdOrderBook, target);
            Assert.Throws<ArgumentException>((Action)FromOrderBook);

            btcEurOrderBook.SetAssetPair("EUR");

            void FromOrderBook2() => CrossRate.FromOrderBooks(btcEurOrderBook, eurUsdOrderBook, target);
            Assert.Throws<ArgumentException>((Action)FromOrderBook2);

            eurUsdOrderBook.SetAssetPair("EUR");

            void FromOrderBook4() => CrossRate.FromOrderBooks(null, eurUsdOrderBook, target);
            Assert.Throws<ArgumentNullException>((Action)FromOrderBook4);

            void FromOrderBook5() => CrossRate.FromOrderBooks(btcEurOrderBook, null, target);
            Assert.Throws<ArgumentNullException>((Action)FromOrderBook5);

            void FromOrderBook6() => CrossRate.FromOrderBooks(btcEurOrderBook, eurUsdOrderBook, new AssetPair());
            Assert.Throws<ArgumentException>((Action)FromOrderBook6);
        }

        [Fact]
        public void FromOrderBooksStreightTest()
        {
            const string exchange = "FakeExchange";
            const string btceur = "BTCEUR";
            const string eurusd = "EURUSD";
            var timestamp1 = DateTime.UtcNow.AddSeconds(-1);
            var timestamp2 = DateTime.UtcNow;
            var targetAssetPair = new AssetPair("BTC", "USD");

            var btcEurOrderBook = new OrderBook(exchange, btceur,
                new List<VolumePrice> // bids
                {
                    new VolumePrice(8825, 9), new VolumePrice(8823, 5)
                },
                new List<VolumePrice> // asks
                {
                    new VolumePrice(9000, 10), new VolumePrice(8999.95m, 7), new VolumePrice(8900.12345677m, 3)
                },
                timestamp1);
            btcEurOrderBook.SetAssetPair("EUR");

            var eurUsdOrderBook = new OrderBook(exchange, eurusd,
                new List<VolumePrice> // bids
                {
                    new VolumePrice(8825, 9), new VolumePrice(8823, 5)
                },
                new List<VolumePrice> // asks
                {
                    new VolumePrice(9000, 10), new VolumePrice(8999.95m, 7), new VolumePrice(8900.12345677m, 3)
                },
                timestamp2);
            eurUsdOrderBook.SetAssetPair("EUR");

            var crossRate = CrossRate.FromOrderBooks(btcEurOrderBook, eurUsdOrderBook, targetAssetPair);
            Assert.Equal(CrossRate.GetSourcesPath(exchange, exchange), crossRate.Source);
            Assert.Equal(targetAssetPair.Name, crossRate.AssetPairStr);
            Assert.Equal(targetAssetPair, crossRate.AssetPair);
            Assert.Equal(CrossRate.GetConversionPath(exchange, btceur, exchange, eurusd), crossRate.ConversionPath);
            Assert.Equal(4, crossRate.Bids.Count);
            Assert.Equal(9, crossRate.Asks.Count);
            Assert.Equal(timestamp1, crossRate.Timestamp);
            Assert.Equal(2, crossRate.OriginalOrderBooks.Count);
        }

        [Fact]
        public void FromOrderBooksReverseTest()
        {
            const string exchange = "FakeExchange";
            const string eurbtc = "EURBTC";
            const string eurusd = "EURUSD";
            var timestamp1 = DateTime.UtcNow.AddSeconds(-1);
            var timestamp2 = DateTime.UtcNow;
            var targetAssetPair = new AssetPair("BTC", "USD");

            var btcEurOrderBook = new OrderBook(exchange, eurbtc,
                new List<VolumePrice> // bids
                {
                    new VolumePrice(8825, 9), new VolumePrice(8823, 5)
                },
                new List<VolumePrice> // asks
                {
                    new VolumePrice(9000, 10), new VolumePrice(8999.95m, 7), new VolumePrice(8900.12345677m, 3)
                },
                timestamp1);
            btcEurOrderBook.SetAssetPair("EUR");

            var eurUsdOrderBook = new OrderBook(exchange, eurusd,
                new List<VolumePrice> // bids
                {
                    new VolumePrice(8825, 9), new VolumePrice(8823, 5)
                },
                new List<VolumePrice> // asks
                {
                    new VolumePrice(9000, 10), new VolumePrice(8999.95m, 7), new VolumePrice(8900.12345677m, 3)
                },
                timestamp2);
            eurUsdOrderBook.SetAssetPair("EUR");

            var crossRate = CrossRate.FromOrderBooks(btcEurOrderBook, eurUsdOrderBook, targetAssetPair);
            Assert.Equal(CrossRate.GetSourcesPath(exchange, exchange), crossRate.Source);
            Assert.Equal(targetAssetPair.Name, crossRate.AssetPairStr);
            Assert.Equal(targetAssetPair, crossRate.AssetPair);
            Assert.Equal(CrossRate.GetConversionPath(exchange, eurbtc, exchange, eurusd), crossRate.ConversionPath);
            Assert.Equal(6, crossRate.Bids.Count);
            Assert.Equal(6, crossRate.Asks.Count);
            Assert.Equal(timestamp1, crossRate.Timestamp);
            Assert.Equal(2, crossRate.OriginalOrderBooks.Count);
        }

        [Fact]
        public void FromOrderBooksReverse2Test()
        {
            const string exchange = "FakeExchange";
            const string btceur = "BTCEUR";
            const string usdeur = "USDEUR";
            var timestamp1 = DateTime.UtcNow.AddSeconds(-1);
            var timestamp2 = DateTime.UtcNow;
            var targetAssetPair = new AssetPair("BTC", "USD");

            var btcEurOrderBook = new OrderBook(exchange, btceur,
                new List<VolumePrice> // bids
                {
                    new VolumePrice(8825, 9), new VolumePrice(8823, 5)
                },
                new List<VolumePrice> // asks
                {
                    new VolumePrice(9000, 10), new VolumePrice(8999.95m, 7), new VolumePrice(8900.12345677m, 3)
                },
                timestamp1);
            btcEurOrderBook.SetAssetPair("EUR");

            var eurUsdOrderBook = new OrderBook(exchange, usdeur,
                new List<VolumePrice> // bids
                {
                    new VolumePrice(8825, 9), new VolumePrice(8823, 5)
                },
                new List<VolumePrice> // asks
                {
                    new VolumePrice(9000, 10), new VolumePrice(8999.95m, 7), new VolumePrice(8900.12345677m, 3)
                },
                timestamp2);
            eurUsdOrderBook.SetAssetPair("EUR");

            var crossRate = CrossRate.FromOrderBooks(btcEurOrderBook, eurUsdOrderBook, targetAssetPair);
            Assert.Equal(CrossRate.GetSourcesPath(exchange,exchange), crossRate.Source);
            Assert.Equal(targetAssetPair.Name, crossRate.AssetPairStr);
            Assert.Equal(targetAssetPair, crossRate.AssetPair);
            Assert.Equal(CrossRate.GetConversionPath(exchange, btceur, exchange, usdeur), crossRate.ConversionPath);
            Assert.Equal(6, crossRate.Bids.Count);
            Assert.Equal(6, crossRate.Asks.Count);
            Assert.Equal(timestamp1, crossRate.Timestamp);
            Assert.Equal(2, crossRate.OriginalOrderBooks.Count);
        }

        [Fact]
        public void FromOrderBooksReverseDoubleTest()
        {
            const string exchange = "FakeExchange";
            const string eurbtc = "EURBTC";
            const string usdeur = "USDEUR";
            var timestamp1 = DateTime.UtcNow.AddSeconds(-1);
            var timestamp2 = DateTime.UtcNow;
            var targetAssetPair = new AssetPair("BTC", "USD");

            var btcEurOrderBook = new OrderBook(exchange, eurbtc,
                new List<VolumePrice> // bids
                {
                    new VolumePrice(8825, 9), new VolumePrice(8823, 5)
                },
                new List<VolumePrice> // asks
                {
                    new VolumePrice(9000, 10), new VolumePrice(8999.95m, 7), new VolumePrice(8900.12345677m, 3)
                },
                timestamp1);
            btcEurOrderBook.SetAssetPair("EUR");

            var eurUsdOrderBook = new OrderBook(exchange, usdeur,
                new List<VolumePrice> // bids
                {
                    new VolumePrice(8825, 9), new VolumePrice(8823, 5)
                },
                new List<VolumePrice> // asks
                {
                    new VolumePrice(9000, 10), new VolumePrice(8999.95m, 7), new VolumePrice(8900.12345677m, 3)
                },
                timestamp2);
            eurUsdOrderBook.SetAssetPair("EUR");

            var crossRate = CrossRate.FromOrderBooks(btcEurOrderBook, eurUsdOrderBook, targetAssetPair);
            Assert.Equal(CrossRate.GetSourcesPath(exchange, exchange), crossRate.Source);
            Assert.Equal(targetAssetPair.Name, crossRate.AssetPairStr);
            Assert.Equal(targetAssetPair, crossRate.AssetPair);
            Assert.Equal(CrossRate.GetConversionPath(exchange, eurbtc, exchange, usdeur), crossRate.ConversionPath);
            Assert.Equal(9, crossRate.Bids.Count);
            Assert.Equal(4, crossRate.Asks.Count);
            Assert.Equal(timestamp1, crossRate.Timestamp);
            Assert.Equal(2, crossRate.OriginalOrderBooks.Count);
        }
    }
}
