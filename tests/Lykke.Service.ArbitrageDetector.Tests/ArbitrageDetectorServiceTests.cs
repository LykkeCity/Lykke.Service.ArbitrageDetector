using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Common.Log;
using Lykke.Service.ArbitrageDetector.Core;
using Lykke.Service.ArbitrageDetector.Core.Domain;
using Lykke.Service.ArbitrageDetector.Core.Repositories;
using Lykke.Service.ArbitrageDetector.Services;
using Moq;
using Xunit;

namespace Lykke.Service.ArbitrageDetector.Tests
{
    public class ArbitrageDetectorServiceTests
    {
        private const bool performance = false;

        [Fact]
        public async Task From2OrderBooks_0_0_Test()
        {
            // BTCEUR * EURUSD
            var baseAssets = new List<string> { "BTC" };
            const string quoteAsset = "USD";
            const string exchange = "FE";
            const string btceur = "BTCEUR";
            const string eurusd = "EURUSD";
            const string btcusd = "BTCUSD";

            var settings = GetSettings(baseAssets, quoteAsset, -20);
            var arbitrageCalculator = new ArbitrageDetectorService(new LogToConsole(), null, SettingsRepository(settings));

            var btcEurOrderBook = new OrderBook(exchange, btceur,
                new List<VolumePrice> // bids
                {
                    new VolumePrice(8825, 10),
                    new VolumePrice(8823, 10)
                },
                new List<VolumePrice> // asks
                {
                    new VolumePrice(8999.95m, 10),
                    new VolumePrice(9000, 10),
                    new VolumePrice(9100, 10)
                },
                DateTime.UtcNow);

            var eurUsdOrderBook = new OrderBook(exchange, eurusd,
                new List<VolumePrice> // bids
                {
                    new VolumePrice(1.2203m, 10),
                    new VolumePrice(1.2201m, 10)
                },
                new List<VolumePrice> // asks
                {
                    new VolumePrice(1.22033m, 10),
                    new VolumePrice(1.22035m, 10),
                    new VolumePrice(1.22040m, 10)
                },
                DateTime.UtcNow);

            var btcUsdOrderBook = new OrderBook(exchange, btcusd,
                new List<VolumePrice> // bids
                {
                    new VolumePrice(10760, 10),
                    new VolumePrice(10768, 10)
                },
                new List<VolumePrice> // asks
                {
                    new VolumePrice(10761, 10),
                    new VolumePrice(10762, 10),
                    new VolumePrice(10763, 10)
                },
                DateTime.UtcNow);

            arbitrageCalculator.Process(btcEurOrderBook);
            arbitrageCalculator.Process(eurUsdOrderBook);
            arbitrageCalculator.Process(btcUsdOrderBook);

            await arbitrageCalculator.Execute();

            var crossRates = arbitrageCalculator.GetCrossRates().ToList();
            var arbitrages = arbitrageCalculator.GetArbitrages().ToList();

            Assert.Equal(2, crossRates.Count);
            Assert.Equal(2, arbitrages.Count);

            var arbitrage1 = arbitrages.Single(x => x.ConversionPath == "(FE-BTCUSD) > (FE-BTCUSD)");
            Assert.Equal(70, arbitrage1.PnL);
            Assert.Equal(-0.06500743m, arbitrage1.Spread, 8);
            Assert.Equal(10, arbitrage1.Volume);
            Assert.NotEqual(default, arbitrage1.StartedAt);
            Assert.Equal(default, arbitrage1.EndedAt);
            Assert.NotEqual(default, arbitrage1.Lasted);
            Assert.Equal(10768, arbitrage1.Bid.Price);
            Assert.Equal(10, arbitrage1.Bid.Volume);
            Assert.Equal(10761, arbitrage1.Ask.Price);
            Assert.Equal(10, arbitrage1.Ask.Volume);

            var arbitrage2 = arbitrages.Single(x => x.ConversionPath == "(FE-BTCEUR * FE-EURUSD) > (FE-BTCUSD)");
            Assert.Equal(0.00923229m, arbitrage2.PnL, 8);
            Assert.Equal(-0.07565594m, arbitrage2.Spread, 8);
            Assert.Equal(0.00113314m, arbitrage2.Volume, 8);
            Assert.NotEqual(default, arbitrage2.StartedAt);
            Assert.Equal(default, arbitrage2.EndedAt);
            Assert.NotEqual(default, arbitrage2.Lasted);
            Assert.Equal(10769.1475m, arbitrage2.Bid.Price, 8);
            Assert.Equal(0.00113314m, arbitrage2.Bid.Volume, 8);
            Assert.Equal(10761, arbitrage2.Ask.Price, 8);
            Assert.Equal(10, arbitrage2.Ask.Volume, 8);
        }

        [Fact]
        public async Task From2OrderBooks_0_1_Test()
        {
            // BTCEUR * USDEUR
            var baseAssets = new List<string> { "BTC" };
            const string quoteAsset = "USD";
            const string exchange = "FE";
            const string btceur = "BTCEUR";
            const string usdeur = "USDEUR";
            const string btcusd = "BTCUSD";

            var settings = GetSettings(baseAssets, quoteAsset, -20);
            var arbitrageCalculator = new ArbitrageDetectorService(new LogToConsole(), null, SettingsRepository(settings));

            var btcEurOrderBook = new OrderBook(exchange, btceur,
                new List<VolumePrice> // bids
                {
                    new VolumePrice(8825, 10),
                    new VolumePrice(8823, 10)
                },
                new List<VolumePrice> // asks
                {
                    new VolumePrice(8999.95m, 10),
                    new VolumePrice(9000, 10),
                    new VolumePrice(9100, 10)
                },
                DateTime.UtcNow);

            var usdEurOrderBook = new OrderBook(exchange, usdeur,
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

            var btcUsdOrderBook = new OrderBook(exchange, btcusd,
                new List<VolumePrice> // bids
                {
                    new VolumePrice(10760, 10),
                    new VolumePrice(10768, 10)
                },
                new List<VolumePrice> // asks
                {
                    new VolumePrice(10761, 10),
                    new VolumePrice(10762, 10),
                    new VolumePrice(10763, 10)
                },
                DateTime.UtcNow);

            arbitrageCalculator.Process(btcEurOrderBook);
            arbitrageCalculator.Process(usdEurOrderBook);
            arbitrageCalculator.Process(btcUsdOrderBook);

            await arbitrageCalculator.Execute();

            var crossRates = arbitrageCalculator.GetCrossRates().ToList();
            var arbitrages = arbitrageCalculator.GetArbitrages().ToList();

            Assert.Equal(2, crossRates.Count);
            Assert.Equal(2, arbitrages.Count);

            var arbitrage1 = arbitrages.Single(x => x.ConversionPath == "(FE-BTCUSD) > (FE-BTCUSD)");
            Assert.Equal(70, arbitrage1.PnL);
            Assert.Equal(-0.06500743m, arbitrage1.Spread, 8);
            Assert.Equal(10, arbitrage1.Volume);
            Assert.NotEqual(default, arbitrage1.StartedAt);
            Assert.Equal(default, arbitrage1.EndedAt);
            Assert.NotEqual(default, arbitrage1.Lasted);
            Assert.Equal(10768, arbitrage1.Bid.Price);
            Assert.Equal(10, arbitrage1.Bid.Volume);
            Assert.Equal(10761, arbitrage1.Ask.Price);
            Assert.Equal(10, arbitrage1.Ask.Volume);

            var arbitrage2 = arbitrages.Single(x => x.ConversionPath == "(FE-BTCEUR * FE-USDEUR) > (FE-BTCUSD)");
            Assert.Equal(0.00756559m, arbitrage2.PnL, 8);
            Assert.Equal(-0.07565594m, arbitrage2.Spread, 8);
            Assert.Equal(0.00092858m, arbitrage2.Volume, 8);
            Assert.NotEqual(default, arbitrage2.StartedAt);
            Assert.Equal(default, arbitrage2.EndedAt);
            Assert.NotEqual(default, arbitrage2.Lasted);
            Assert.Equal(10769.1475m, arbitrage2.Bid.Price, 8);
            Assert.Equal(0.00092858m, arbitrage2.Bid.Volume, 8);
            Assert.Equal(10761, arbitrage2.Ask.Price, 8);
            Assert.Equal(10, arbitrage2.Ask.Volume, 8);
        }

        [Fact]
        public async Task From2OrderBooks_1_0_Test()
        {
            // EURBTC * EURUSD
            var baseAssets = new List<string> { "BTC" };
            const string quoteAsset = "USD";
            const string exchange = "FE";
            const string eurbtc = "EURBTC";
            const string eurusd = "EURUSD";
            const string btcusd = "BTCUSD";

            var settings = GetSettings(baseAssets, quoteAsset, -20);
            var arbitrageCalculator = new ArbitrageDetectorService(new LogToConsole(), null, SettingsRepository(settings));

            var btcEurOrderBook = new OrderBook(exchange, eurbtc,
                new List<VolumePrice> // bids
                {
                    new VolumePrice(1/8999.95m, 10),
                    new VolumePrice(1/9000m, 10)
                },
                new List<VolumePrice> // asks
                {
                    new VolumePrice(1/8825m, 10),
                    new VolumePrice(1/8823m, 10),
                    new VolumePrice(9100, 10)
                },
                DateTime.UtcNow);

            var eurUsdOrderBook = new OrderBook(exchange, eurusd,
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

            var btcUsdOrderBook = new OrderBook(exchange, btcusd,
                new List<VolumePrice> // bids
                {
                    new VolumePrice(10760, 10),
                    new VolumePrice(10768, 10)
                },
                new List<VolumePrice> // asks
                {
                    new VolumePrice(10761, 10),
                    new VolumePrice(10762, 10),
                    new VolumePrice(10763, 10)
                },
                DateTime.UtcNow);

            arbitrageCalculator.Process(btcEurOrderBook);
            arbitrageCalculator.Process(eurUsdOrderBook);
            arbitrageCalculator.Process(btcUsdOrderBook);

            await arbitrageCalculator.Execute();

            var crossRates = arbitrageCalculator.GetCrossRates().ToList();
            var arbitrages = arbitrageCalculator.GetArbitrages().ToList();

            Assert.Equal(2, crossRates.Count);
            Assert.Equal(2, arbitrages.Count);

            var arbitrage1 = arbitrages.Single(x => x.ConversionPath == "(FE-BTCUSD) > (FE-BTCUSD)");
            Assert.Equal(70, arbitrage1.PnL);
            Assert.Equal(-0.06500743m, arbitrage1.Spread, 8);
            Assert.Equal(10, arbitrage1.Volume);
            Assert.NotEqual(default, arbitrage1.StartedAt);
            Assert.Equal(default, arbitrage1.EndedAt);
            Assert.NotEqual(default, arbitrage1.Lasted);
            Assert.Equal(10768, arbitrage1.Bid.Price);
            Assert.Equal(10, arbitrage1.Bid.Volume);
            Assert.Equal(10761, arbitrage1.Ask.Price);
            Assert.Equal(10, arbitrage1.Ask.Volume);

            var arbitrage2 = arbitrages.Single(x => x.ConversionPath == "(FE-EURBTC * FE-EURUSD) > (FE-BTCUSD)");
            Assert.Equal(0.00923229m, arbitrage2.PnL, 8);
            Assert.Equal(-0.07565594m, arbitrage2.Spread, 8);
            Assert.Equal(0.00113314m, arbitrage2.Volume, 8);
            Assert.NotEqual(default, arbitrage2.StartedAt);
            Assert.Equal(default, arbitrage2.EndedAt);
            Assert.NotEqual(default, arbitrage2.Lasted);
            Assert.Equal(10769.1475m, arbitrage2.Bid.Price, 8);
            Assert.Equal(0.00113314m, arbitrage2.Bid.Volume, 8);
            Assert.Equal(10761, arbitrage2.Ask.Price, 8);
            Assert.Equal(10, arbitrage2.Ask.Volume, 8);
        }

        [Fact]
        public async Task From2OrderBooks_1_1_Test()
        {
            // EURBTC * USDEUR
            var baseAssets = new List<string> { "BTC" };
            const string quoteAsset = "USD";
            const string exchange = "FE";
            const string eurbtc = "EURBTC";
            const string usdeur = "USDEUR";
            const string btcusd = "BTCUSD";

            var settings = GetSettings(baseAssets, quoteAsset, -20);
            var arbitrageCalculator = new ArbitrageDetectorService(new LogToConsole(), null, SettingsRepository(settings));

            var eurBtcOrderBook = new OrderBook(exchange, eurbtc,
                new List<VolumePrice> // bids
                {
                    new VolumePrice(1/8999.95m, 10),
                    new VolumePrice(1/9000m, 10)
                },
                new List<VolumePrice> // asks
                {
                    new VolumePrice(1/8825m, 10),
                    new VolumePrice(1/8823m, 10),
                    new VolumePrice(9100, 10)
                },
                DateTime.UtcNow);

            var usdEurOrderBook = new OrderBook(exchange, usdeur,
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

            var btcUsdOrderBook = new OrderBook(exchange, btcusd,
                new List<VolumePrice> // bids
                {
                    new VolumePrice(10760, 10),
                    new VolumePrice(10768, 10)
                },
                new List<VolumePrice> // asks
                {
                    new VolumePrice(10761, 10),
                    new VolumePrice(10762, 10),
                    new VolumePrice(10763, 10)
                },
                DateTime.UtcNow);

            arbitrageCalculator.Process(eurBtcOrderBook);
            arbitrageCalculator.Process(usdEurOrderBook);
            arbitrageCalculator.Process(btcUsdOrderBook);

            await arbitrageCalculator.Execute();

            var crossRates = arbitrageCalculator.GetCrossRates().ToList();
            var arbitrages = arbitrageCalculator.GetArbitrages().ToList();

            Assert.Equal(2, crossRates.Count);
            Assert.Equal(2, arbitrages.Count);

            var arbitrage1 = arbitrages.Single(x => x.ConversionPath == "(FE-BTCUSD) > (FE-BTCUSD)");
            Assert.Equal(70, arbitrage1.PnL);
            Assert.Equal(-0.06500743m, arbitrage1.Spread, 8);
            Assert.Equal(10, arbitrage1.Volume);
            Assert.NotEqual(default, arbitrage1.StartedAt);
            Assert.Equal(default, arbitrage1.EndedAt);
            Assert.NotEqual(default, arbitrage1.Lasted);
            Assert.Equal(10768, arbitrage1.Bid.Price);
            Assert.Equal(10, arbitrage1.Bid.Volume);
            Assert.Equal(10761, arbitrage1.Ask.Price);
            Assert.Equal(10, arbitrage1.Ask.Volume);

            var arbitrage2 = arbitrages.Single(x => x.ConversionPath == "(FE-EURBTC * FE-USDEUR) > (FE-BTCUSD)");
            Assert.Equal(0.00756559m, arbitrage2.PnL, 8);
            Assert.Equal(-0.07565594m, arbitrage2.Spread, 8);
            Assert.Equal(0.00092858m, arbitrage2.Volume, 8);
            Assert.NotEqual(default, arbitrage2.StartedAt);
            Assert.Equal(default, arbitrage2.EndedAt);
            Assert.NotEqual(default, arbitrage2.Lasted);
            Assert.Equal(10769.1475m, arbitrage2.Bid.Price, 8);
            Assert.Equal(0.00092858m, arbitrage2.Bid.Volume, 8);
            Assert.Equal(10761, arbitrage2.Ask.Price, 8);
            Assert.Equal(10, arbitrage2.Ask.Volume, 8);
        }



        //[Fact]
        public async Task From3OrderBooks_0_0_0_Test()
        {
            var baseAssets = new List<string> { "BTC" };
            const string quoteAsset = "USD";
            const string exchange1 = "TEST1";
            const string exchange2 = "TEST2";
            const string exchange3 = "TEST3";
            const string exchange4 = "TEST4";
            const string btcEur = "BTCEUR";
            const string eurJpy = "EURJPY";
            const string jpyUsd = "JPYUSD";
            const string btcUsd = "BTCUSD";
            var timestamp1 = DateTime.UtcNow.AddSeconds(-2);
            var timestamp2 = DateTime.UtcNow.AddSeconds(-1);
            var timestamp3 = DateTime.UtcNow;

            var settings = GetSettings(baseAssets, quoteAsset, 0);
            var arbitrageCalculator = new ArbitrageDetectorService(new LogToConsole(), null, SettingsRepository(settings));

            var btcEurOrderBook = new OrderBook(exchange1, btcEur,
                new List<VolumePrice> // bids
                {
                    new VolumePrice(7310m, 9),
                    new VolumePrice(7300m, 5)
                },
                new List<VolumePrice> // asks
                {
                    new VolumePrice(7320m, 10),
                    new VolumePrice(7330m, 7),
                    new VolumePrice(7340m, 3)
                },
                timestamp1);

            var eurJpyOrderBook = new OrderBook(exchange2, eurJpy,
                new List<VolumePrice> // bids
                {
                    new VolumePrice(131m, 9),
                    new VolumePrice(130m, 5)
                },
                new List<VolumePrice> // asks
                {
                    new VolumePrice(132m, 11),
                    new VolumePrice(133m, 7),
                    new VolumePrice(134m, 3)
                },
                timestamp2);
            eurJpyOrderBook.SetAssetPair("EUR");

            var jpyUsdOrderBook = new OrderBook(exchange3, jpyUsd,
                new List<VolumePrice> // bids
                {
                    new VolumePrice(0.009132m, 9),
                    new VolumePrice(0.009131m, 5)
                },
                new List<VolumePrice> // asks
                {
                    new VolumePrice(0.009133m, 12),
                    new VolumePrice(0.009134m, 7),
                    new VolumePrice(0.009135m, 3)
                },
                timestamp3);

            var btcUsdOrderBook = new OrderBook(exchange4, btcUsd,
                new List<VolumePrice> // bids
                {
                    new VolumePrice(8742, 7),
                    new VolumePrice(8743, 9)
                },
                new List<VolumePrice> // asks
                {
                    new VolumePrice(8744, 11),
                    new VolumePrice(8745, 13),
                    new VolumePrice(8746, 17)
                },
                DateTime.UtcNow);

            arbitrageCalculator.Process(btcEurOrderBook);
            arbitrageCalculator.Process(eurJpyOrderBook);
            arbitrageCalculator.Process(jpyUsdOrderBook);
            arbitrageCalculator.Process(btcUsdOrderBook);

            await arbitrageCalculator.Execute();

            var crossRates = arbitrageCalculator.GetCrossRates().ToList();
            var arbitrages = arbitrageCalculator.GetArbitrages().ToList();

            Assert.Equal(2, crossRates.Count);
            Assert.Equal(1, arbitrages.Count);

            var arbitrage1 = arbitrages.Single(x => x.ConversionPath == "(TEST1-BTCEUR * TEST2-EURJPY * TEST3-JPYUSD) > (TEST4-BTCUSD)");
            Assert.Equal(0.00000841m, arbitrage1.PnL, 8);
            Assert.Equal(-0.01022905m, arbitrage1.Spread, 8);
            Assert.Equal(0.0000094m, arbitrage1.Volume, 8);
            Assert.NotEqual(default, arbitrage1.StartedAt);
            Assert.Equal(default, arbitrage1.EndedAt);
            Assert.NotEqual(default, arbitrage1.Lasted);
            Assert.Equal(8744.894520m, arbitrage1.Bid.Price, 8);
            Assert.Equal(0.0000094m, arbitrage1.Bid.Volume, 8);
            Assert.Equal(8744, arbitrage1.Ask.Price, 8);
            Assert.Equal(11, arbitrage1.Ask.Volume, 8);
        }

        //[Fact]
        public async Task From3OrderBooks_0_0_1_Test()
        {
            var baseAssets = new List<string> { "BTC" };
            const string quoteAsset = "USD";
            const string exchange1 = "TEST1";
            const string exchange2 = "TEST2";
            const string exchange3 = "TEST3";
            const string exchange4 = "TEST4";
            const string btcEur = "BTCEUR";
            const string eurJpy = "EURJPY";
            const string usdJpy = "USDJPY";
            const string btcUsd = "BTCUSD";
            var timestamp1 = DateTime.UtcNow.AddSeconds(-2);
            var timestamp2 = DateTime.UtcNow.AddSeconds(-1);
            var timestamp3 = DateTime.UtcNow;

            var settings = GetSettings(baseAssets, quoteAsset, 0);
            var arbitrageCalculator = new ArbitrageDetectorService(new LogToConsole(), null, SettingsRepository(settings));

            var btcEurOrderBook = new OrderBook(exchange1, btcEur,
                new List<VolumePrice> // bids
                {
                    new VolumePrice(7310m, 9),
                    new VolumePrice(7300m, 5)
                },
                new List<VolumePrice> // asks
                {
                    new VolumePrice(7320m, 10),
                    new VolumePrice(7330m, 7),
                    new VolumePrice(7340m, 3)
                },
                timestamp1);

            var eurJpyOrderBook = new OrderBook(exchange2, eurJpy,
                new List<VolumePrice> // bids
                {
                    new VolumePrice(131m, 9),
                    new VolumePrice(130m, 5)
                },
                new List<VolumePrice> // asks
                {
                    new VolumePrice(132m, 11),
                    new VolumePrice(133m, 7),
                    new VolumePrice(134m, 3)
                },
                timestamp2);
            eurJpyOrderBook.SetAssetPair("EUR");

            var jpyUsdOrderBook = new OrderBook(exchange3, usdJpy,
                new List<VolumePrice> // bids
                {
                    new VolumePrice(1/0.009132m, 9),
                    new VolumePrice(1/0.009131m, 5)
                },
                new List<VolumePrice> // asks
                {
                    new VolumePrice(1/0.009133m, 12),
                    new VolumePrice(1/0.009134m, 7),
                    new VolumePrice(1/0.009135m, 3)
                },
                timestamp3);

            var btcUsdOrderBook = new OrderBook(exchange4, btcUsd,
                new List<VolumePrice> // bids
                {
                    new VolumePrice(1/8743m, 11),
                    new VolumePrice(1/8744m, 13),
                    new VolumePrice(1/8745m, 17)
                },
                new List<VolumePrice> // asks
                {
                    new VolumePrice(1/8741m, 7),
                    new VolumePrice(1/8742m, 9)
                },
                DateTime.UtcNow);

            arbitrageCalculator.Process(btcEurOrderBook);
            arbitrageCalculator.Process(eurJpyOrderBook);
            arbitrageCalculator.Process(jpyUsdOrderBook);
            arbitrageCalculator.Process(btcUsdOrderBook);

            await arbitrageCalculator.Execute();

            var crossRates = arbitrageCalculator.GetCrossRates().ToList();
            var arbitrages = arbitrageCalculator.GetArbitrages().ToList();

            Assert.Equal(2, crossRates.Count);
            Assert.Equal(1, arbitrages.Count);

            var arbitrage1 = arbitrages.Single(x => x.ConversionPath == "(TEST1-BTCEUR * TEST2-EURJPY * TEST3-USDJPY) > (TEST4-BTCUSD)");
            Assert.Equal(10.76780686m, arbitrage1.PnL, 8);
            Assert.Equal(-99.99999869m, arbitrage1.Spread, 8);
            Assert.Equal(0.00123119m, arbitrage1.Volume, 8);
            Assert.NotEqual(default, arbitrage1.StartedAt);
            Assert.Equal(default, arbitrage1.EndedAt);
            Assert.NotEqual(default, arbitrage1.Lasted);
            Assert.Equal(8745.852130m, arbitrage1.Bid.Price, 8);
            Assert.Equal(0.00123119m, arbitrage1.Bid.Volume, 8);
            Assert.Equal(0.00011439m, arbitrage1.Ask.Price, 8);
            Assert.Equal(9, arbitrage1.Ask.Volume, 8);
        }

        //[Fact]
        public async Task From3OrderBooks_0_1_0_Test()
        {
            var baseAssets = new List<string> { "BTC" };
            const string quoteAsset = "USD";
            const string exchange1 = "TEST1";
            const string exchange2 = "TEST2";
            const string exchange3 = "TEST3";
            const string exchange4 = "TEST4";
            const string btcEur = "BTCEUR";
            const string jpyEur = "JPYEUR";
            const string jpyUsd = "JPYUSD";
            const string btcUsd = "BTCUSD";
            var timestamp1 = DateTime.UtcNow.AddSeconds(-2);
            var timestamp2 = DateTime.UtcNow.AddSeconds(-1);
            var timestamp3 = DateTime.UtcNow;

            var settings = GetSettings(baseAssets, quoteAsset, 0);
            var arbitrageCalculator = new ArbitrageDetectorService(new LogToConsole(), null, SettingsRepository(settings));

            var btcEurOrderBook = new OrderBook(exchange1, btcEur,
                new List<VolumePrice> // bids
                {
                    new VolumePrice(7310m, 9),
                    new VolumePrice(7300m, 5)
                },
                new List<VolumePrice> // asks
                {
                    new VolumePrice(7320m, 10),
                    new VolumePrice(7330m, 7),
                    new VolumePrice(7340m, 3)
                },
                timestamp1);

            var eurJpyOrderBook = new OrderBook(exchange2, jpyEur,
                new List<VolumePrice> // bids
                {
                    new VolumePrice(1/132m, 11),
                    new VolumePrice(1/133m, 7),
                    new VolumePrice(1/134m, 3)
                },
                new List<VolumePrice> // asks
                {
                    new VolumePrice(1/131m, 9),
                    new VolumePrice(1/130m, 5)
                },
                timestamp2);
            eurJpyOrderBook.SetAssetPair("EUR");

            var jpyUsdOrderBook = new OrderBook(exchange3, jpyUsd,
                new List<VolumePrice> // bids
                {
                    new VolumePrice(0.009132m, 9),
                    new VolumePrice(0.009131m, 5)
                },
                new List<VolumePrice> // asks
                {
                    new VolumePrice(0.009133m, 12),
                    new VolumePrice(0.009134m, 7),
                    new VolumePrice(0.009135m, 3)
                },
                timestamp3);

            var btcUsdOrderBook = new OrderBook(exchange4, btcUsd,
                new List<VolumePrice> // bids
                {
                    new VolumePrice(8741, 7),
                    new VolumePrice(8742, 9)
                },
                new List<VolumePrice> // asks
                {
                    new VolumePrice(8743, 11),
                    new VolumePrice(8744, 13),
                    new VolumePrice(8745, 17)
                },
                DateTime.UtcNow);

            arbitrageCalculator.Process(btcEurOrderBook);
            arbitrageCalculator.Process(eurJpyOrderBook);
            arbitrageCalculator.Process(jpyUsdOrderBook);
            arbitrageCalculator.Process(btcUsdOrderBook);

            await arbitrageCalculator.Execute();

            var crossRates = arbitrageCalculator.GetCrossRates().ToList();
            var arbitrages = arbitrageCalculator.GetArbitrages().ToList();

            Assert.Equal(2, crossRates.Count);
            Assert.Equal(1, arbitrages.Count);

            var arbitrage1 = arbitrages.Single(x => x.ConversionPath == "(TEST1-BTCEUR * TEST2-JPYEUR * TEST3-JPYUSD) > (TEST4-BTCUSD)");
            Assert.Equal(0.00001781m, arbitrage1.PnL, 8);
            Assert.Equal(-0.0216643m, arbitrage1.Spread, 8);
            Assert.Equal(0.0000094m, arbitrage1.Volume, 8);
            Assert.NotEqual(default, arbitrage1.StartedAt);
            Assert.Equal(default, arbitrage1.EndedAt);
            Assert.NotEqual(default, arbitrage1.Lasted);
            Assert.Equal(8744.894520m, arbitrage1.Bid.Price, 8);
            Assert.Equal(0.0000094m, arbitrage1.Bid.Volume, 8);
            Assert.Equal(8743, arbitrage1.Ask.Price, 8);
            Assert.Equal(11, arbitrage1.Ask.Volume, 8);
        }

        //[Fact]
        public async Task From3OrderBooks_1_0_0_Test()
        {
            var baseAssets = new List<string> { "BTC" };
            const string quoteAsset = "USD";
            const string exchange1 = "TEST1";
            const string exchange2 = "TEST2";
            const string exchange3 = "TEST3";
            const string exchange4 = "TEST4";
            const string eurBtc = "EURBTC";
            const string eurJpy = "EURJPY";
            const string jpyUsd = "JPYUSD";
            const string btcUsd = "BTCUSD";
            var timestamp1 = DateTime.UtcNow.AddSeconds(-2);
            var timestamp2 = DateTime.UtcNow.AddSeconds(-1);
            var timestamp3 = DateTime.UtcNow;

            var settings = GetSettings(baseAssets, quoteAsset, 0);
            var arbitrageCalculator = new ArbitrageDetectorService(new LogToConsole(), null, SettingsRepository(settings));

            var btcEurOrderBook = new OrderBook(exchange1, eurBtc,
                new List<VolumePrice> // bids
                {
                    new VolumePrice(1/7320m, 10),
                    new VolumePrice(1/7330m, 7),
                    new VolumePrice(1/7340m, 3)
                },
                new List<VolumePrice> // asks
                {
                    new VolumePrice(1/7310m, 9),
                    new VolumePrice(1/7300m, 5)
                },
                timestamp1);

            var eurJpyOrderBook = new OrderBook(exchange2, eurJpy,
                new List<VolumePrice> // bids
                {
                    new VolumePrice(131m, 9),
                    new VolumePrice(130m, 5)
                },
                new List<VolumePrice> // asks
                {
                    new VolumePrice(132m, 11),
                    new VolumePrice(133m, 7),
                    new VolumePrice(134m, 3)
                },
                timestamp2);
            eurJpyOrderBook.SetAssetPair("EUR");

            var jpyUsdOrderBook = new OrderBook(exchange3, jpyUsd,
                new List<VolumePrice> // bids
                {
                    new VolumePrice(0.009132m, 9),
                    new VolumePrice(0.009131m, 5)
                },
                new List<VolumePrice> // asks
                {
                    new VolumePrice(0.009133m, 12),
                    new VolumePrice(0.009134m, 7),
                    new VolumePrice(0.009135m, 3)
                },
                timestamp3);

            var btcUsdOrderBook = new OrderBook(exchange4, btcUsd,
                new List<VolumePrice> // bids
                {
                    new VolumePrice(8741, 7),
                    new VolumePrice(8742, 9)
                },
                new List<VolumePrice> // asks
                {
                    new VolumePrice(8743, 11),
                    new VolumePrice(8744, 13),
                    new VolumePrice(8745, 17)
                },
                DateTime.UtcNow);

            arbitrageCalculator.Process(btcEurOrderBook);
            arbitrageCalculator.Process(eurJpyOrderBook);
            arbitrageCalculator.Process(jpyUsdOrderBook);
            arbitrageCalculator.Process(btcUsdOrderBook);

            await arbitrageCalculator.Execute();

            var crossRates = arbitrageCalculator.GetCrossRates().ToList();
            var arbitrages = arbitrageCalculator.GetArbitrages().ToList();

            Assert.Equal(2, crossRates.Count);
            Assert.Equal(1, arbitrages.Count);

            var arbitrage1 = arbitrages.Single(x => x.ConversionPath == "(TEST1-EURBTC * TEST2-EURJPY * TEST3-JPYUSD) > (TEST4-BTCUSD)");
            Assert.Equal(0.00001781m, arbitrage1.PnL, 8);
            Assert.Equal(-0.0216643m, arbitrage1.Spread, 8);
            Assert.Equal(0.0000094m, arbitrage1.Volume, 8);
            Assert.NotEqual(default, arbitrage1.StartedAt);
            Assert.Equal(default, arbitrage1.EndedAt);
            Assert.NotEqual(default, arbitrage1.Lasted);
            Assert.Equal(8744.894520m, arbitrage1.Bid.Price, 8);
            Assert.Equal(0.0000094m, arbitrage1.Bid.Volume, 8);
            Assert.Equal(8743, arbitrage1.Ask.Price, 8);
            Assert.Equal(11, arbitrage1.Ask.Volume, 8);
        }

        //[Fact]
        public async Task From3OrderBooks_0_1_1_Test()
        {
            var baseAssets = new List<string> { "BTC" };
            const string quoteAsset = "USD";
            const string exchange1 = "TEST1";
            const string exchange2 = "TEST2";
            const string exchange3 = "TEST3";
            const string exchange4 = "TEST4";
            const string btcEur = "BTCEUR";
            const string jpyEur = "JPYEUR";
            const string usdJpy = "USDJPY";
            const string btcUsd = "BTCUSD";
            var timestamp1 = DateTime.UtcNow.AddSeconds(-2);
            var timestamp2 = DateTime.UtcNow.AddSeconds(-1);
            var timestamp3 = DateTime.UtcNow;

            var settings = GetSettings(baseAssets, quoteAsset, 0);
            var arbitrageCalculator = new ArbitrageDetectorService(new LogToConsole(), null, SettingsRepository(settings));

            var btcEurOrderBook = new OrderBook(exchange1, btcEur,
                new List<VolumePrice> // bids
                {
                    new VolumePrice(7310m, 9),
                    new VolumePrice(7300m, 5)
                },
                new List<VolumePrice> // asks
                {
                    new VolumePrice(7320m, 10),
                    new VolumePrice(7330m, 7),
                    new VolumePrice(7340m, 3)
                },
                timestamp1);

            var eurJpyOrderBook = new OrderBook(exchange2, jpyEur,
                new List<VolumePrice> // bids
                {
                    new VolumePrice(1/132m, 11),
                    new VolumePrice(1/133m, 7),
                    new VolumePrice(1/134m, 3)
                },
                new List<VolumePrice> // asks
                {
                    new VolumePrice(1/131m, 9),
                    new VolumePrice(1/130m, 5)
                },
                timestamp2);
            eurJpyOrderBook.SetAssetPair("EUR");

            var jpyUsdOrderBook = new OrderBook(exchange3, usdJpy,
                new List<VolumePrice> // bids
                {
                    new VolumePrice(1/0.009132m, 9),
                    new VolumePrice(1/0.009131m, 5)
                },
                new List<VolumePrice> // asks
                {
                    new VolumePrice(1/0.009133m, 12),
                    new VolumePrice(1/0.009134m, 7),
                    new VolumePrice(1/0.009135m, 3)
                },
                timestamp3);

            var btcUsdOrderBook = new OrderBook(exchange4, btcUsd,
                new List<VolumePrice> // bids
                {
                    new VolumePrice(1/8743m, 11),
                    new VolumePrice(1/8744m, 13),
                    new VolumePrice(1/8745m, 17)
                },
                new List<VolumePrice> // asks
                {
                    new VolumePrice(1/8741m, 7),
                    new VolumePrice(1/8742m, 9)
                },
                DateTime.UtcNow);

            arbitrageCalculator.Process(btcEurOrderBook);
            arbitrageCalculator.Process(eurJpyOrderBook);
            arbitrageCalculator.Process(jpyUsdOrderBook);
            arbitrageCalculator.Process(btcUsdOrderBook);

            await arbitrageCalculator.Execute();

            var crossRates = arbitrageCalculator.GetCrossRates().ToList();
            var arbitrages = arbitrageCalculator.GetArbitrages().ToList();

            Assert.Equal(2, crossRates.Count);
            Assert.Equal(1, arbitrages.Count);

            var arbitrage1 = arbitrages.Single(x => x.ConversionPath == "(TEST1-BTCEUR * TEST2-JPYEUR * TEST3-USDJPY) > (TEST4-BTCUSD)");
            Assert.Equal(0.082215m, arbitrage1.PnL, 8);
            Assert.Equal(-99.99999869m, arbitrage1.Spread, 8);
            Assert.Equal(0.0000094m, arbitrage1.Volume, 8);
            Assert.NotEqual(default, arbitrage1.StartedAt);
            Assert.Equal(default, arbitrage1.EndedAt);
            Assert.NotEqual(default, arbitrage1.Lasted);
            Assert.Equal(8747.76735m, arbitrage1.Bid.Price, 8);
            Assert.Equal(0.0000094m, arbitrage1.Bid.Volume, 8);
            Assert.Equal(0.00011439m, arbitrage1.Ask.Price, 8);
            Assert.Equal(9, arbitrage1.Ask.Volume, 8);
        }

        //[Fact]
        public async Task From3OrderBooks_1_0_1_Test()
        {
            var baseAssets = new List<string> { "BTC" };
            const string quoteAsset = "USD";
            const string exchange1 = "TEST1";
            const string exchange2 = "TEST2";
            const string exchange3 = "TEST3";
            const string exchange4 = "TEST4";
            const string eurBtc = "EURBTC";
            const string eurJpy = "EURJPY";
            const string usdJpy = "USDJPY";
            const string btcUsd = "BTCUSD";
            var timestamp1 = DateTime.UtcNow.AddSeconds(-2);
            var timestamp2 = DateTime.UtcNow.AddSeconds(-1);
            var timestamp3 = DateTime.UtcNow;

            var settings = GetSettings(baseAssets, quoteAsset, 0);
            var arbitrageCalculator = new ArbitrageDetectorService(new LogToConsole(), null, SettingsRepository(settings));

            var btcEurOrderBook = new OrderBook(exchange1, eurBtc,
                new List<VolumePrice> // bids
                {
                    new VolumePrice(1/7320m, 10),
                    new VolumePrice(1/7330m, 7),
                    new VolumePrice(1/7340m, 3)
                },
                new List<VolumePrice> // asks
                {
                    new VolumePrice(1/7310m, 9),
                    new VolumePrice(1/7300m, 5)
                },
                timestamp1);

            var eurJpyOrderBook = new OrderBook(exchange2, eurJpy,
                new List<VolumePrice> // bids
                {
                    new VolumePrice(131m, 9),
                    new VolumePrice(130m, 5)
                },
                new List<VolumePrice> // asks
                {
                    new VolumePrice(132m, 11),
                    new VolumePrice(133m, 7),
                    new VolumePrice(134m, 3)
                },
                timestamp2);
            eurJpyOrderBook.SetAssetPair("EUR");

            var jpyUsdOrderBook = new OrderBook(exchange3, usdJpy,
                new List<VolumePrice> // bids
                {
                    new VolumePrice(1/0.009133m, 12),
                    new VolumePrice(1/0.009134m, 7),
                    new VolumePrice(1/0.009135m, 3)
                },
                new List<VolumePrice> // asks
                {
                    new VolumePrice(1/0.009132m, 9),
                    new VolumePrice(1/0.009131m, 5)
                },
                timestamp3);

            var btcUsdOrderBook = new OrderBook(exchange4, btcUsd,
                new List<VolumePrice> // bids
                {
                    new VolumePrice(8741, 7),
                    new VolumePrice(8742, 9)
                },
                new List<VolumePrice> // asks
                {
                    new VolumePrice(8743, 11),
                    new VolumePrice(8744, 13),
                    new VolumePrice(8745, 17)
                },
                DateTime.UtcNow);

            arbitrageCalculator.Process(btcEurOrderBook);
            arbitrageCalculator.Process(eurJpyOrderBook);
            arbitrageCalculator.Process(jpyUsdOrderBook);
            arbitrageCalculator.Process(btcUsdOrderBook);

            await arbitrageCalculator.Execute();

            var crossRates = arbitrageCalculator.GetCrossRates().ToList();
            var arbitrages = arbitrageCalculator.GetArbitrages().ToList();

            Assert.Equal(2, crossRates.Count);
            Assert.Equal(1, arbitrages.Count);

            var arbitrage1 = arbitrages.Single(x => x.ConversionPath == "(TEST1-EURBTC * TEST2-EURJPY * TEST3-USDJPY) > (TEST4-BTCUSD)");
            Assert.Equal(0.00194979m, arbitrage1.PnL, 8);
            Assert.Equal(-0.0216643m, arbitrage1.Spread, 8);
            Assert.Equal(0.00102917m, arbitrage1.Volume, 8);
            Assert.NotEqual(default, arbitrage1.StartedAt);
            Assert.Equal(default, arbitrage1.EndedAt);
            Assert.NotEqual(default, arbitrage1.Lasted);
            Assert.Equal(8744.894520m, arbitrage1.Bid.Price, 8);
            Assert.Equal(0.00102917m, arbitrage1.Bid.Volume, 8);
            Assert.Equal(8743, arbitrage1.Ask.Price, 8);
            Assert.Equal(11, arbitrage1.Ask.Volume, 8);
        }

        //[Fact]
        public async Task From3OrderBooks_1_1_0_Test()
        {
            var baseAssets = new List<string> { "BTC" };
            const string quoteAsset = "USD";
            const string exchange1 = "TEST1";
            const string exchange2 = "TEST2";
            const string exchange3 = "TEST3";
            const string exchange4 = "TEST4";
            const string eurBtc = "EURBTC";
            const string jpyEur = "JPYEUR";
            const string jpyUsd = "JPYUSD";
            const string btcUsd = "BTCUSD";
            var timestamp1 = DateTime.UtcNow.AddSeconds(-2);
            var timestamp2 = DateTime.UtcNow.AddSeconds(-1);
            var timestamp3 = DateTime.UtcNow;

            var settings = GetSettings(baseAssets, quoteAsset, 0);
            var arbitrageCalculator = new ArbitrageDetectorService(new LogToConsole(), null, SettingsRepository(settings));

            var btcEurOrderBook = new OrderBook(exchange1, eurBtc,
                new List<VolumePrice> // bids
                {
                    new VolumePrice(1/7320m, 10),
                    new VolumePrice(1/7330m, 7),
                    new VolumePrice(1/7340m, 3)
                },
                new List<VolumePrice> // asks
                {
                    new VolumePrice(1/7310m, 9),
                    new VolumePrice(1/7300m, 5)
                },
                timestamp1);

            var jpyEurOrderBook = new OrderBook(exchange2, jpyEur,
                new List<VolumePrice> // bids
                {
                    new VolumePrice(1/132m, 11),
                    new VolumePrice(1/133m, 7),
                    new VolumePrice(1/134m, 3)
                },
                new List<VolumePrice> // asks
                {
                    new VolumePrice(1/131m, 9),
                    new VolumePrice(1/130m, 5)
                },
                timestamp2);
            jpyEurOrderBook.SetAssetPair("EUR");

            var jpyUsdOrderBook = new OrderBook(exchange3, jpyUsd,
                new List<VolumePrice> // bids
                {
                    new VolumePrice(0.009132m, 9),
                    new VolumePrice(0.009131m, 5)
                },
                new List<VolumePrice> // asks
                {
                    new VolumePrice(0.009133m, 12),
                    new VolumePrice(0.009134m, 7),
                    new VolumePrice(0.009135m, 3)
                },
                timestamp3);

            var btcUsdOrderBook = new OrderBook(exchange4, btcUsd,
                new List<VolumePrice> // bids
                {
                    new VolumePrice(8741, 7),
                    new VolumePrice(8742, 9)
                },
                new List<VolumePrice> // asks
                {
                    new VolumePrice(8743, 11),
                    new VolumePrice(8744, 13),
                    new VolumePrice(8745, 17)
                },
                DateTime.UtcNow);

            arbitrageCalculator.Process(btcEurOrderBook);
            arbitrageCalculator.Process(jpyEurOrderBook);
            arbitrageCalculator.Process(jpyUsdOrderBook);
            arbitrageCalculator.Process(btcUsdOrderBook);

            await arbitrageCalculator.Execute();

            var crossRates = arbitrageCalculator.GetCrossRates().ToList();
            var arbitrages = arbitrageCalculator.GetArbitrages().ToList();

            Assert.Equal(2, crossRates.Count);
            Assert.Equal(1, arbitrages.Count);

            var arbitrage1 = arbitrages.Single(x => x.ConversionPath == "(TEST1-EURBTC * TEST2-JPYEUR * TEST3-JPYUSD) > (TEST4-BTCUSD)");
            Assert.Equal(0.00001781m, arbitrage1.PnL, 8);
            Assert.Equal(-0.0216643m, arbitrage1.Spread, 8);
            Assert.Equal(0.0000094m, arbitrage1.Volume, 8);
            Assert.NotEqual(default, arbitrage1.StartedAt);
            Assert.Equal(default, arbitrage1.EndedAt);
            Assert.NotEqual(default, arbitrage1.Lasted);
            Assert.Equal(8744.894520m, arbitrage1.Bid.Price, 8);
            Assert.Equal(0.0000094m, arbitrage1.Bid.Volume, 8);
            Assert.Equal(8743, arbitrage1.Ask.Price, 8);
            Assert.Equal(11, arbitrage1.Ask.Volume, 8);
        }

        //[Fact]
        public async Task From3OrderBooks_1_1_1_Test()
        {
            var baseAssets = new List<string> { "BTC" };
            const string quoteAsset = "USD";
            const string exchange1 = "TEST1";
            const string exchange2 = "TEST2";
            const string exchange3 = "TEST3";
            const string exchange4 = "TEST4";
            const string eurBtc = "EURBTC";
            const string jpyEur = "JPYEUR";
            const string usdJpy = "USDJPY";
            const string btcUsd = "BTCUSD";
            var timestamp1 = DateTime.UtcNow.AddSeconds(-2);
            var timestamp2 = DateTime.UtcNow.AddSeconds(-1);
            var timestamp3 = DateTime.UtcNow;

            var settings = GetSettings(baseAssets, quoteAsset, 0);
            var arbitrageCalculator = new ArbitrageDetectorService(new LogToConsole(), null, SettingsRepository(settings));

            var btcEurOrderBook = new OrderBook(exchange1, eurBtc,
                new List<VolumePrice> // bids
                {
                    new VolumePrice(1/7320m, 10),
                    new VolumePrice(1/7330m, 7),
                    new VolumePrice(1/7340m, 3)
                },
                new List<VolumePrice> // asks
                {
                    new VolumePrice(1/7310m, 9),
                    new VolumePrice(1/7300m, 5)
                },
                timestamp1);

            var jpyEurOrderBook = new OrderBook(exchange2, jpyEur,
                new List<VolumePrice> // bids
                {
                    new VolumePrice(1/132m, 11),
                    new VolumePrice(1/133m, 7),
                    new VolumePrice(1/134m, 3)
                },
                new List<VolumePrice> // asks
                {
                    new VolumePrice(1/131m, 9),
                    new VolumePrice(1/130m, 5)
                },
                timestamp2);
            jpyEurOrderBook.SetAssetPair("EUR");

            var usdJpyOrderBook = new OrderBook(exchange3, usdJpy,
                new List<VolumePrice> // bids
                {
                    new VolumePrice(1/0.009133m, 12),
                    new VolumePrice(1/0.009134m, 7),
                    new VolumePrice(1/0.009135m, 3)
                },
                new List<VolumePrice> // asks
                {
                    new VolumePrice(1/0.009132m, 9),
                    new VolumePrice(1/0.009131m, 5)
                },
                timestamp3);

            var btcUsdOrderBook = new OrderBook(exchange4, btcUsd,
                new List<VolumePrice> // bids
                {
                    new VolumePrice(8741, 7),
                    new VolumePrice(8742, 9)
                },
                new List<VolumePrice> // asks
                {
                    new VolumePrice(8743, 11),
                    new VolumePrice(8744, 13),
                    new VolumePrice(8745, 17)
                },
                DateTime.UtcNow);

            arbitrageCalculator.Process(btcEurOrderBook);
            arbitrageCalculator.Process(jpyEurOrderBook);
            arbitrageCalculator.Process(usdJpyOrderBook);
            arbitrageCalculator.Process(btcUsdOrderBook);

            await arbitrageCalculator.Execute();

            var crossRates = arbitrageCalculator.GetCrossRates().ToList();
            var arbitrages = arbitrageCalculator.GetArbitrages().ToList();

            Assert.Equal(2, crossRates.Count);
            Assert.Equal(1, arbitrages.Count);

            var arbitrage1 = arbitrages.Single(x => x.ConversionPath == "(TEST1-EURBTC * TEST2-JPYEUR * TEST3-USDJPY) > (TEST4-BTCUSD)");
            Assert.Equal(0.00001781m, arbitrage1.PnL, 8);
            Assert.Equal(-0.0216643m, arbitrage1.Spread, 8);
            Assert.Equal(0.0000094m, arbitrage1.Volume, 8);
            Assert.NotEqual(default, arbitrage1.StartedAt);
            Assert.Equal(default, arbitrage1.EndedAt);
            Assert.NotEqual(default, arbitrage1.Lasted);
            Assert.Equal(8744.894520m, arbitrage1.Bid.Price, 8);
            Assert.Equal(0.0000094m, arbitrage1.Bid.Volume, 8);
            Assert.Equal(8743, arbitrage1.Ask.Price, 8);
            Assert.Equal(11, arbitrage1.Ask.Volume, 8);
        }



        [Fact]
        public async Task ArbitragesTest()
        {
            var baseAssets = new List<string> { "BTC" };
            const string quoteAsset = "USD";

            var settings = GetSettings(baseAssets, quoteAsset, -20);
            var arbitrageDetector = new ArbitrageDetectorService(new LogToConsole(), null, SettingsRepository(settings));

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
        public async Task ArbitrageHistoryTest()
        {
            var baseAssets = new List<string> { "BTC" };
            const string quoteAsset = "USD";

            var settings = new Settings(50, 1, baseAssets, new List<string>(), quoteAsset, -20, new List<string>(), 0, 0, new List<string>(), new Dictionary<string, string>());
            var arbitrageDetector = new ArbitrageDetectorService(new LogToConsole(), null, SettingsRepository(settings));

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

            await arbitrageDetector.Execute();
            Thread.Sleep(1000); // Wait until cross rate expire and arbitrage appears in history
            await arbitrageDetector.Execute();

            var arbitrageHistory = arbitrageDetector.GetArbitrageHistory(DateTime.MinValue, short.MaxValue);

            Assert.Equal(3, arbitrageHistory.Count());
        }


        [Fact]
        public async Task ManyCrossRatesPerformanceTest()
        {
            var baseAssets = new List<string> { "BTC" };
            const string quoteAsset = "USD";

            var settings = GetSettings(baseAssets, quoteAsset, -20);
            var arbitrageDetector = new ArbitrageDetectorService(new LogToConsole(), null, SettingsRepository(settings));

            var orderBooks = new List<OrderBook>();
            orderBooks.AddRange(Generate2OrderBooksForCrossRates(500, "GDAX", new AssetPair("BTC", "USD"), 10, 11000, 10000, 10, 11500, 11000));
            orderBooks.AddRange(Generate2OrderBooksForCrossRates(500, "Bitfinex", new AssetPair("BTC", "USD"), 10, 11000, 10200, 10, 11600, 11000));
            Assert.Equal(2000, orderBooks.Count);

            foreach (var orderBook in orderBooks)
                arbitrageDetector.Process(orderBook);

            var watch = Stopwatch.StartNew();
            await arbitrageDetector.CalculateCrossRates();
            watch.Stop();
            if (performance)
                Assert.InRange(watch.ElapsedMilliseconds, 400, 500);

            var crossRates = arbitrageDetector.GetCrossRates().ToList();
            var arbitrages = arbitrageDetector.GetArbitrages().ToList();

            Assert.InRange(crossRates.Count, 1000, 1048); // because of sqrt
            Assert.Empty(arbitrages);
        }

        //[Fact]
        public async Task ManyArbitragesPerformanceTest()
        {
            var baseAssets = new List<string> { "BTC" };
            const string quoteAsset = "USD";

            var settings = GetSettings(baseAssets, quoteAsset, -20);
            var arbitrageDetector = new ArbitrageDetectorService(new LogToConsole(), null, SettingsRepository(settings));

            var orderBooks = new List<OrderBook>();
            orderBooks.AddRange(GenerateOrderBooks(2, "TEST1", new AssetPair("BTC", "USD"), 10, 11000, 10000, 10, 11500, 11000));

            orderBooks.AddRange(GenerateOrderBooks(2, "TEST2", new AssetPair("BTC", "EUR"), 10, 8980.95m, 8825, 10, 9000, 8980.95m));
            orderBooks.AddRange(GenerateOrderBooks(1, "TEST3", new AssetPair("EUR", "USD"), 10, 1.2200m, 1.2190m, 10, 1.2205m, 1.2200m));

            orderBooks.AddRange(GenerateOrderBooks(2, "TEST4", new AssetPair("BTC", "CHF"), 10, 9197, 9196, 10, 9196, 9195));
            orderBooks.AddRange(GenerateOrderBooks(1, "TEST5", new AssetPair("CHF", "JPY"), 10, 131, 130, 10, 134, 132));
            orderBooks.AddRange(GenerateOrderBooks(1, "TEST6", new AssetPair("JPY", "USD"), 10, 0.009132m, 0.009131m, 10, 0.009135m, 0.009133m));

            Assert.Equal(9, orderBooks.Count);

            foreach (var orderBook in orderBooks)
                arbitrageDetector.Process(orderBook);

            var watch = Stopwatch.StartNew();
            var crossRates = await arbitrageDetector.CalculateCrossRates();
            if (performance)
                Assert.InRange(watch.ElapsedMilliseconds, 5, 50);
            var arbitrages = await arbitrageDetector.CalculateArbitrages();
            watch.Stop();
            if (performance)
                Assert.InRange(watch.ElapsedMilliseconds, 600, 700);

            Assert.Equal(6, crossRates.Count());
            Assert.Equal(12, arbitrages.Count());
        }

        [Fact]
        public async Task ManyArbitragesInHistoryPerformanceTest()
        {
            var baseAssets = new List<string> { "BTC" };
            const string quoteAsset = "USD";

            var settings = GetSettings(baseAssets, quoteAsset, -20);
            var arbitrageDetector = new ArbitrageDetectorService(new LogToConsole(), null, SettingsRepository(settings));

            var orderBooks = new List<OrderBook>();
            orderBooks.AddRange(GenerateOrderBooks(7, "GDAX", new AssetPair("BTC", "USD"), 10, 11000, 10000, 10, 11500, 11000));
            orderBooks.AddRange(GenerateOrderBooks(7, "Bitfinex", new AssetPair("BTC", "USD"), 10, 10900, 10200, 10, 11600, 10900));
            orderBooks.AddRange(GenerateOrderBooks(7, "Quoine", new AssetPair("BTC", "EUR"), 10, 8980.95m, 8825, 10, 9000, 8980.95m));
            orderBooks.AddRange(GenerateOrderBooks(7, "Binance", new AssetPair("EUR", "USD"), 10, 1.2200m, 1.2190m, 10, 1.2205m, 1.2200m));
            Assert.Equal(28, orderBooks.Count);
            foreach (var orderBook in orderBooks)
                arbitrageDetector.Process(orderBook);

            var watch = Stopwatch.StartNew();
            await arbitrageDetector.Execute();
            watch.Stop();
            if (performance)
                Assert.InRange(watch.ElapsedMilliseconds, 350, 450);

            var crossRates = arbitrageDetector.GetCrossRates();
            var arbitrages = arbitrageDetector.GetArbitrages();

            Assert.Equal(63, crossRates.Count());
            Assert.Equal(735, arbitrages.Count());


            orderBooks = new List<OrderBook>();
            orderBooks.AddRange(GenerateOrderBooks(7, "GDAX", new AssetPair("BTC", "USD"), 10, 11000, 10000, 10, 11500, 11000));
            orderBooks.AddRange(GenerateOrderBooks(7, "Bitfinex", new AssetPair("BTC", "USD"), 10, 10900, 10200, 10, 11600, 10900));
            orderBooks.AddRange(GenerateOrderBooks(7, "Quoine", new AssetPair("BTC", "EUR"), 10, 8980.95m, 8825, 10, 9000, 8980.95m));
            orderBooks.AddRange(GenerateOrderBooks(7, "Binance", new AssetPair("EUR", "USD"), 10, 1.2200m, 1.2190m, 10, 1.2205m, 1.2200m));
            Assert.Equal(28, orderBooks.Count);
            foreach (var orderBook in orderBooks)
                arbitrageDetector.Process(orderBook);

            watch = Stopwatch.StartNew();
            await arbitrageDetector.Execute();
            watch.Stop();
            if (performance)
                Assert.InRange(watch.ElapsedMilliseconds, 300, 400); // Second time may be faster

            crossRates = arbitrageDetector.GetCrossRates();
            arbitrages = arbitrageDetector.GetArbitrages();

            Assert.Equal(63, crossRates.Count());
            Assert.Equal(735, arbitrages.Count());
        }


        [Fact]
        public async Task MatrixTest()
        {
            // TODO: Must be implemented
        }

        [Fact]
        public async Task SettingsSetAllTest()
        {
            var startupSettings = new Settings(50, 10, new List<string> { "BTC", "ETH" }, new List<string> { "EUR", "CHF" }, "USD", -20, new List<string> { "GDAX" }, 0,
                10.00000001m, new List<string> { "ABC" }, new Dictionary<string, string> { { "Bitfinex(e)", "Bitfinex" } });

            var arbitrageCalculator = new ArbitrageDetectorService(new LogToConsole(), null, SettingsRepository(startupSettings));

            var settings = new Settings(30, 5, new List<string> { "EUR", "USD" }, new List<string> { "BTC", "ETH" }, "BTC", -20, new List<string> { "Bitfinex" }, 0,
                10.00000001m, new List<string> { "XYZ" }, new Dictionary<string, string> { { "Kucoin(e)", "Kucoin" } });

            arbitrageCalculator.SetSettings(settings);

            var newSettings = arbitrageCalculator.GetSettings();
            Assert.Equal(settings.ExpirationTimeInSeconds, newSettings.ExpirationTimeInSeconds);
            Assert.Equal(settings.BaseAssets, newSettings.BaseAssets);
            Assert.Equal(settings.IntermediateAssets, newSettings.IntermediateAssets);
            Assert.Equal(settings.QuoteAsset, newSettings.QuoteAsset);
            Assert.Equal(settings.MinSpread, newSettings.MinSpread);
            Assert.Equal(settings.Exchanges, newSettings.Exchanges);
            Assert.Equal(settings.MinimumPnL, newSettings.MinimumPnL);
            Assert.Equal(settings.MinimumVolume, newSettings.MinimumVolume);
        }

        [Fact]
        public async Task SettingsMinimumPnLTest()
        {
            var baseAssets = new List<string> { "BTC" };
            const string quoteAsset = "USD";

            
            var settings = new Settings(50, 10, baseAssets, new List<string>(), quoteAsset, -20, new List<string>(), 0, 500.00000001m, new List<string>(), new Dictionary<string, string>());
            var arbitrageDetector = new ArbitrageDetectorService(new LogToConsole(), null, SettingsRepository(settings));

            var btcUsdOrderBook1 = new OrderBook("GDAX", "BTCUSD",
                new List<VolumePrice> { new VolumePrice(11000, 10) }, // bids
                new List<VolumePrice> { new VolumePrice(11050, 10) }, // asks
                DateTime.UtcNow);

            var btcUsdOrderBook2 = new OrderBook("Bitfinex", "BTCUSD",
                new List<VolumePrice> { new VolumePrice(11100, 10) }, // bids
                new List<VolumePrice> { new VolumePrice(11300, 10) }, // asks
                DateTime.UtcNow);

            arbitrageDetector.Process(btcUsdOrderBook1);
            arbitrageDetector.Process(btcUsdOrderBook2);

            await arbitrageDetector.Execute();

            var crossRates = arbitrageDetector.GetCrossRates().ToList();
            var arbitrages = arbitrageDetector.GetArbitrages().ToList();

            Assert.Equal(2, crossRates.Count);
            Assert.Equal(0, arbitrages.Count);
        }

        [Fact]
        public async Task SettingsMinimumVolumeTest()
        {
            var baseAssets = new List<string> { "BTC" };
            const string quoteAsset = "USD";

            var settings = new Settings(50, 10, baseAssets, new List<string>(), quoteAsset, -20, new List<string>(), 0, 10.00000001m, new List<string>(), new Dictionary<string, string>());
            var arbitrageDetector = new ArbitrageDetectorService(new LogToConsole(), null, SettingsRepository(settings));

            var btcUsdOrderBook1 = new OrderBook("GDAX", "BTCUSD",
                new List<VolumePrice> { new VolumePrice(11000, 10) }, // bids
                new List<VolumePrice> { new VolumePrice(11050, 10) }, // asks
                DateTime.UtcNow);

            var btcUsdOrderBook2 = new OrderBook("Bitfinex", "BTCUSD",
                new List<VolumePrice> { new VolumePrice(11100, 10) }, // bids
                new List<VolumePrice> { new VolumePrice(11300, 10) }, // asks
                DateTime.UtcNow);

            arbitrageDetector.Process(btcUsdOrderBook1);
            arbitrageDetector.Process(btcUsdOrderBook2);

            await arbitrageDetector.Execute();

            var crossRates = arbitrageDetector.GetCrossRates().ToList();
            var arbitrages = arbitrageDetector.GetArbitrages().ToList();

            Assert.Equal(2, crossRates.Count);
            Assert.Equal(0, arbitrages.Count);
        }

        [Fact]
        public async Task SettingsExchangesTest()
        {
            var baseAssets = new List<string> { "BTC" };
            const string quoteAsset = "USD";

            var settings = new Settings(50, 10, baseAssets, new List<string>(), quoteAsset, -20, new List<string> { "GDAX" }, 0, 0, new List<string>(), new Dictionary<string, string>());
            var arbitrageDetector = new ArbitrageDetectorService(new LogToConsole(), null, SettingsRepository(settings));

            var btcUsdOrderBook1 = new OrderBook("GDAX", "BTCUSD",
                new List<VolumePrice> { new VolumePrice(11000, 10) }, // bids
                new List<VolumePrice> { new VolumePrice(11050, 10) }, // asks
                DateTime.UtcNow);

            var btcUsdOrderBook2 = new OrderBook("Bitfinex", "BTCUSD",
                new List<VolumePrice> { new VolumePrice(11100, 10) }, // bids
                new List<VolumePrice> { new VolumePrice(11300, 10) }, // asks
                DateTime.UtcNow);

            arbitrageDetector.Process(btcUsdOrderBook1);
            arbitrageDetector.Process(btcUsdOrderBook2);

            await arbitrageDetector.Execute();

            var crossRates = arbitrageDetector.GetCrossRates().ToList();
            var arbitrages = arbitrageDetector.GetArbitrages().ToList();

            Assert.Equal(1, crossRates.Count);
            Assert.Equal(0, arbitrages.Count);
        }


        private ISettingsRepository SettingsRepository(ISettings settings)
        {
            var settingsRepository = new Mock<ISettingsRepository>();
            settingsRepository.Setup(x => x.GetAsync()).ReturnsAsync(settings);

            return settingsRepository.Object;
        }

        private Settings GetSettings(IEnumerable<string> baseAssets, string quoteAsset, int minSpread)
        {
            var result = new Settings(50, 10, baseAssets, new List<string>(), quoteAsset, minSpread, new List<string>(), 0, 0, new List<string>(), new Dictionary<string, string>());

            return result;
        }


        private IEnumerable<OrderBook> GenerateOrderBooks(int count, string source, AssetPair assetPair, int bidCount, decimal maxBid, decimal minBid, int askCount, decimal maxAsk, decimal minAsk)
        {
            #region Arguments checking

            if (minAsk > maxAsk)
                throw new Exception("minAsk > maxAsk");

            if (minBid > maxBid)
                throw new Exception("minBid > maxBid");

            #endregion

            var result = new List<OrderBook>();

            for (var i = 0; i < count; i++)
            {
                var asks = GenerateVolumePrices(askCount, minAsk, maxAsk);
                var bids = GenerateVolumePrices(bidCount, minBid, maxBid);

                var orderBook = new OrderBook(source + i, assetPair.Name, bids, asks, DateTime.UtcNow);
                orderBook.SetAssetPair(assetPair.Base);

                result.Add(orderBook);
            }

            #region Asserts

            Assert.Equal(result.Count, count);
            Assert.True(result.TrueForAll(x => x.Source.Contains(source)));
            Assert.True(result.TrueForAll(x => x.AssetPair.Equals(assetPair)));

            #endregion

            return result;
        }

        private IReadOnlyCollection<VolumePrice> GenerateVolumePrices(int count, decimal min, decimal max)
        {
            var result = new List<VolumePrice>();

            var step = (max - min) / (count - 1);
            for (var i = 0; i < count - 1; i++)
            {
                var volumePrice = new VolumePrice(min + i * step, new Random().Next(1, 10));
                result.Add(volumePrice);
            }
            result.Add(new VolumePrice(max, new Random().Next(1, 10)));

            Assert.Equal(result.Count, count);
            Assert.Single(result.Where(x => min == x.Price));
            Assert.Single(result.Where(x => max == x.Price));
            Assert.True(result.TrueForAll(x => min <= x.Price));
            Assert.True(result.TrueForAll(x => x.Price <= max));

            return result;
        }

        private IEnumerable<OrderBook> Generate2OrderBooksForCrossRates(int count, string source, AssetPair assetPair, int bidCount, decimal maxBid, decimal minBid, int askCount, decimal maxAsk, decimal minAsk)
        {
            #region Arguments checking

            if (minBid > maxBid)
                throw new Exception("minBid > maxBid");

            if (minAsk > maxAsk)
                throw new Exception("minAsk > maxAsk");

            #endregion

            var result = new List<OrderBook>();

            for (var i = 0; i < count; i++)
            {
                var bids = GenerateVolumePricesForCrossRates(bidCount, minBid, maxBid);
                var asks = GenerateVolumePricesForCrossRates(askCount, minAsk, maxAsk);

                var intermediateAsset = RandomString(3);
                var orderBook1 = new OrderBook(source + i, assetPair.Base + intermediateAsset, bids, asks, DateTime.UtcNow);
                orderBook1.SetAssetPair(assetPair.Base);
                var orderBook2 = new OrderBook(source + i, intermediateAsset + assetPair.Quote, bids, asks, DateTime.UtcNow);
                orderBook2.SetAssetPair(assetPair.Quote);

                result.Add(orderBook1);
                result.Add(orderBook2);
            }

            #region Asserts

            Assert.Equal(result.Count, count * 2);
            Assert.True(result.TrueForAll(x => x.Source.Contains(source)));
            Assert.True(result.TrueForAll(x => x.AssetPair.ContainsAsset(assetPair.Base) || x.AssetPair.ContainsAsset(assetPair.Quote)));

            #endregion

            return result;
        }

        private IEnumerable<OrderBook> Generate3OrderBooksForCrossRates(int count, string source, AssetPair assetPair, int bidCount, decimal maxBid, decimal minBid, int askCount, decimal maxAsk, decimal minAsk)
        {
            #region Arguments checking

            if (minBid > maxBid)
                throw new Exception("minBid > maxBid");

            if (minAsk > maxAsk)
                throw new Exception("minAsk > maxAsk");

            #endregion

            var result = new List<OrderBook>();

            for (var i = 0; i < count; i++)
            {
                var bids = GenerateVolumePricesForCrossRates(bidCount, minBid, maxBid);
                var asks = GenerateVolumePricesForCrossRates(askCount, minAsk, maxAsk);

                var intermediate1Asset = RandomString(3);
                var orderBook1 = new OrderBook(source + i, assetPair.Base + intermediate1Asset, bids, asks, DateTime.UtcNow);
                orderBook1.SetAssetPair(assetPair.Base);

                var intermediate2Asset = RandomString(3);
                var orderBook2 = new OrderBook(source + i, intermediate1Asset + intermediate2Asset, bids, asks, DateTime.UtcNow);
                orderBook2.SetAssetPair(assetPair.Quote);

                var orderBook3 = new OrderBook(source + i, intermediate2Asset + assetPair.Quote, bids, asks, DateTime.UtcNow);
                orderBook3.SetAssetPair(assetPair.Quote);

                result.Add(orderBook1);
                result.Add(orderBook2);
            }

            #region Asserts

            Assert.Equal(result.Count, count * 2);
            Assert.True(result.TrueForAll(x => x.Source.Contains(source)));
            Assert.True(result.TrueForAll(x => x.AssetPair.ContainsAsset(assetPair.Base) || x.AssetPair.ContainsAsset(assetPair.Quote)));

            #endregion

            return result;
        }

        private IReadOnlyCollection<VolumePrice> GenerateVolumePricesForCrossRates(int count, decimal min, decimal max)
        {
            var result = new List<VolumePrice>();

            var step = (max - min) / (count - 1);
            for (var i = 0; i < count - 1; i++)
            {
                var volumePrice = new VolumePrice(Sqrt(min + i * step), new Random().Next(1, 10));
                result.Add(volumePrice);
            }
            result.Add(new VolumePrice(Sqrt(max), new Random().Next(1, 10)));

            Assert.Equal(result.Count, count);
            Assert.True(result.TrueForAll(x => Sqrt(min) <= x.Price));
            Assert.True(result.TrueForAll(x => x.Price <= Sqrt(max)));
            Assert.Single(result.Where(x => Sqrt(min) == x.Price));
            Assert.Single(result.Where(x => Sqrt(max) == x.Price));

            return result;
        }

        private string RandomString(int length)
        {
            var random = new Random();
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            return new string(Enumerable.Repeat(chars, length)
                .Select(s => s[random.Next(s.Length)]).ToArray());
        }

        private decimal Sqrt(decimal value)
        {
            return (decimal)Math.Sqrt((double)value);
        }
    }
}
