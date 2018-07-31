using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Lykke.Service.ArbitrageDetector.Client.Models;
using Xunit;

namespace Lykke.Service.ArbitrageDetector.Client.Tests
{
    [Collection("Client collection")]
    public class ClientTest : ClientFixture
    {
        [Fact]
        public async Task OrderBooksTest()
        {
            var orderBooks = (await Client.OrderBooksAsync(string.Empty, string.Empty)).ToList();
            Assert.NotNull(orderBooks);
            Assert.NotEmpty(orderBooks);

            var orderBook = orderBooks.First();
            AssertOrderBookRow(orderBook);
        }

        [Fact]
        public async Task OrderBooksFilterExchangeTest()
        {
            var orderBooks = (await Client.OrderBooksAsync("lykke", string.Empty)).ToList();
            Assert.NotNull(orderBooks);
            Assert.NotEmpty(orderBooks);

            var orderBook = orderBooks.First();
            Assert.Equal("lykke", orderBook.Source);
            AssertOrderBookRow(orderBook);
        }

        [Fact]
        public async Task OrderBooksFilterAssetPairTest()
        {
            var orderBooks = (await Client.OrderBooksAsync(string.Empty, "USD")).ToList();
            Assert.NotNull(orderBooks);
            Assert.NotEmpty(orderBooks);

            var orderBook = orderBooks.First();
            Assert.True(orderBook.AssetPair.Base == "USD" || orderBook.AssetPair.Quote == "USD");
            AssertOrderBookRow(orderBook);
        }

        [Fact]
        public async Task SynthOrderBooksTest()
        {
            var synthOrderBooks = (await Client.SynthOrderBooksAsync()).ToList();
            Assert.NotNull(synthOrderBooks);
            Assert.NotEmpty(synthOrderBooks);

            var synthOrderBook = synthOrderBooks.First();
            AssertSynthOrderBookRow(synthOrderBook);
        }

        [Fact]
        public async Task ArbitragesTest()
        {
            var arbitrages = (await Client.ArbitragesAsync()).ToList();
            Assert.NotEmpty(arbitrages);

            var arbitrage = arbitrages.First();
            AssertArbitrageRow(arbitrage, false);
        }

        [Fact]
        public async Task ArbitrageFromHistoryTest()
        {
            var arbitrages = (await Client.ArbitrageHistoryAsync(DateTime.MinValue, short.MaxValue)).ToList();
            Assert.NotEmpty(arbitrages);

            var conversionPath = arbitrages.First().ConversionPath;
            var arbitrage = await Client.ArbitrageFromHistoryAsync(conversionPath);
            Assert.NotNull(arbitrage);
            Assert.Equal(conversionPath, arbitrage.ConversionPath);
            
            AssertArbitrage(arbitrage, false);
        }

        [Fact]
        public async Task ArbitrageFromActiveOrHistoryTest()
        {
            // From active

            var arbitrages = (await Client.ArbitragesAsync()).ToList();
            Assert.NotEmpty(arbitrages);

            var conversionPath = arbitrages.First().ConversionPath;
            var arbitrage = await Client.ArbitrageFromActiveOrHistoryAsync(conversionPath);
            Assert.NotNull(arbitrage);
            Assert.Equal(conversionPath, arbitrage.ConversionPath);

            AssertArbitrage(arbitrage, true);

            // From history

            arbitrages = (await Client.ArbitrageHistoryAsync(DateTime.MinValue, short.MaxValue)).ToList();
            Assert.NotEmpty(arbitrages);

            conversionPath = arbitrages.First().ConversionPath;
            arbitrage = await Client.ArbitrageFromActiveOrHistoryAsync(conversionPath);
            Assert.NotNull(arbitrage);
            Assert.Equal(conversionPath, arbitrage.ConversionPath);

            AssertArbitrage(arbitrage, false);
        }

        [Fact]
        public async Task ArbitrageHistoryTest()
        {
            var arbitrages = (await Client.ArbitrageHistoryAsync(DateTime.MinValue, short.MaxValue)).ToList();
            Assert.NotEmpty(arbitrages);

            var arbitrage = arbitrages.First();
            AssertArbitrageRow(arbitrage, true);
        }

        [Fact]
        public async Task MatrixTest()
        {
            var matrix = await Client.MatrixAsync("BTCUSD");
            Assert.NotNull(matrix);

            Assert.NotEmpty(matrix.AssetPair);
            Assert.NotEmpty(matrix.Bids);
            Assert.NotEmpty(matrix.Asks);
            Assert.NotEmpty(matrix.Exchanges);
            Assert.NotEmpty(matrix.Cells);

            Assert.Equal(matrix.Bids.Count, matrix.Asks.Count);
            Assert.Equal(matrix.Exchanges.Count, matrix.Asks.Count);
            Assert.Equal(matrix.Cells.Count, matrix.Asks.Count);
            Assert.Equal(matrix.Cells[0].Count, matrix.Asks.Count);
        }

        [Fact]
        public async Task PublicMatrixTest()
        {
            var matrix = await Client.PublicMatrixAsync("BTCUSD");
            Assert.NotNull(matrix);

            Assert.NotEmpty(matrix.AssetPair);
            Assert.NotEmpty(matrix.Bids);
            Assert.NotEmpty(matrix.Asks);
            Assert.NotEmpty(matrix.Exchanges);
            Assert.NotEmpty(matrix.Cells);

            Assert.Equal(matrix.Bids.Count, matrix.Asks.Count);
            Assert.Equal(matrix.Exchanges.Count, matrix.Asks.Count);
            Assert.Equal(matrix.Cells.Count, matrix.Asks.Count);
            Assert.Equal(matrix.Cells[0].Count, matrix.Asks.Count);
        }

        [Fact]
        public async Task PublicMatrixAssetPairsTest()
        {
            var assetPairs = await Client.PublicMatrixAssetPairsAsync();
            Assert.NotNull(assetPairs);
        }

        [Fact]
        public async Task GetSettignsTest()
        {
            var settings = await Client.GetSettingsAsync();
            Assert.NotEqual(default, settings.ExpirationTimeInSeconds);
            Assert.NotEmpty(settings.QuoteAsset);
            Assert.NotEmpty(settings.BaseAssets);
            Assert.False(settings.BaseAssets.Where(string.IsNullOrWhiteSpace).Any());
        }

        [Fact]
        public async Task SetSettingsAllTest()
        {
            var oldSettings = await Client.GetSettingsAsync();

            var settings = new Settings
            {
                HistoryMaxSize = 33,
                ExpirationTimeInSeconds = 3,
                BaseAssets = new List<string> { "ETH" },
                IntermediateAssets = new List<string> { "ETH" },
                QuoteAsset = "ETH",
                MinSpread = -33,
                Exchanges = new List<string> { "Qoinex" },
                MinimumPnL = 33,
                MinimumVolume = 33,
                PublicMatrixAssetPairs = new List<string> { "ETHUSD" },
                PublicMatrixExchanges = new Dictionary<string, string> { { "Qoinex(e)", "Qoinex" } },
                MatrixAssetPairs = new List<string> { "ETHUSD" },
                MatrixHistoryInterval = new TimeSpan(0, 0, 3, 0),
                MatrixHistoryAssetPairs = new List<string> { "ETHUSD" },
                MatrixSignificantSpread = -33,
                MatrixHistoryLykkeName = "lykke33",
                ExchangesFees = new List<ExchangeFees> { new ExchangeFees { ExchangeName = "Qoinex", DepositFee = 0.33m, TradingFee = 0.33m } }
            };

            await Client.SetSettingsAsync(settings);

            var newSettings = await Client.GetSettingsAsync();
            AssertSettings(settings, newSettings);

            await Client.SetSettingsAsync(oldSettings);

            newSettings = await Client.GetSettingsAsync();
            AssertSettings(oldSettings, newSettings);
        }

        [Fact]
        public async Task SetSettingsExpirationTimeTest()
        {
            var oldSettings = await Client.GetSettingsAsync();

            var settings = new Settings { ExpirationTimeInSeconds = 97 };

            await Client.SetSettingsAsync(settings);

            var newSettings = await Client.GetSettingsAsync();
            AssertSettings(settings, newSettings);

            await Client.SetSettingsAsync(oldSettings);

            newSettings = await Client.GetSettingsAsync();
            AssertSettings(oldSettings, newSettings);
        }

        [Fact]
        public async Task SetSettingsBaseAssetsTest()
        {
            var oldSettings = await Client.GetSettingsAsync();

            var settings = new Settings { BaseAssets = new List<string> { "REP", "DAT" } };

            await Client.SetSettingsAsync(settings);

            var newSettings = await Client.GetSettingsAsync();
            AssertSettings(settings, newSettings);

            await Client.SetSettingsAsync(oldSettings);

            newSettings = await Client.GetSettingsAsync();
            AssertSettings(oldSettings, newSettings);
        }

        [Fact]
        public async Task SetSettingsQuoteAssetTest()
        {
            var oldSettings = await Client.GetSettingsAsync();

            var settings = new Settings { QuoteAsset = "AID" };

            await Client.SetSettingsAsync(settings);

            var newSettings = await Client.GetSettingsAsync();
            AssertSettings(settings, newSettings);

            await Client.SetSettingsAsync(oldSettings);

            newSettings = await Client.GetSettingsAsync();
            AssertSettings(oldSettings, newSettings);
        }

        [Fact]
        public async Task SetSettingsIntermediateAssetsTest()
        {
            var oldSettings = await Client.GetSettingsAsync();

            var settings = new Settings { IntermediateAssets = new List<string> { "LKKY1", "LKKY2" } };

            await Client.SetSettingsAsync(settings);

            var newSettings = await Client.GetSettingsAsync();
            AssertSettings(settings, newSettings);

            await Client.SetSettingsAsync(oldSettings);

            newSettings = await Client.GetSettingsAsync();
            AssertSettings(oldSettings, newSettings);
        }

        [Fact]
        public async Task SetSettingsMinSpreadTest()
        {
            var oldSettings = await Client.GetSettingsAsync();

            var settings = new Settings { MinSpread = -97 };

            await Client.SetSettingsAsync(settings);

            var newSettings = await Client.GetSettingsAsync();
            AssertSettings(settings, newSettings);

            await Client.SetSettingsAsync(oldSettings);

            newSettings = await Client.GetSettingsAsync();
            AssertSettings(oldSettings, newSettings);
        }

        [Fact]
        public async Task SetSettingsExchangesTest()
        {
            var oldSettings = await Client.GetSettingsAsync();

            var settings = new Settings { Exchanges = new List<string> { "GDAX" } };

            await Client.SetSettingsAsync(settings);

            var newSettings = await Client.GetSettingsAsync();
            AssertSettings(settings, newSettings);

            await Client.SetSettingsAsync(oldSettings);

            newSettings = await Client.GetSettingsAsync();
            AssertSettings(oldSettings, newSettings);
        }

        [Fact]
        public async Task SetSettingsMinimumPnLTest()
        {
            var oldSettings = await Client.GetSettingsAsync();

            var settings = new Settings { MinimumPnL = 13 };

            await Client.SetSettingsAsync(settings);

            var newSettings = await Client.GetSettingsAsync();
            AssertSettings(settings, newSettings);

            await Client.SetSettingsAsync(oldSettings);

            newSettings = await Client.GetSettingsAsync();
            AssertSettings(oldSettings, newSettings);
        }

        [Fact]
        public async Task SetSettingsMinimumVolumeTest()
        {
            var oldSettings = await Client.GetSettingsAsync();

            var settings = new Settings { MinimumVolume = 17 };

            await Client.SetSettingsAsync(settings);

            var newSettings = await Client.GetSettingsAsync();
            AssertSettings(settings, newSettings);

            await Client.SetSettingsAsync(oldSettings);

            newSettings = await Client.GetSettingsAsync();
            AssertSettings(oldSettings, newSettings);
        }

        [Fact]
        public async Task SetSettingsPublicMatrixAssetPairsTest()
        {
            var oldSettings = await Client.GetSettingsAsync();

            var settings = new Settings { PublicMatrixAssetPairs = new List<string> { "ABCUSD" } };

            await Client.SetSettingsAsync(settings);

            var newSettings = await Client.GetSettingsAsync();
            AssertSettings(settings, newSettings);

            await Client.SetSettingsAsync(oldSettings);

            newSettings = await Client.GetSettingsAsync();
            AssertSettings(oldSettings, newSettings);
        }

        [Fact]
        public async Task SetSettingsPublicMatrixExchangesTest()
        {
            var oldSettings = await Client.GetSettingsAsync();
            
            var settings = new Settings { PublicMatrixExchanges = new Dictionary<string, string> { { "Bitfinex(e)", "Bitfinex" } } };

            await Client.SetSettingsAsync(settings);

            var newSettings = await Client.GetSettingsAsync();
            AssertSettings(settings, newSettings);

            await Client.SetSettingsAsync(oldSettings);

            newSettings = await Client.GetSettingsAsync();
            AssertSettings(oldSettings, newSettings);
        }

        [Fact]
        public async Task SetSettingsMatrixAssetPairsTest()
        {
            var oldSettings = await Client.GetSettingsAsync();

            var settings = new Settings { MatrixAssetPairs = new List<string> { "ABCUSD" } };

            await Client.SetSettingsAsync(settings);

            var newSettings = await Client.GetSettingsAsync();
            AssertSettings(settings, newSettings);

            await Client.SetSettingsAsync(oldSettings);

            newSettings = await Client.GetSettingsAsync();
            AssertSettings(oldSettings, newSettings);
        }


        private void AssertOrderBook(OrderBook orderBook)
        {
            Assert.NotEmpty(orderBook.Source);
            Assert.False(orderBook.AssetPair.IsEmpty());
            Assert.NotEmpty(orderBook.Bids);
            Assert.NotEmpty(orderBook.Asks);
            Assert.NotEqual(default, orderBook.Bids.First().Price);
            Assert.NotEqual(default, orderBook.Bids.First().Volume);
            Assert.NotEqual(default, orderBook.Asks.First().Price);
            Assert.NotEqual(default, orderBook.Asks.First().Volume);
            Assert.NotEqual(default, orderBook.Timestamp);
        }

        private void AssertOrderBookRow(OrderBookRow orderBookRow)
        {
            Assert.NotEmpty(orderBookRow.Source);
            Assert.False(orderBookRow.AssetPair.IsEmpty());
            Assert.True(orderBookRow.BestBid.HasValue || orderBookRow.BestAsk.HasValue);
            Assert.True(orderBookRow.BidsVolume != 0 || orderBookRow.AsksVolume != 0);
            Assert.NotEqual(default, orderBookRow.Timestamp);
        }

        private void AssertSynthOrderBook(SynthOrderBook synthOrderBook)
        {
            AssertOrderBook(synthOrderBook);
            Assert.NotEmpty(synthOrderBook.ConversionPath);
            Assert.NotEmpty(synthOrderBook.OriginalOrderBooks);
            foreach (var orderBook in synthOrderBook.OriginalOrderBooks)
                AssertOrderBook(orderBook);
        }

        private void AssertSynthOrderBookRow(SynthOrderBookRow synthOrderBook)
        {
            Assert.NotEmpty(synthOrderBook.Source);
            Assert.False(synthOrderBook.AssetPair.IsEmpty());
            if (synthOrderBook.BestBid.HasValue)
            {
                Assert.NotEqual(default, synthOrderBook.BestBid.Value.Price);
                Assert.NotEqual(default, synthOrderBook.BestBid.Value.Volume);
            }

            if (synthOrderBook.BestAsk.HasValue)
            {
                Assert.NotEqual(default, synthOrderBook.BestAsk.Value.Price);
                Assert.NotEqual(default, synthOrderBook.BestAsk.Value.Volume);
            }

            Assert.NotEmpty(synthOrderBook.ConversionPath);
            Assert.NotEqual(default, synthOrderBook.Timestamp);
        }

        private void AssertArbitrage(Arbitrage arbitrage, bool isActive)
        {
            Assert.False(arbitrage.AssetPair.IsEmpty());
            AssertSynthOrderBook(arbitrage.AskSynth);
            AssertSynthOrderBook(arbitrage.BidSynth);
            Assert.NotEqual(default, arbitrage.Bid.Price);
            Assert.NotEqual(default, arbitrage.Bid.Volume);
            Assert.NotEqual(default, arbitrage.Ask.Price);
            Assert.NotEqual(default, arbitrage.Ask.Volume);
            Assert.NotEqual(default, arbitrage.Spread);
            Assert.NotEqual(default, arbitrage.Volume);
            Assert.NotEqual(default, arbitrage.PnL);
            Assert.NotEqual(default, arbitrage.StartedAt);

            if (isActive)
                Assert.Equal(default, arbitrage.EndedAt);
            else
                Assert.NotEqual(default, arbitrage.EndedAt);
        }

        private void AssertArbitrageRow(ArbitrageRow arbitrage, bool ended)
        {
            Assert.False(arbitrage.AssetPair.IsEmpty());
            Assert.NotEmpty(arbitrage.AskSource);
            Assert.NotEmpty(arbitrage.BidSource);
            Assert.NotEmpty(arbitrage.AskConversionPath);
            Assert.NotEmpty(arbitrage.BidConversionPath);
            Assert.NotEqual(default, arbitrage.Bid.Price);
            Assert.NotEqual(default, arbitrage.Bid.Volume);
            Assert.NotEqual(default, arbitrage.Ask.Price);
            Assert.NotEqual(default, arbitrage.Ask.Volume);
            Assert.NotEqual(default, arbitrage.Spread);
            Assert.NotEqual(default, arbitrage.Volume);
            Assert.NotEqual(default, arbitrage.PnL);
            Assert.NotEqual(default, arbitrage.StartedAt);
            Assert.Equal(ended, arbitrage.EndedAt != default);
        }

        private void AssertSettings(Settings one, Settings another)
        {
            Assert.Equal(one.ExpirationTimeInSeconds, another.ExpirationTimeInSeconds);

            Assert.Equal(one.MinSpread, another.MinSpread);

            if (one.BaseAssets != null && another.BaseAssets != null)
                Assert.Equal(one.BaseAssets, another.BaseAssets);

            if (one.IntermediateAssets != null && another.IntermediateAssets != null)
                Assert.Equal(one.IntermediateAssets, another.IntermediateAssets);

            if (one.QuoteAsset != null && another.QuoteAsset != null)
                Assert.Equal(one.QuoteAsset, another.QuoteAsset);

            if (one.Exchanges != null && another.Exchanges != null)
                Assert.Equal(one.Exchanges, another.Exchanges);

            Assert.Equal(one.MinimumPnL, another.MinimumPnL);

            Assert.Equal(one.MinimumVolume, another.MinimumVolume);
        }
    }
}
