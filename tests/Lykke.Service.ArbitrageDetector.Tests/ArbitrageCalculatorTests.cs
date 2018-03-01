using System;
using System.Collections.Generic;
using System.Linq;
using Lykke.Service.ArbitrageDetector.Core.Domain;
using Lykke.Service.ArbitrageDetector.Services;
using Xunit;

namespace Lykke.Service.ArbitrageDetector.Tests
{
    public class ArbitrageCalculatorTests
    {
        [Fact]
        public void StraightConversionTest()
        {
            var wantedCurrencies = new List<string> { "BTC" };
            const string baseCurrency = "USD";
            const string exchange = "Lykke";
            const string btcusd = "BTCUSD";

            var arbitrageCalculator = new ArbitrageCalculator(null, wantedCurrencies, baseCurrency, 10, 10);

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

            var crossRates = arbitrageCalculator.CalculateCrossRates();
            Assert.Single(crossRates);
            var crossRate = crossRates.First();
            Assert.Equal(exchange, crossRate.Key.Source);
            Assert.Equal(btcusd, crossRate.Key.AssetPair);
            Assert.Equal(exchange, crossRate.Value.Source);
            Assert.Equal(btcusd, crossRate.Value.AssetPair);
            Assert.Equal(10769.1475m, crossRate.Value.BestBid, 5);
            Assert.Equal(10982.90898m, crossRate.Value.BestAsk, 5);
            Assert.Equal(2, crossRate.Value.OriginalOrderBooks.Count);
        }

        [Fact]
        public void ReverseConversionFirstPairTest()
        {
            var wantedCurrencies = new List<string> { "BTC" };
            const string baseCurrency = "USD";
            const string exchange = "Lykke";
            const string btcusd = "BTCUSD";

            var arbitrageCalculator = new ArbitrageCalculator(null, wantedCurrencies, baseCurrency, 10, 10);

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

            var crossRates = arbitrageCalculator.CalculateCrossRates();
            Assert.Single(crossRates);
            var crossRate = crossRates.First();
            Assert.Equal(exchange, crossRate.Key.Source);
            Assert.Equal(btcusd, crossRate.Key.AssetPair);
            Assert.Equal(exchange, crossRate.Value.Source);
            Assert.Equal(btcusd, crossRate.Value.AssetPair);
            Assert.Equal(10769.1475m, crossRate.Value.BestBid, 5);
            Assert.Equal(10982.90898m, crossRate.Value.BestAsk, 5);
            Assert.Equal(2, crossRate.Value.OriginalOrderBooks.Count);
        }

        [Fact]
        public void ReverseConversionSecondPairTest()
        {
            var wantedCurrencies = new List<string> { "BTC" };
            const string baseCurrency = "USD";
            const string exchange = "Lykke";
            const string btcusd = "BTCUSD";

            var arbitrageCalculator = new ArbitrageCalculator(null, wantedCurrencies, baseCurrency, 10, 10);

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

            var crossRates = arbitrageCalculator.CalculateCrossRates();
            Assert.Single(crossRates);
            var crossRate = crossRates.First();
            Assert.Equal(exchange, crossRate.Key.Source);
            Assert.Equal(btcusd, crossRate.Key.AssetPair);
            Assert.Equal(exchange, crossRate.Value.Source);
            Assert.Equal(btcusd, crossRate.Value.AssetPair);
            Assert.Equal(10769.1475m, crossRate.Value.BestBid, 5);
            Assert.Equal(10982.90898m, crossRate.Value.BestAsk, 5);
            Assert.Equal(2, crossRate.Value.OriginalOrderBooks.Count);
        }

        [Fact]
        public void ReverseConversionBothPairsTest()
        {
            var wantedCurrencies = new List<string> { "BTC" };
            const string baseCurrency = "USD";
            const string exchange = "Lykke";
            const string btcusd = "BTCUSD";

            var arbitrageCalculator = new ArbitrageCalculator(null, wantedCurrencies, baseCurrency, 10, 10);

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

            var crossRates = arbitrageCalculator.CalculateCrossRates();
            Assert.Single(crossRates);
            var crossRate = crossRates.First();
            Assert.Equal(exchange, crossRate.Key.Source);
            Assert.Equal(btcusd, crossRate.Key.AssetPair);
            Assert.Equal(exchange, crossRate.Value.Source);
            Assert.Equal(btcusd, crossRate.Value.AssetPair);
            Assert.Equal(10769.1475m, crossRate.Value.BestBid, 5);
            Assert.Equal(10982.90898m, crossRate.Value.BestAsk, 5);
            Assert.Equal(2, crossRate.Value.OriginalOrderBooks.Count);
        }
    }
}
