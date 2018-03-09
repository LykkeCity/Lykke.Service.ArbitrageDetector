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
                new List<VolumePrice> // bids
                {
                    new VolumePrice(8825, 10), new VolumePrice(8823, 10)
                },
                new List<VolumePrice> // asks
                {
                    new VolumePrice(8999.95m, 10), new VolumePrice(9000, 10)
                },
                DateTime.UtcNow);

            var eurUsdOrderBook = new OrderBook(exchange, "EURUSD",
                new List<VolumePrice> // bids
                {
                    new VolumePrice(1.2203m, 10), new VolumePrice(1.2201m, 10)
                },
                new List<VolumePrice> // asks
                {
                    new VolumePrice(1.22033m, 10), new VolumePrice(1.22035m, 10)
                },
                DateTime.UtcNow);

            arbitrageCalculator.Process(btcEurOrderBook);
            arbitrageCalculator.Process(eurUsdOrderBook);

            var crossRates = arbitrageCalculator.CalculateCrossRates();
            Assert.Single(crossRates);
            var crossRate = crossRates.First();
            Assert.Equal($"{exchange}-{exchange}", crossRate.Source);
            Assert.Equal("Lykke-BTCEUR & Lykke-EURUSD", crossRate.ConversionPath);
            Assert.Equal(btcusd, crossRate.AssetPair);
            Assert.Equal(10769.1475m, crossRate.Bid, 5);
            Assert.Equal(10982.90898m, crossRate.Ask, 5);
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
                new List<VolumePrice> // bids
                {
                    new VolumePrice(8825, 10),
                    new VolumePrice(8823, 10)
                },
                new List<VolumePrice> // asks
                {
                    new VolumePrice(8999.95m, 10),
                    new VolumePrice(9000, 10)
                },
                DateTime.UtcNow);

            var usdEurOrderBook = new OrderBook(exchange, "USDEUR",
                new List<VolumePrice> // bids
                {
                    new VolumePrice(1/1.22033m, 10),
                    new VolumePrice(1/1.22035m, 10)
                },
                new List<VolumePrice> // ask
                {
                    new VolumePrice(1/1.2203m, 10),
                    new VolumePrice(1/1.2201m, 10)
                },
                DateTime.UtcNow);

            arbitrageCalculator.Process(btcEurOrderBook);
            arbitrageCalculator.Process(usdEurOrderBook);

            var crossRates = arbitrageCalculator.CalculateCrossRates().ToList();
            Assert.Single(crossRates);
            var crossRate = crossRates.First();
            Assert.Equal($"{exchange}-{exchange}", crossRate.Source);
            Assert.Equal("Lykke-BTCEUR & Lykke-USDEUR", crossRate.ConversionPath);
            Assert.Equal(btcusd, crossRate.AssetPair);
            Assert.Equal(10769.1475m, crossRate.Bid, 5);
            Assert.Equal(10982.90898m, crossRate.Ask, 5);
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
                new List<VolumePrice> // asks
                {
                    new VolumePrice(1/8999.95m, 10),
                    new VolumePrice(1/9000m, 10)
                },
                new List<VolumePrice> // bids
                {
                    new VolumePrice(1/8825m, 10),
                    new VolumePrice(1/8823m, 10)
                },
                DateTime.UtcNow);

            var eurUsdOrderBook = new OrderBook(exchange, "EURUSD",
                new List<VolumePrice> // bids
                {
                    new VolumePrice(1.2203m, 10),
                    new VolumePrice(1.2201m, 10)
                },
                new List<VolumePrice> // asks
                {
                    new VolumePrice(1.22033m, 10),
                    new VolumePrice(1.22035m, 10)
                },
                DateTime.UtcNow);

            arbitrageCalculator.Process(btcEurOrderBook);
            arbitrageCalculator.Process(eurUsdOrderBook);

            var crossRates = arbitrageCalculator.CalculateCrossRates().ToList();
            Assert.Single(crossRates);
            var crossRate = crossRates.First();
            Assert.Equal($"{exchange}-{exchange}", crossRate.Source);
            Assert.Equal("Lykke-EURBTC & Lykke-EURUSD", crossRate.ConversionPath);
            Assert.Equal(btcusd, crossRate.AssetPair);
            Assert.Equal(10769.1475m, crossRate.Bid, 5);
            Assert.Equal(10982.90898m, crossRate.Ask, 5);
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
                new List<VolumePrice> // bids
                {
                    new VolumePrice(1/8999.95m, 10),
                    new VolumePrice(1/9000m, 10)
                },
                new List<VolumePrice> // asks
                {
                    new VolumePrice(1/8825m, 10),
                    new VolumePrice(1/8823m, 10)
                },
                DateTime.UtcNow);

            var usdEurOrderBook = new OrderBook(exchange, "USDEUR",
                new List<VolumePrice> // bids
                {
                    new VolumePrice(1/1.22033m, 10),
                    new VolumePrice(1/1.22035m, 10)
                },
                new List<VolumePrice> // ask
                {
                    new VolumePrice(1/1.2203m, 10),
                    new VolumePrice(1/1.2201m, 10)
                },
                DateTime.UtcNow);

            arbitrageCalculator.Process(eurBtcOrderBook);
            arbitrageCalculator.Process(usdEurOrderBook);

            var crossRates = arbitrageCalculator.CalculateCrossRates().ToList();
            Assert.Single(crossRates);
            var crossRate = crossRates.First();
            Assert.Equal($"{exchange}-{exchange}", crossRate.Source);
            Assert.Equal("Lykke-EURBTC & Lykke-USDEUR", crossRate.ConversionPath);
            Assert.Equal(btcusd, crossRate.AssetPair);
            Assert.Equal(10769.1475m, crossRate.Bid, 5);
            Assert.Equal(10982.90898m, crossRate.Ask, 5);
            Assert.Equal(2, crossRate.OriginalOrderBooks.Count);
        }

        [Fact]
        public void ArbitrageTest()
        {
            var wantedCurrencies = new List<string> { "BTC" };
            const string baseCurrency = "USD";

            var arbitrageDetector = new ArbitrageDetectorService(null, null, wantedCurrencies, baseCurrency, 10, 10);

            var btcUsdOrderBook1 = new OrderBook("GDAX", "BTCUSD",
                new List<VolumePrice> { new VolumePrice(11000, 10) }, // bids
                new List<VolumePrice> { new VolumePrice(11050, 10) }, // asks
                DateTime.UtcNow);

            var btcUsdOrderBook2 = new OrderBook("Bitfinex", "BTCUSD",
                new List<VolumePrice> { new VolumePrice(11100, 10) }, // bids
                new List<VolumePrice> { new VolumePrice(11300, 10) }, // asks
                DateTime.UtcNow);

            var btcEurOrderBook = new OrderBook("Quoine", "BTCEUR",
                new List<VolumePrice> { new VolumePrice(8825, 10) }, // bids
                new List<VolumePrice> { new VolumePrice(8999.95m, 10) }, // asks
                DateTime.UtcNow);

            var eurUsdOrderBook = new OrderBook("Binance", "EURUSD",
                new List<VolumePrice> { new VolumePrice(1.2203m, 10) }, // bids
                new List<VolumePrice> { new VolumePrice(1.22033m, 10) }, // asks
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
            Assert.Equal(11000, arbitrage1.HighBid.Bid);
            Assert.Equal(10982.90898m, arbitrage1.LowAsk.Ask, 5);

            var arbitrage2 = arbitrages.First(x => x.HighBid.Source == "Bitfinex" && x.LowAsk.Source == "Quoine-Binance");
            Assert.Equal(11100, arbitrage2.HighBid.Bid);
            Assert.Equal(10982.90898m, arbitrage2.LowAsk.Ask, 5);

            var arbitrage3 = arbitrages.First(x => x.HighBid.Source == "Bitfinex" && x.LowAsk.Source == "GDAX");
            Assert.Equal(11100, arbitrage3.HighBid.Bid);
            Assert.Equal(11050m, arbitrage3.LowAsk.Ask);
        }
    }
}
