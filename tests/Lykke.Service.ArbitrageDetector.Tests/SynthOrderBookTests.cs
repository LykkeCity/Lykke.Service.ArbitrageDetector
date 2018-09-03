using System;
using System.Collections.Generic;
using System.Linq;
using Lykke.Service.ArbitrageDetector.Core.Domain;
using Xunit;

namespace Lykke.Service.ArbitrageDetector.Tests
{
    public class SynthOrderBookTests
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

            void Construct1() => new SynthOrderBook("", assetPair, bids, asks, conversionPath, new List<OrderBook>(), timestamp);
            Assert.Throws<ArgumentException>((Action)Construct1);

            void Construct2() => new SynthOrderBook(null, assetPair, bids, asks, conversionPath, new List<OrderBook>(), timestamp);
            Assert.Throws<ArgumentException>((Action)Construct2);

            void Construct3() => new SynthOrderBook(exchangeName, assetPair, bids, asks, "", new List<OrderBook>(), timestamp);
            Assert.Throws<ArgumentException>((Action)Construct3);

            void Construct4() => new SynthOrderBook(exchangeName, assetPair, bids, asks, null, new List<OrderBook>(), timestamp);
            Assert.Throws<ArgumentException>((Action)Construct4);

            void Construct5() => new SynthOrderBook(exchangeName, assetPair, bids, asks, conversionPath, null, timestamp);
            Assert.Throws<ArgumentNullException>((Action)Construct5);
        }

        [Fact]
        public void FromOrderBookExceptionTest()
        {
            const string exchange = "FakeExchange";
            var timestamp = DateTime.UtcNow;
            var assetPair = new AssetPair("BTC", "EUR");

            var orderBookWithEmptyAssetPair = new OrderBook(exchange, "BTCEUR", new List<VolumePrice>(), new List<VolumePrice>(), timestamp);

            // AssetPair is not set in order book
            Assert.Throws<ArgumentException>(() => SynthOrderBook.FromOrderBook(orderBookWithEmptyAssetPair, assetPair));

            orderBookWithEmptyAssetPair.SetAssetPair("EUR");

            Assert.Throws<ArgumentException>(() => SynthOrderBook.FromOrderBook(orderBookWithEmptyAssetPair, new AssetPair()));
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

            var synthOrderBook = SynthOrderBook.FromOrderBook(btcEurOrderBook, assetPair);
            Assert.Equal(exchange, synthOrderBook.Source);
            Assert.Equal(btceur, synthOrderBook.AssetPairStr);
            Assert.Equal(assetPair, synthOrderBook.AssetPair);
            Assert.Equal(OrderBook.FormatSourceAssetPair(exchange, btceur), synthOrderBook.ConversionPath);
            Assert.Equal(2, synthOrderBook.Bids.Count);
            Assert.Equal(3, synthOrderBook.Asks.Count);
            Assert.Equal(8825m, synthOrderBook.Bids.Max(x => x.Price));
            Assert.Equal(9000m, synthOrderBook.Asks.Max(x => x.Price));
            Assert.Equal(8823m, synthOrderBook.Bids.Min(x => x.Price));
            Assert.Equal(8900.12345677m, synthOrderBook.Asks.Min(x => x.Price));
            Assert.Equal(9, synthOrderBook.Bids.Max(x => x.Volume));
            Assert.Equal(10, synthOrderBook.Asks.Max(x => x.Volume));
            Assert.Equal(5, synthOrderBook.Bids.Min(x => x.Volume));
            Assert.Equal(3, synthOrderBook.Asks.Min(x => x.Volume));
            Assert.Equal(timestamp, synthOrderBook.Timestamp);
            Assert.Equal(1, synthOrderBook.OriginalOrderBooks.Count);
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
                    new VolumePrice(1/8825m, 9), new VolumePrice(1/8823m, 5)
                },
                new List<VolumePrice> // asks
                {
                    new VolumePrice(1/9000m, 10), new VolumePrice(1/8999.95m, 7), new VolumePrice(1/8900.12345677m, 3)
                },
                timestamp);
            btcUsdOrderBook.SetAssetPair("USD");

            var synthOrderBook = SynthOrderBook.FromOrderBook(btcUsdOrderBook, reversed);
            Assert.Equal(exchange, synthOrderBook.Source);
            Assert.Equal(reversed.Name, synthOrderBook.AssetPairStr);
            Assert.Equal(reversed, synthOrderBook.AssetPair);
            Assert.Equal(OrderBook.FormatSourceAssetPair(exchange, btcusd), synthOrderBook.ConversionPath);
            Assert.Equal(3, synthOrderBook.Bids.Count);
            Assert.Equal(2, synthOrderBook.Asks.Count);
            Assert.Equal(9000m, synthOrderBook.Bids.Max(x => x.Price), 8);
            Assert.Equal(8825m, synthOrderBook.Asks.Max(x => x.Price), 8);
            Assert.Equal(8900.12345677m, synthOrderBook.Bids.Min(x => x.Price), 8);
            Assert.Equal(8823m, synthOrderBook.Asks.Min(x => x.Price), 8);
            Assert.Equal(0.00111111m, synthOrderBook.Bids.Max(x => x.Volume), 8);
            Assert.Equal(0.00101983m, synthOrderBook.Asks.Max(x => x.Volume), 8);
            Assert.Equal(0.00033707m, synthOrderBook.Bids.Min(x => x.Volume), 8);
            Assert.Equal(0.00056670m, synthOrderBook.Asks.Min(x => x.Volume), 8);
            Assert.Equal(timestamp, synthOrderBook.Timestamp);
            Assert.Equal(1, synthOrderBook.OriginalOrderBooks.Count);
        }


        [Fact]
        public void From2OrderBooksExceptionTest()
        {   
            const string exchange = "FakeExchange";
            var timestamp = DateTime.UtcNow;
            var target = new AssetPair("BTC", "USD");

            var orderBookWithEmptyAssetPair = new OrderBook(exchange, "BTCEUR", new List<VolumePrice>(), new List<VolumePrice>(), timestamp);
            var goodOrderBook = new OrderBook(exchange, "BTCCHF", new List<VolumePrice>(), new List<VolumePrice>(), timestamp);

            // AssetPair is not set in order book
            Assert.Throws<ArgumentException>(() => SynthOrderBook.FromOrderBooks(orderBookWithEmptyAssetPair, goodOrderBook, target));
            Assert.Throws<ArgumentException>(() => SynthOrderBook.FromOrderBooks(goodOrderBook, orderBookWithEmptyAssetPair, target));

            orderBookWithEmptyAssetPair.SetAssetPair("EUR");

            Assert.Throws<ArgumentException>(() => SynthOrderBook.FromOrderBooks(goodOrderBook, orderBookWithEmptyAssetPair, new AssetPair()));
        }

        [Fact]
        public void From2OrderBooksReversed_0_0_Test()
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
                    new VolumePrice(8825, 9),
                    new VolumePrice(8823, 5)
                },
                new List<VolumePrice> // asks
                {
                    new VolumePrice(9000, 10),
                    new VolumePrice(8999.95m, 7),
                    new VolumePrice(8900.12345677m, 3)
                },
                timestamp1);
            btcEurOrderBook.SetAssetPair("EUR");

            var eurUsdOrderBook = new OrderBook(exchange, eurusd,
                new List<VolumePrice> // bids
                {
                    new VolumePrice(1.11m, 9),
                    new VolumePrice(1.10m, 5)
                },
                new List<VolumePrice> // asks
                {
                    new VolumePrice(1.12m, 10),
                    new VolumePrice(1.13m, 7),
                    new VolumePrice(1.14m, 3)
                },
                timestamp2);
            eurUsdOrderBook.SetAssetPair("EUR");

            var synthOrderBook = SynthOrderBook.FromOrderBooks(btcEurOrderBook, eurUsdOrderBook, targetAssetPair);
            Assert.Equal(SynthOrderBook.GetSourcesPath(exchange, exchange), synthOrderBook.Source);
            Assert.Equal(targetAssetPair.Name, synthOrderBook.AssetPairStr);
            Assert.Equal(targetAssetPair, synthOrderBook.AssetPair);
            Assert.Equal(SynthOrderBook.GetConversionPath(exchange, btceur, exchange, eurusd), synthOrderBook.ConversionPath);
            Assert.Equal(4, synthOrderBook.Bids.Count);
            Assert.Equal(9, synthOrderBook.Asks.Count);
            Assert.Equal(9795.75m, synthOrderBook.Bids.Max(x => x.Price), 8);
            Assert.Equal(10260m, synthOrderBook.Asks.Max(x => x.Price), 8);
            Assert.Equal(9705.3m, synthOrderBook.Bids.Min(x => x.Price), 8);
            Assert.Equal(9968.1382715824m, synthOrderBook.Asks.Min(x => x.Price), 8);
            Assert.Equal(0.00102006m, synthOrderBook.Bids.Max(x => x.Volume), 8);
            Assert.Equal(0.00112358m, synthOrderBook.Asks.Max(x => x.Volume), 8);
            Assert.Equal(0.00056657m, synthOrderBook.Bids.Min(x => x.Volume), 8);
            Assert.Equal(0.00033333m, synthOrderBook.Asks.Min(x => x.Volume), 8);
            Assert.Equal(timestamp1, synthOrderBook.Timestamp);
            Assert.Equal(2, synthOrderBook.OriginalOrderBooks.Count);
        }

        [Fact]
        public void From2OrderBooksReversed_0_1_Test()
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
                    new VolumePrice(8825, 9),
                    new VolumePrice(8823, 5)
                },
                new List<VolumePrice> // asks
                {
                    new VolumePrice(9000, 10),
                    new VolumePrice(8999.95m, 7),
                    new VolumePrice(8900.12345677m, 3)
                },
                timestamp1);
            btcEurOrderBook.SetAssetPair("EUR");

            var eurUsdOrderBook = new OrderBook(exchange, usdeur,
                new List<VolumePrice> // bids
                {
                    new VolumePrice(1/1.11m, 9),
                    new VolumePrice(1/1.10m, 5)
                },
                new List<VolumePrice> // asks
                {
                    new VolumePrice(1/1.12m, 10),
                    new VolumePrice(1/1.13m, 7),
                    new VolumePrice(1/1.14m, 3)
                },
                timestamp2);
            eurUsdOrderBook.SetAssetPair("EUR");

            var synthOrderBook = SynthOrderBook.FromOrderBooks(btcEurOrderBook, eurUsdOrderBook, targetAssetPair);
            Assert.Equal(SynthOrderBook.GetSourcesPath(exchange, exchange), synthOrderBook.Source);
            Assert.Equal(targetAssetPair.Name, synthOrderBook.AssetPairStr);
            Assert.Equal(targetAssetPair, synthOrderBook.AssetPair);
            Assert.Equal(SynthOrderBook.GetConversionPath(exchange, btceur, exchange, usdeur), synthOrderBook.ConversionPath);
            Assert.Equal(6, synthOrderBook.Bids.Count);
            Assert.Equal(6, synthOrderBook.Asks.Count);
            Assert.Equal(10060.5m, synthOrderBook.Bids.Max(x => x.Price), 8);
            Assert.Equal(9990m, synthOrderBook.Asks.Max(x => x.Price), 8);
            Assert.Equal(9881.76m, synthOrderBook.Bids.Min(x => x.Price), 8);
            Assert.Equal(9790.135802447m, synthOrderBook.Asks.Min(x => x.Price), 8);
            Assert.Equal(0.00101197m, synthOrderBook.Bids.Max(x => x.Volume), 8);
            Assert.Equal(0.00091101m, synthOrderBook.Asks.Max(x => x.Volume), 8);
            Assert.Equal(0.00029820m, synthOrderBook.Bids.Min(x => x.Volume), 8);
            Assert.Equal(0.00050505m, synthOrderBook.Asks.Min(x => x.Volume), 8);
            Assert.Equal(timestamp1, synthOrderBook.Timestamp);
            Assert.Equal(2, synthOrderBook.OriginalOrderBooks.Count);
        }

        [Fact]
        public void From2OrderBooksReversed_1_0_Test()
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
                    new VolumePrice(1/8825m, 9),
                    new VolumePrice(1/8823m, 5)
                },
                new List<VolumePrice> // asks
                {
                    new VolumePrice(1/9000m, 10),
                    new VolumePrice(1/8999.95m, 7),
                    new VolumePrice(1/8900.12345677m, 3)
                },
                timestamp1);
            btcEurOrderBook.SetAssetPair("EUR");

            var eurUsdOrderBook = new OrderBook(exchange, eurusd,
                new List<VolumePrice> // bids
                {
                    new VolumePrice(1.11m, 9),
                    new VolumePrice(1.10m, 5)
                },
                new List<VolumePrice> // asks
                {
                    new VolumePrice(1.12m, 10),
                    new VolumePrice(1.13m, 7),
                    new VolumePrice(1.14m, 3)
                },
                timestamp2);
            eurUsdOrderBook.SetAssetPair("EUR");

            var synthOrderBook = SynthOrderBook.FromOrderBooks(btcEurOrderBook, eurUsdOrderBook, targetAssetPair);
            Assert.Equal(SynthOrderBook.GetSourcesPath(exchange, exchange), synthOrderBook.Source);
            Assert.Equal(targetAssetPair.Name, synthOrderBook.AssetPairStr);
            Assert.Equal(targetAssetPair, synthOrderBook.AssetPair);
            Assert.Equal(SynthOrderBook.GetConversionPath(exchange, eurbtc, exchange, eurusd), synthOrderBook.ConversionPath);
            Assert.Equal(6, synthOrderBook.Bids.Count);
            Assert.Equal(6, synthOrderBook.Asks.Count);
            Assert.Equal(9990m, synthOrderBook.Bids.Max(x => x.Price), 8);
            Assert.Equal(10060.5m, synthOrderBook.Asks.Max(x => x.Price), 8);
            Assert.Equal(9790.13580245m, synthOrderBook.Bids.Min(x => x.Price), 8);
            Assert.Equal(9881.76m, synthOrderBook.Asks.Min(x => x.Price), 8);
            Assert.Equal(0.001m, synthOrderBook.Bids.Max(x => x.Volume), 8);
            Assert.Equal(0.00101983m, synthOrderBook.Asks.Max(x => x.Volume), 8);
            Assert.Equal(0.00033707m, synthOrderBook.Bids.Min(x => x.Volume), 8);
            Assert.Equal(0.00033994m, synthOrderBook.Asks.Min(x => x.Volume), 8);
            Assert.Equal(timestamp1, synthOrderBook.Timestamp);
            Assert.Equal(2, synthOrderBook.OriginalOrderBooks.Count);
        }

        [Fact]
        public void From2OrderBooksReversed_1_1_Test()
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
                    new VolumePrice(1/8825m, 9),
                    new VolumePrice(1/8823m, 5)
                },
                new List<VolumePrice> // asks
                {
                    new VolumePrice(1/9000m, 10),
                    new VolumePrice(1/8999.95m, 7),
                    new VolumePrice(1/8900.12345677m, 3)
                },
                timestamp1);
            btcEurOrderBook.SetAssetPair("EUR");

            var eurUsdOrderBook = new OrderBook(exchange, usdeur,
                new List<VolumePrice> // bids
                {
                    new VolumePrice(1/1.11m, 9),
                    new VolumePrice(1/1.10m, 5)
                },
                new List<VolumePrice> // asks
                {
                    new VolumePrice(1/1.12m, 10),
                    new VolumePrice(1/1.13m, 7),
                    new VolumePrice(1/1.14m, 3)
                },
                timestamp2);
            eurUsdOrderBook.SetAssetPair("EUR");

            var synthOrderBook = SynthOrderBook.FromOrderBooks(btcEurOrderBook, eurUsdOrderBook, targetAssetPair);
            Assert.Equal(SynthOrderBook.GetSourcesPath(exchange, exchange), synthOrderBook.Source);
            Assert.Equal(targetAssetPair.Name, synthOrderBook.AssetPairStr);
            Assert.Equal(targetAssetPair, synthOrderBook.AssetPair);
            Assert.Equal(SynthOrderBook.GetConversionPath(exchange, eurbtc, exchange, usdeur), synthOrderBook.ConversionPath);
            Assert.Equal(9, synthOrderBook.Bids.Count);
            Assert.Equal(4, synthOrderBook.Asks.Count);
            Assert.Equal(10260m, synthOrderBook.Bids.Max(x => x.Price), 8);
            Assert.Equal(9795.75m, synthOrderBook.Asks.Max(x => x.Price), 8);
            Assert.Equal(9968.13827158m, synthOrderBook.Bids.Min(x => x.Price), 8);
            Assert.Equal(9705.30m, synthOrderBook.Asks.Min(x => x.Price), 8);
            Assert.Equal(0.00099206m, synthOrderBook.Bids.Max(x => x.Volume), 8);
            Assert.Equal(0.00091877m, synthOrderBook.Asks.Max(x => x.Volume), 8);
            Assert.Equal(0.00029240m, synthOrderBook.Bids.Min(x => x.Volume), 8);
            Assert.Equal(0.00051507m, synthOrderBook.Asks.Min(x => x.Volume), 8);
            Assert.Equal(timestamp1, synthOrderBook.Timestamp);
            Assert.Equal(2, synthOrderBook.OriginalOrderBooks.Count);
        }


        [Fact]
        public void From3OrderBooksExceptionTest()
        {
            const string exchange = "FakeExchange";
            var timestamp = DateTime.UtcNow;
            var assetPair = new AssetPair("BTC", "EUR");

            var orderBookWithEmptyAssetPair = new OrderBook(exchange, "BTCEUR", new List<VolumePrice>(), new List<VolumePrice>(), timestamp);
            var goodOrderBook1 = new OrderBook(exchange, "BTCCHF", new List<VolumePrice>(), new List<VolumePrice>(), timestamp);
            var goodOrderBook2 = new OrderBook(exchange, "BTCJPY", new List<VolumePrice>(), new List<VolumePrice>(), timestamp);

            // AssetPair is not set in order book
            Assert.Throws<ArgumentException>(() => SynthOrderBook.FromOrderBooks(orderBookWithEmptyAssetPair, goodOrderBook1, goodOrderBook2, assetPair));
            Assert.Throws<ArgumentException>(() => SynthOrderBook.FromOrderBooks(goodOrderBook1, orderBookWithEmptyAssetPair, goodOrderBook2, assetPair));
            Assert.Throws<ArgumentException>(() => SynthOrderBook.FromOrderBooks(goodOrderBook1, goodOrderBook2, orderBookWithEmptyAssetPair, assetPair));

            orderBookWithEmptyAssetPair.SetAssetPair("EUR");

            Assert.Throws<ArgumentException>(() => SynthOrderBook.FromOrderBooks(orderBookWithEmptyAssetPair, goodOrderBook1, goodOrderBook2, new AssetPair()));
        }

        [Fact]
        public void From3OrderBooksReversed_0_0_0_Test()
        {
            const string exchange1 = "TEST1";
            const string exchange2 = "TEST2";
            const string exchange3 = "TEST3";
            const string btcEur = "BTCEUR";
            const string eurJpy = "EURJPY";
            const string jpyUsd = "JPYUSD";
            var timestamp1 = DateTime.UtcNow.AddSeconds(-2);
            var timestamp2 = DateTime.UtcNow.AddSeconds(-1);
            var timestamp3 = DateTime.UtcNow;
            var targetAssetPair = new AssetPair("BTC", "USD");

            var btcEurOrderBook = new OrderBook(exchange1, btcEur,
                new List<VolumePrice> // bids
                {
                    new VolumePrice(7310m, 9), new VolumePrice(7300m, 5)
                },
                new List<VolumePrice> // asks
                {
                    new VolumePrice(7320m, 10), new VolumePrice(7330m, 7), new VolumePrice(7340m, 3)
                },
                timestamp1);
            btcEurOrderBook.SetAssetPair("BTC");

            var eurJpyOrderBook = new OrderBook(exchange2, eurJpy,
                new List<VolumePrice> // bids
                {
                    new VolumePrice(131m, 9), new VolumePrice(130m, 5)
                },
                new List<VolumePrice> // asks
                {
                    new VolumePrice(132m, 11), new VolumePrice(133m, 7), new VolumePrice(134m, 3)
                },
                timestamp2);
            eurJpyOrderBook.SetAssetPair("EUR");

            var jpyUsdOrderBook = new OrderBook(exchange3, jpyUsd,
                new List<VolumePrice> // bids
                {
                    new VolumePrice(0.009132m, 9), new VolumePrice(0.009131m, 5)
                },
                new List<VolumePrice> // asks
                {
                    new VolumePrice(0.009133m, 12), new VolumePrice(0.009134m, 7), new VolumePrice(0.009135m, 3)
                },
                timestamp3);
            jpyUsdOrderBook.SetAssetPair("JPY");

            var synthOrderBook = SynthOrderBook.FromOrderBooks(btcEurOrderBook, eurJpyOrderBook, jpyUsdOrderBook, targetAssetPair);
            Assert.Equal(SynthOrderBook.GetSourcesPath(exchange1, exchange2, exchange3), synthOrderBook.Source);
            Assert.Equal(targetAssetPair.Name, synthOrderBook.AssetPairStr);
            Assert.Equal(targetAssetPair, synthOrderBook.AssetPair);
            Assert.Equal(SynthOrderBook.GetConversionPath(exchange1, btcEur, exchange2, eurJpy, exchange3, jpyUsd), synthOrderBook.ConversionPath);
            Assert.Equal(8, synthOrderBook.Bids.Count);
            Assert.Equal(27, synthOrderBook.Asks.Count);
            Assert.Equal(8744.894520m, synthOrderBook.Bids.Max(x => x.Price), 8);
            Assert.Equal(8984.820600m, synthOrderBook.Asks.Max(x => x.Price), 8);
            Assert.Equal(8665.319000m, synthOrderBook.Bids.Min(x => x.Price), 8);
            Assert.Equal(8824.669920m, synthOrderBook.Asks.Min(x => x.Price), 8);
            Assert.Equal(0.00000948m, synthOrderBook.Bids.Max(x => x.Volume), 8);
            Assert.Equal(0.00001242m, synthOrderBook.Asks.Max(x => x.Volume), 8);
            Assert.Equal(0.00000522m, synthOrderBook.Bids.Min(x => x.Volume), 8);
            Assert.Equal(0.00000305m, synthOrderBook.Asks.Min(x => x.Volume), 8);
            Assert.Equal(timestamp1, synthOrderBook.Timestamp);
            Assert.Equal(3, synthOrderBook.OriginalOrderBooks.Count);
        }

        [Fact]
        public void From3OrderBooksReversed_0_0_1_Test()
        {
            const string exchange1 = "TEST1";
            const string exchange2 = "TEST2";
            const string exchange3 = "TEST3";
            const string btcEur = "BTCEUR";
            const string eurJpy = "EURJPY";
            const string usdJpy = "USDJPY";
            var timestamp1 = DateTime.UtcNow.AddSeconds(-2);
            var timestamp2 = DateTime.UtcNow.AddSeconds(-1);
            var timestamp3 = DateTime.UtcNow;
            var targetAssetPair = new AssetPair("BTC", "USD");

            var btcEurOrderBook = new OrderBook(exchange1, btcEur,
                new List<VolumePrice> // bids
                {
                    new VolumePrice(7310m, 9), new VolumePrice(7300m, 5)
                },
                new List<VolumePrice> // asks
                {
                    new VolumePrice(7320m, 10), new VolumePrice(7330m, 7), new VolumePrice(7340m, 3)
                },
                timestamp1);
            btcEurOrderBook.SetAssetPair("BTC");

            var eurJpyOrderBook = new OrderBook(exchange2, eurJpy,
                new List<VolumePrice> // bids
                {
                    new VolumePrice(131m, 9), new VolumePrice(130m, 5)
                },
                new List<VolumePrice> // asks
                {
                    new VolumePrice(132m, 11), new VolumePrice(133m, 7), new VolumePrice(134m, 3)
                },
                timestamp2);
            eurJpyOrderBook.SetAssetPair("EUR");

            var jpyUsdOrderBook = new OrderBook(exchange3, usdJpy,
                new List<VolumePrice> // bids
                {
                    new VolumePrice(1/0.009132m, 9), new VolumePrice(1/0.009131m, 5)
                },
                new List<VolumePrice> // asks
                {
                    new VolumePrice(1/0.009133m, 12), new VolumePrice(1/0.009134m, 7), new VolumePrice(1/0.009135m, 3)
                },
                timestamp3);
            jpyUsdOrderBook.SetAssetPair("JPY");

            var synthOrderBook = SynthOrderBook.FromOrderBooks(btcEurOrderBook, eurJpyOrderBook, jpyUsdOrderBook, targetAssetPair);
            Assert.Equal(SynthOrderBook.GetSourcesPath(exchange1, exchange2, exchange3), synthOrderBook.Source);
            Assert.Equal(targetAssetPair.Name, synthOrderBook.AssetPairStr);
            Assert.Equal(targetAssetPair, synthOrderBook.AssetPair);
            Assert.Equal(SynthOrderBook.GetConversionPath(exchange1, btcEur, exchange2, eurJpy, exchange3, usdJpy), synthOrderBook.ConversionPath);
            Assert.Equal(12, synthOrderBook.Bids.Count);
            Assert.Equal(18, synthOrderBook.Asks.Count);
            Assert.Equal(8747.767350m, synthOrderBook.Bids.Max(x => x.Price), 8);
            Assert.Equal(8981.869920m, synthOrderBook.Asks.Max(x => x.Price), 8);
            Assert.Equal(8667.217000m, synthOrderBook.Bids.Min(x => x.Price), 8);
            Assert.Equal(8822.737440m, synthOrderBook.Asks.Min(x => x.Price), 8);
            Assert.Equal(0.00123288m, synthOrderBook.Bids.Max(x => x.Volume), 8);
            Assert.Equal(0.00101998m, synthOrderBook.Asks.Max(x => x.Volume), 8);
            Assert.Equal(0.00034294m, synthOrderBook.Bids.Min(x => x.Volume), 8);
            Assert.Equal(0.00040872m, synthOrderBook.Asks.Min(x => x.Volume), 8);
            Assert.Equal(timestamp1, synthOrderBook.Timestamp);
            Assert.Equal(3, synthOrderBook.OriginalOrderBooks.Count);
        }

        [Fact]
        public void From3OrderBooksReversed_0_1_0_Test()
        {
            const string exchange1 = "TEST1";
            const string exchange2 = "TEST2";
            const string exchange3 = "TEST3";
            const string btcEur = "BTCEUR";
            const string eurJpy = "EURJPY";
            const string jpyUsd = "JPYUSD";
            var timestamp1 = DateTime.UtcNow.AddSeconds(-2);
            var timestamp2 = DateTime.UtcNow.AddSeconds(-1);
            var timestamp3 = DateTime.UtcNow;
            var targetAssetPair = new AssetPair("BTC", "USD");

            var btcEurOrderBook = new OrderBook(exchange1, btcEur,
                new List<VolumePrice> // bids
                {
                    new VolumePrice(7310m, 9), new VolumePrice(7300m, 5)
                },
                new List<VolumePrice> // asks
                {
                    new VolumePrice(7320m, 10), new VolumePrice(7330m, 7), new VolumePrice(7340m, 3)
                },
                timestamp1);
            btcEurOrderBook.SetAssetPair("BTC");

            var eurJpyOrderBook = new OrderBook(exchange2, eurJpy,
                new List<VolumePrice> // bids
                {
                    new VolumePrice(131m, 9), new VolumePrice(130m, 5)
                },
                new List<VolumePrice> // asks
                {
                    new VolumePrice(132m, 11), new VolumePrice(133m, 7), new VolumePrice(134m, 3)
                },
                timestamp2);
            eurJpyOrderBook.SetAssetPair("EUR");

            var jpyUsdOrderBook = new OrderBook(exchange3, jpyUsd,
                new List<VolumePrice> // bids
                {
                    new VolumePrice(0.009132m, 9), new VolumePrice(0.009131m, 5)
                },
                new List<VolumePrice> // asks
                {
                    new VolumePrice(0.009133m, 12), new VolumePrice(0.009134m, 7), new VolumePrice(0.009135m, 3)
                },
                timestamp3);
            jpyUsdOrderBook.SetAssetPair("JPY");

            var synthOrderBook = SynthOrderBook.FromOrderBooks(btcEurOrderBook, eurJpyOrderBook, jpyUsdOrderBook, targetAssetPair);
            Assert.Equal(SynthOrderBook.GetSourcesPath(exchange1, exchange2, exchange3), synthOrderBook.Source);
            Assert.Equal(targetAssetPair.Name, synthOrderBook.AssetPairStr);
            Assert.Equal(targetAssetPair, synthOrderBook.AssetPair);
            Assert.Equal(SynthOrderBook.GetConversionPath(exchange1, btcEur, exchange2, eurJpy, exchange3, jpyUsd), synthOrderBook.ConversionPath);
            Assert.Equal(8, synthOrderBook.Bids.Count);
            Assert.Equal(27, synthOrderBook.Asks.Count);
            Assert.Equal(8744.894520m, synthOrderBook.Bids.Max(x => x.Price));
            Assert.Equal(8984.820600m, synthOrderBook.Asks.Max(x => x.Price));
            Assert.Equal(8665.319000m, synthOrderBook.Bids.Min(x => x.Price));
            Assert.Equal(8824.669920m, synthOrderBook.Asks.Min(x => x.Price));
            Assert.Equal(0.00000948m, synthOrderBook.Bids.Max(x => x.Volume), 8);
            Assert.Equal(0.00001242m, synthOrderBook.Asks.Max(x => x.Volume), 8);
            Assert.Equal(0.00000522m, synthOrderBook.Bids.Min(x => x.Volume), 8);
            Assert.Equal(0.00000305m, synthOrderBook.Asks.Min(x => x.Volume), 8);
            Assert.Equal(timestamp1, synthOrderBook.Timestamp);
            Assert.Equal(3, synthOrderBook.OriginalOrderBooks.Count);
        }

        [Fact]
        public void From3OrderBooksReversed_1_0_0_Test()
        {
            const string exchange1 = "TEST1";
            const string exchange2 = "TEST2";
            const string exchange3 = "TEST3";
            const string eurBtc = "EURBTC";
            const string eurJpy = "EURJPY";
            const string jpyUsd = "JPYUSD";
            var timestamp1 = DateTime.UtcNow.AddSeconds(-2);
            var timestamp2 = DateTime.UtcNow.AddSeconds(-1);
            var timestamp3 = DateTime.UtcNow;
            var targetAssetPair = new AssetPair("BTC", "USD");

            var btcEurOrderBook = new OrderBook(exchange1, eurBtc,
                new List<VolumePrice> // bids
                {
                    new VolumePrice(1/7310m, 9), new VolumePrice(1/7300m, 5)
                },
                new List<VolumePrice> // asks
                {
                    new VolumePrice(1/7320m, 10), new VolumePrice(1/7330m, 7), new VolumePrice(1/7340m, 3)
                },
                timestamp1);
            btcEurOrderBook.SetAssetPair("BTC");

            var eurJpyOrderBook = new OrderBook(exchange2, eurJpy,
                new List<VolumePrice> // bids
                {
                    new VolumePrice(131m, 9), new VolumePrice(130m, 5)
                },
                new List<VolumePrice> // asks
                {
                    new VolumePrice(132m, 11), new VolumePrice(133m, 7), new VolumePrice(134m, 3)
                },
                timestamp2);
            eurJpyOrderBook.SetAssetPair("EUR");

            var jpyUsdOrderBook = new OrderBook(exchange3, jpyUsd,
                new List<VolumePrice> // bids
                {
                    new VolumePrice(0.009132m, 9), new VolumePrice(0.009131m, 5)
                },
                new List<VolumePrice> // asks
                {
                    new VolumePrice(0.009133m, 12), new VolumePrice(0.009134m, 7), new VolumePrice(0.009135m, 3)
                },
                timestamp3);
            jpyUsdOrderBook.SetAssetPair("JPY");

            var synthOrderBook = SynthOrderBook.FromOrderBooks(btcEurOrderBook, eurJpyOrderBook, jpyUsdOrderBook, targetAssetPair);
            Assert.Equal(SynthOrderBook.GetSourcesPath(exchange1, exchange2, exchange3), synthOrderBook.Source);
            Assert.Equal(targetAssetPair.Name, synthOrderBook.AssetPairStr);
            Assert.Equal(targetAssetPair, synthOrderBook.AssetPair);
            Assert.Equal(SynthOrderBook.GetConversionPath(exchange1, eurBtc, exchange2, eurJpy, exchange3, jpyUsd), synthOrderBook.ConversionPath);
            Assert.Equal(12, synthOrderBook.Bids.Count);
            Assert.Equal(18, synthOrderBook.Asks.Count);
            Assert.Equal(8780.78328m, synthOrderBook.Bids.Max(x => x.Price), 8);
            Assert.Equal(8948.0979m, synthOrderBook.Asks.Max(x => x.Price), 8);
            Assert.Equal(8689.0596m, synthOrderBook.Bids.Min(x => x.Price), 8);
            Assert.Equal(8800.5588m, synthOrderBook.Asks.Min(x => x.Price), 8);
            Assert.Equal(0.00000946m, synthOrderBook.Bids.Max(x => x.Volume), 8);
            Assert.Equal(0.00001245m, synthOrderBook.Asks.Max(x => x.Volume), 8);
            Assert.Equal(0.0000052m, synthOrderBook.Bids.Min(x => x.Volume), 8);
            Assert.Equal(0.00000306m, synthOrderBook.Asks.Min(x => x.Volume), 8);
            Assert.Equal(timestamp1, synthOrderBook.Timestamp);
            Assert.Equal(3, synthOrderBook.OriginalOrderBooks.Count);
        }

        [Fact]
        public void From3OrderBooksReversed_0_1_1_Test()
        {
            const string exchange1 = "TEST1";
            const string exchange2 = "TEST2";
            const string exchange3 = "TEST3";
            const string btcEur = "BTCEUR";
            const string jpyEur = "JPYEUR";
            const string usdJpy = "USDJPY";
            var timestamp1 = DateTime.UtcNow.AddSeconds(-2);
            var timestamp2 = DateTime.UtcNow.AddSeconds(-1);
            var timestamp3 = DateTime.UtcNow;
            var targetAssetPair = new AssetPair("BTC", "USD");

            var btcEurOrderBook = new OrderBook(exchange1, btcEur,
                new List<VolumePrice> // bids
                {
                    new VolumePrice(7310m, 9), new VolumePrice(7300m, 5)
                },
                new List<VolumePrice> // asks
                {
                    new VolumePrice(7320m, 10), new VolumePrice(7330m, 7), new VolumePrice(7340m, 3)
                },
                timestamp1);
            btcEurOrderBook.SetAssetPair("BTC");

            var jpyEurOrderBook = new OrderBook(exchange2, jpyEur,
                new List<VolumePrice> // bids
                {
                    new VolumePrice(1/132m, 11), new VolumePrice(1/133m, 7), new VolumePrice(1/134m, 3)
                },
                new List<VolumePrice> // asks
                {
                    new VolumePrice(1/131m, 9), new VolumePrice(1/130m, 5)
                },
                timestamp2);
            jpyEurOrderBook.SetAssetPair("EUR");

            var jpyUsdOrderBook = new OrderBook(exchange3, usdJpy,
                new List<VolumePrice> // bids
                {
                    new VolumePrice(1/0.009132m, 9), new VolumePrice(1/0.009131m, 5)
                },
                new List<VolumePrice> // asks
                {
                    new VolumePrice(1/0.009133m, 12), new VolumePrice(1/0.009134m, 7), new VolumePrice(1/0.009135m, 3)
                },
                timestamp3);
            jpyUsdOrderBook.SetAssetPair("JPY");

            var synthOrderBook = SynthOrderBook.FromOrderBooks(btcEurOrderBook, jpyEurOrderBook, jpyUsdOrderBook, targetAssetPair);
            Assert.Equal(SynthOrderBook.GetSourcesPath(exchange1, exchange2, exchange3), synthOrderBook.Source);
            Assert.Equal(targetAssetPair.Name, synthOrderBook.AssetPairStr);
            Assert.Equal(targetAssetPair, synthOrderBook.AssetPair);
            Assert.Equal(SynthOrderBook.GetConversionPath(exchange1, btcEur, exchange2, jpyEur, exchange3, usdJpy), synthOrderBook.ConversionPath);
            Assert.Equal(12, synthOrderBook.Bids.Count);
            Assert.Equal(18, synthOrderBook.Asks.Count);
            Assert.Equal(8747.76735m, synthOrderBook.Bids.Max(x => x.Price), 8);
            Assert.Equal(8981.86992m, synthOrderBook.Asks.Max(x => x.Price), 8);
            Assert.Equal(8667.217m, synthOrderBook.Bids.Min(x => x.Price), 8);
            Assert.Equal(8822.73744m, synthOrderBook.Asks.Min(x => x.Price), 8);
            Assert.Equal(0.00000941m, synthOrderBook.Bids.Max(x => x.Volume), 8);
            Assert.Equal(0.00001138m, synthOrderBook.Asks.Max(x => x.Volume), 8);
            Assert.Equal(0.00000526m, synthOrderBook.Bids.Min(x => x.Volume), 8);
            Assert.Equal(0.00000305m, synthOrderBook.Asks.Min(x => x.Volume), 8);
            Assert.Equal(timestamp1, synthOrderBook.Timestamp);
            Assert.Equal(3, synthOrderBook.OriginalOrderBooks.Count);
        }

        [Fact]
        public void From3OrderBooksReversed_1_0_1_Test()
        {
            const string exchange1 = "TEST1";
            const string exchange2 = "TEST2";
            const string exchange3 = "TEST3";
            const string eurBtc = "EURBTC";
            const string eurJpy = "EURJPY";
            const string usdJpy = "USDJPY";
            var timestamp1 = DateTime.UtcNow.AddSeconds(-2);
            var timestamp2 = DateTime.UtcNow.AddSeconds(-1);
            var timestamp3 = DateTime.UtcNow;
            var targetAssetPair = new AssetPair("BTC", "USD");

            var btcEurOrderBook = new OrderBook(exchange1, eurBtc,
                new List<VolumePrice> // bids
                {
                    new VolumePrice(1/7310m, 9), new VolumePrice(1/7300m, 5)
                },
                new List<VolumePrice> // asks
                {
                    new VolumePrice(1/7320m, 10), new VolumePrice(1/7330m, 7), new VolumePrice(1/7340m, 3)
                },
                timestamp1);
            btcEurOrderBook.SetAssetPair("BTC");

            var eurJpyOrderBook = new OrderBook(exchange2, eurJpy,
                new List<VolumePrice> // bids
                {
                    new VolumePrice(131m, 9), new VolumePrice(130m, 5)
                },
                new List<VolumePrice> // asks
                {
                    new VolumePrice(132m, 11), new VolumePrice(133m, 7), new VolumePrice(134m, 3)
                },
                timestamp2);
            eurJpyOrderBook.SetAssetPair("EUR");

            var jpyUsdOrderBook = new OrderBook(exchange3, usdJpy,
                new List<VolumePrice> // bids
                {
                    new VolumePrice(1/0.009132m, 9), new VolumePrice(1/0.009131m, 5)
                },
                new List<VolumePrice> // asks
                {
                    new VolumePrice(1/0.009133m, 12), new VolumePrice(1/0.009134m, 7), new VolumePrice(1/0.009135m, 3)
                },
                timestamp3);
            jpyUsdOrderBook.SetAssetPair("JPY");

            var synthOrderBook = SynthOrderBook.FromOrderBooks(btcEurOrderBook, eurJpyOrderBook, jpyUsdOrderBook, targetAssetPair);
            Assert.Equal(SynthOrderBook.GetSourcesPath(exchange1, exchange2, exchange3), synthOrderBook.Source);
            Assert.Equal(targetAssetPair.Name, synthOrderBook.AssetPairStr);
            Assert.Equal(targetAssetPair, synthOrderBook.AssetPair);
            Assert.Equal(SynthOrderBook.GetConversionPath(exchange1, eurBtc, exchange2, eurJpy, exchange3, usdJpy), synthOrderBook.ConversionPath);
            Assert.Equal(18, synthOrderBook.Bids.Count);
            Assert.Equal(12, synthOrderBook.Asks.Count);
            Assert.Equal(8783.6679m, synthOrderBook.Bids.Max(x => x.Price), 8);
            Assert.Equal(8945.15928m, synthOrderBook.Asks.Max(x => x.Price), 8);
            Assert.Equal(8690.9628m, synthOrderBook.Bids.Min(x => x.Price), 8);
            Assert.Equal(8798.6316m, synthOrderBook.Asks.Min(x => x.Price), 8);
            Assert.Equal(0.00122951m, synthOrderBook.Bids.Max(x => x.Volume), 8);
            Assert.Equal(0.00102138m, synthOrderBook.Asks.Max(x => x.Volume), 8);
            Assert.Equal(0.00034154m, synthOrderBook.Bids.Min(x => x.Volume), 8);
            Assert.Equal(0.00041040m, synthOrderBook.Asks.Min(x => x.Volume), 8);
            Assert.Equal(timestamp1, synthOrderBook.Timestamp);
            Assert.Equal(3, synthOrderBook.OriginalOrderBooks.Count);
        }

        [Fact]
        public void From3OrderBooksReversed_1_1_0_Test()
        {
            const string exchange1 = "TEST1";
            const string exchange2 = "TEST2";
            const string exchange3 = "TEST3";
            const string eurBtc = "EURBTC";
            const string jpyEur = "JPYEUR";
            const string jpyUsd = "JPYUSD";
            var timestamp1 = DateTime.UtcNow.AddSeconds(-2);
            var timestamp2 = DateTime.UtcNow.AddSeconds(-1);
            var timestamp3 = DateTime.UtcNow;
            var targetAssetPair = new AssetPair("BTC", "USD");

            var btcEurOrderBook = new OrderBook(exchange1, eurBtc,
                new List<VolumePrice> // bids
                {
                    new VolumePrice(1/7310m, 9), new VolumePrice(1/7300m, 5)
                },
                new List<VolumePrice> // asks
                {
                    new VolumePrice(1/7320m, 10), new VolumePrice(1/7330m, 7), new VolumePrice(1/7340m, 3)
                },
                timestamp1);
            btcEurOrderBook.SetAssetPair("BTC");

            var eurJpyOrderBook = new OrderBook(exchange2, jpyEur,
                new List<VolumePrice> // bids
                {
                    new VolumePrice(1/131m, 9), new VolumePrice(1/130m, 5)
                },
                new List<VolumePrice> // asks
                {
                    new VolumePrice(1/132m, 11), new VolumePrice(1/133m, 7), new VolumePrice(1/134m, 3)
                },
                timestamp2);
            eurJpyOrderBook.SetAssetPair("EUR");

            var jpyUsdOrderBook = new OrderBook(exchange3, jpyUsd,
                new List<VolumePrice> // bids
                {
                    new VolumePrice(0.009132m, 9), new VolumePrice(0.009131m, 5)
                },
                new List<VolumePrice> // asks
                {
                    new VolumePrice(0.009133m, 12), new VolumePrice(0.009134m, 7), new VolumePrice(0.009135m, 3)
                },
                timestamp3);
            jpyUsdOrderBook.SetAssetPair("JPY");

            var synthOrderBook = SynthOrderBook.FromOrderBooks(btcEurOrderBook, eurJpyOrderBook, jpyUsdOrderBook, targetAssetPair);
            Assert.Equal(SynthOrderBook.GetSourcesPath(exchange1, exchange2, exchange3), synthOrderBook.Source);
            Assert.Equal(targetAssetPair.Name, synthOrderBook.AssetPairStr);
            Assert.Equal(targetAssetPair, synthOrderBook.AssetPair);
            Assert.Equal(SynthOrderBook.GetConversionPath(exchange1, eurBtc, exchange2, jpyEur, exchange3, jpyUsd), synthOrderBook.ConversionPath);
            Assert.Equal(18, synthOrderBook.Bids.Count);
            Assert.Equal(12, synthOrderBook.Asks.Count);
            Assert.Equal(8981.86992m, synthOrderBook.Bids.Max(x => x.Price), 8);
            Assert.Equal(8747.76735m, synthOrderBook.Asks.Max(x => x.Price), 8);
            Assert.Equal(8822.73744m, synthOrderBook.Bids.Min(x => x.Price), 8);
            Assert.Equal(8667.217m, synthOrderBook.Asks.Min(x => x.Price), 8);
            Assert.Equal(0.00000931m, synthOrderBook.Bids.Max(x => x.Volume), 8);
            Assert.Equal(0.00000941m, synthOrderBook.Asks.Max(x => x.Volume), 8);
            Assert.Equal(0.00000305m, synthOrderBook.Bids.Min(x => x.Volume), 8);
            Assert.Equal(0.00000313m, synthOrderBook.Asks.Min(x => x.Volume), 8);
            Assert.Equal(timestamp1, synthOrderBook.Timestamp);
            Assert.Equal(3, synthOrderBook.OriginalOrderBooks.Count);
        }
        
        [Fact]
        public void From3OrderBooksReversed_1_1_1_Test()
        {
            const string exchange1 = "TEST1";
            const string exchange2 = "TEST2";
            const string exchange3 = "TEST3";
            const string eurBtc = "EURBTC";
            const string jpyEur = "JPYEUR";
            const string usdJpy = "USDJPY";
            var timestamp1 = DateTime.UtcNow.AddSeconds(-2);
            var timestamp2 = DateTime.UtcNow.AddSeconds(-1);
            var timestamp3 = DateTime.UtcNow;
            var targetAssetPair = new AssetPair("BTC", "USD");

            var btcEurOrderBook = new OrderBook(exchange1, eurBtc,
                new List<VolumePrice> // bids
                {
                    new VolumePrice(1/7310m, 9), new VolumePrice(1/7300m, 5)
                },
                new List<VolumePrice> // asks
                {
                    new VolumePrice(1/7320m, 10), new VolumePrice(1/7330m, 7), new VolumePrice(1/7340m, 3)
                },
                timestamp1);
            btcEurOrderBook.SetAssetPair("BTC");

            var eurJpyOrderBook = new OrderBook(exchange2, jpyEur,
                new List<VolumePrice> // bids
                {
                    new VolumePrice(1/131m, 9), new VolumePrice(1/130m, 5)
                },
                new List<VolumePrice> // asks
                {
                    new VolumePrice(1/132m, 11), new VolumePrice(1/133m, 7), new VolumePrice(1/134m, 3)
                },
                timestamp2);
            eurJpyOrderBook.SetAssetPair("EUR");

            var jpyUsdOrderBook = new OrderBook(exchange3, usdJpy,
                new List<VolumePrice> // bids
                {
                    new VolumePrice(1/0.009132m, 9), new VolumePrice(1/0.009131m, 5)
                },
                new List<VolumePrice> // asks
                {
                    new VolumePrice(1/0.009133m, 12), new VolumePrice(1/0.009134m, 7), new VolumePrice(1/0.009135m, 3)
                },
                timestamp3);
            jpyUsdOrderBook.SetAssetPair("JPY");

            var synthOrderBook = SynthOrderBook.FromOrderBooks(btcEurOrderBook, eurJpyOrderBook, jpyUsdOrderBook, targetAssetPair);
            Assert.Equal(SynthOrderBook.GetSourcesPath(exchange1, exchange2, exchange3), synthOrderBook.Source);
            Assert.Equal(targetAssetPair.Name, synthOrderBook.AssetPairStr);
            Assert.Equal(targetAssetPair, synthOrderBook.AssetPair);
            Assert.Equal(SynthOrderBook.GetConversionPath(exchange1, eurBtc, exchange2, jpyEur, exchange3, usdJpy), synthOrderBook.ConversionPath);
            Assert.Equal(27, synthOrderBook.Bids.Count);
            Assert.Equal(8, synthOrderBook.Asks.Count);
            Assert.Equal(8984.8206m, synthOrderBook.Bids.Max(x => x.Price), 8);
            Assert.Equal(8744.89452m, synthOrderBook.Asks.Max(x => x.Price), 8);
            Assert.Equal(8824.66992m, synthOrderBook.Bids.Min(x => x.Price), 8);
            Assert.Equal(8665.319m, synthOrderBook.Asks.Min(x => x.Price), 8);
            Assert.Equal(0.00001138m, synthOrderBook.Bids.Max(x => x.Volume), 8);
            Assert.Equal(0.00000941m, synthOrderBook.Asks.Max(x => x.Volume), 8);
            Assert.Equal(0.00000305m, synthOrderBook.Bids.Min(x => x.Volume), 8);
            Assert.Equal(0.00000526m, synthOrderBook.Asks.Min(x => x.Volume), 8);
            Assert.Equal(timestamp1, synthOrderBook.Timestamp);
            Assert.Equal(3, synthOrderBook.OriginalOrderBooks.Count);
        }
    }
}
