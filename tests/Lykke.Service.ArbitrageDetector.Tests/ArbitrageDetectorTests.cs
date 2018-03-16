using System;
using System.Collections.Generic;
using System.Linq;
using Lykke.Service.ArbitrageDetector.Core.Domain;
using Lykke.Service.ArbitrageDetector.Services;
using Xunit;

namespace Lykke.Service.ArbitrageDetector.Tests
{
    public class ArbitrageDetectorTests
    {
        [Fact]
        public void StraightConversionTest()
        {
            // BTCEUR * EURUSD
            var wantedCurrencies = new List<string> { "BTC" };
            const string baseCurrency = "USD";
            const string exchange = "Lykke";
            const string btcusd = "BTCUSD";

            var arbitrageCalculator = new ArbitrageDetectorService(null, null, wantedCurrencies, baseCurrency, 10, 10);

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
            Assert.Equal(10769.1475m, crossRate.BestBidPrice, 8);
            Assert.Equal(10982.9089835m, crossRate.BestAskPrice, 8);
            Assert.Equal(9, crossRate.Asks.Count);
            Assert.Equal(4, crossRate.Bids.Count);
            Assert.Equal(2, crossRate.OriginalOrderBooks.Count);
        }

        [Fact]
        public void ReverseConversionFirstPairTest()
        {
            // BTCEUR * USDEUR
            var wantedCurrencies = new List<string> { "BTC" };
            const string baseCurrency = "USD";
            const string exchange = "Lykke";
            const string btcusd = "BTCUSD";

            var arbitrageCalculator = new ArbitrageDetectorService(null, null, wantedCurrencies, baseCurrency, 10, 10);

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

            var crossRates = arbitrageCalculator.CalculateCrossRates().ToList();
            Assert.Single(crossRates);
            var crossRate = crossRates.First();
            Assert.Equal($"{exchange}-{exchange}", crossRate.Source);
            Assert.Equal("Lykke-BTCEUR & Lykke-USDEUR", crossRate.ConversionPath);
            Assert.Equal(btcusd, crossRate.AssetPairStr);
            Assert.Equal(10769.1475m, crossRate.BestBidPrice, 8);
            Assert.Equal(10982.9089835m, crossRate.BestAskPrice, 8);
            Assert.Equal(6, crossRate.Asks.Count);
            Assert.Equal(4, crossRate.Bids.Count);
            Assert.Equal(2, crossRate.OriginalOrderBooks.Count);
        }

        [Fact]
        public void ReverseConversionSecondPairTest()
        {
            // EURBTC * EURUSD
            var wantedCurrencies = new List<string> { "BTC" };
            const string baseCurrency = "USD";
            const string exchange = "Lykke";
            const string btcusd = "BTCUSD";

            var arbitrageCalculator = new ArbitrageDetectorService(null, null, wantedCurrencies, baseCurrency, 10, 10);

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

            var crossRates = arbitrageCalculator.CalculateCrossRates().ToList();
            Assert.Single(crossRates);
            var crossRate = crossRates.First();
            Assert.Equal($"{exchange}-{exchange}", crossRate.Source);
            Assert.Equal("Lykke-EURBTC & Lykke-EURUSD", crossRate.ConversionPath);
            Assert.Equal(btcusd, crossRate.AssetPairStr);
            Assert.Equal(10769.1475m, crossRate.BestBidPrice, 8);
            Assert.Equal(10982.9089835m, crossRate.BestAskPrice, 8);
            Assert.Equal(4, crossRate.Asks.Count);
            Assert.Equal(6, crossRate.Bids.Count);
            Assert.Equal(2, crossRate.OriginalOrderBooks.Count);
        }

        [Fact]
        public void ReverseConversionBothPairsTest()
        {
            // EURBTC * USDEUR
            var wantedCurrencies = new List<string> { "BTC" };
            const string baseCurrency = "USD";
            const string exchange = "Lykke";
            const string btcusd = "BTCUSD";

            var arbitrageCalculator = new ArbitrageDetectorService(null, null, wantedCurrencies, baseCurrency, 10, 10);

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

            var crossRates = arbitrageCalculator.CalculateCrossRates().ToList();
            Assert.Single(crossRates);
            var crossRate = crossRates.First();
            Assert.Equal($"{exchange}-{exchange}", crossRate.Source);
            Assert.Equal("Lykke-EURBTC & Lykke-USDEUR", crossRate.ConversionPath);
            Assert.Equal(btcusd, crossRate.AssetPairStr);
            Assert.Equal(10769.1475m, crossRate.BestBidPrice, 8);
            Assert.Equal(10982.9089835m, crossRate.BestAskPrice, 8);
            Assert.Equal(4, crossRate.Asks.Count);
            Assert.Equal(6, crossRate.Bids.Count);
            Assert.Equal(2, crossRate.OriginalOrderBooks.Count);
        }

        [Fact]
        public void ArbitrageTest()
        {
            var wantedCurrencies = new List<string> { "BTC" };
            const string baseCurrency = "USD";

            var arbitrageDetector = new ArbitrageDetectorService(null, null, wantedCurrencies, baseCurrency, 10, 10);

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

            arbitrageDetector.CalculateCrossRates();

            var crossRates = arbitrageDetector.GetCrossRates();
            var arbitrages = arbitrageDetector.GetArbitrages();

            Assert.Equal(3, crossRates.Count());
            Assert.Equal(3, arbitrages.Count());

            var arbitrage1 = arbitrages.First(x => x.HighBid.Source == "GDAX" && x.LowAsk.Source == "Quoine-Binance");
            Assert.Equal(11000, arbitrage1.HighBid.BestBidPrice);
            Assert.Equal(10982.9089835m, arbitrage1.LowAsk.BestAskPrice, 8);

            var arbitrage2 = arbitrages.First(x => x.HighBid.Source == "Bitfinex" && x.LowAsk.Source == "Quoine-Binance");
            Assert.Equal(11100, arbitrage2.HighBid.BestBidPrice);
            Assert.Equal(10982.9089835m, arbitrage2.LowAsk.BestAskPrice, 8);

            var arbitrage3 = arbitrages.First(x => x.HighBid.Source == "Bitfinex" && x.LowAsk.Source == "GDAX");
            Assert.Equal(11100, arbitrage3.HighBid.BestBidPrice);
            Assert.Equal(11050m, arbitrage3.LowAsk.BestAskPrice);
        }

        [Fact]
        public void ReverseOrderBookTest()
        {
            var exchangeName = "FakeExchange";
            var assetPair = "BTCUSD";
            var timestamp = DateTime.UtcNow;

            var orderBook = new OrderBook(exchangeName, assetPair,
                new List<VolumePrice> // asks
                {
                    new VolumePrice(9000, 10), new VolumePrice(8999.95m, 7), new VolumePrice(8900.12345677m, 3)
                },
                new List<VolumePrice> // bids
                {
                    new VolumePrice(8825, 9), new VolumePrice(8823, 5)
                },
                timestamp);
            orderBook.SetAssetPair("USD");

            // USD
            var reversed = orderBook.Reverse();
            Assert.NotNull(reversed);
            Assert.Equal(exchangeName, reversed.Source);
            Assert.Equal("USDBTC", reversed.AssetPairStr);
            Assert.Equal(orderBook.Asks.Count, reversed.Bids.Count);
            Assert.Equal(orderBook.Bids.Count, reversed.Asks.Count);

            var bidVolumePrice1 = reversed.Bids.Single(x => x.Volume == 10);
            var bidVolumePrice2 = reversed.Bids.Single(x => x.Volume == 7);
            var bidVolumePrice3 = reversed.Bids.Single(x => x.Volume == 3);
            Assert.Equal(bidVolumePrice1.Price, 1 / 9000m, 8);
            Assert.Equal(bidVolumePrice2.Price, 1 / 8999.95m, 8);
            Assert.Equal(bidVolumePrice3.Price, 1 / 8900.12345677m, 8);

            var askVolumePrice1 = reversed.Asks.Single(x => x.Volume == 9);
            var askVolumePrice2 = reversed.Asks.Single(x => x.Volume == 5);
            Assert.Equal(askVolumePrice1.Price, 1 / 8825m, 8);
            Assert.Equal(askVolumePrice2.Price, 1 / 8823m, 8);

            Assert.Equal(timestamp, reversed.Timestamp);
        }
    }
}
