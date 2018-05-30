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
            AssertOrderBook(orderBook);
        }

        [Fact]
        public async Task OrderBooksFilterExchangeTest()
        {
            var orderBooks = (await Client.OrderBooksAsync("lykke", string.Empty)).ToList();
            Assert.NotNull(orderBooks);
            Assert.NotEmpty(orderBooks);

            var orderBook = orderBooks.First();
            Assert.Equal("lykke", orderBook.Source);
            AssertOrderBook(orderBook);
        }

        [Fact]
        public async Task OrderBooksFilterAssetPairTest()
        {
            var orderBooks = (await Client.OrderBooksAsync(string.Empty, "USD")).ToList();
            Assert.NotNull(orderBooks);
            Assert.NotEmpty(orderBooks);

            var orderBook = orderBooks.First();
            Assert.True(orderBook.AssetPair.Base == "USD" || orderBook.AssetPair.Quote == "USD");
            AssertOrderBook(orderBook);
        }

        [Fact]
        public async Task CrossRatesTest()
        {
            var crossRates = (await Client.CrossRatesAsync()).ToList();
            Assert.NotNull(crossRates);
            Assert.NotEmpty(crossRates);

            var crossRate = crossRates.First();
            AssertCrossRateRow(crossRate);
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

            var settings = new Settings(0, new List<string> { "AUD", "CHF" }, new List<string> { "EUR" }, "BTC", -97, new List<string> { "GDAX" }, 13, 17, new List<string> {"BTCUSD"}, new Dictionary<string, string>{ {"", ""} });

            await Client.SetSettingsAsync(settings);

            var newSettings = await Client.GetSettingsAsync();
            AssertSettigns(settings, newSettings);

            await Client.SetSettingsAsync(oldSettings);

            newSettings = await Client.GetSettingsAsync();
            AssertSettigns(oldSettings, newSettings);
        }

        [Fact]
        public async Task SetSettingsExpirationTimeTest()
        {
            var oldSettings = await Client.GetSettingsAsync();

            var settings = new Settings { ExpirationTimeInSeconds = 97 };

            await Client.SetSettingsAsync(settings);

            var newSettings = await Client.GetSettingsAsync();
            AssertSettigns(settings, newSettings);

            await Client.SetSettingsAsync(oldSettings);

            newSettings = await Client.GetSettingsAsync();
            AssertSettigns(oldSettings, newSettings);
        }

        [Fact]
        public async Task SetSettingsBaseAssetsTest()
        {
            var oldSettings = await Client.GetSettingsAsync();

            var settings = new Settings { BaseAssets = new List<string> { "REP", "DAT" } };

            await Client.SetSettingsAsync(settings);

            var newSettings = await Client.GetSettingsAsync();
            AssertSettigns(settings, newSettings);

            await Client.SetSettingsAsync(oldSettings);

            newSettings = await Client.GetSettingsAsync();
            AssertSettigns(oldSettings, newSettings);
        }

        [Fact]
        public async Task SetSettingsQuoteAssetTest()
        {
            var oldSettings = await Client.GetSettingsAsync();

            var settings = new Settings { QuoteAsset = "AID" };

            await Client.SetSettingsAsync(settings);

            var newSettings = await Client.GetSettingsAsync();
            AssertSettigns(settings, newSettings);

            await Client.SetSettingsAsync(oldSettings);

            newSettings = await Client.GetSettingsAsync();
            AssertSettigns(oldSettings, newSettings);
        }

        [Fact]
        public async Task SetSettingsIntermediateAssetsTest()
        {
            var oldSettings = await Client.GetSettingsAsync();

            var settings = new Settings { IntermediateAssets = new List<string> { "LKKY1", "LKKY2" } };

            await Client.SetSettingsAsync(settings);

            var newSettings = await Client.GetSettingsAsync();
            AssertSettigns(settings, newSettings);

            await Client.SetSettingsAsync(oldSettings);

            newSettings = await Client.GetSettingsAsync();
            AssertSettigns(oldSettings, newSettings);
        }

        [Fact]
        public async Task SetSettingsMinSpreadTest()
        {
            var oldSettings = await Client.GetSettingsAsync();

            var settings = new Settings { MinSpread = -97 };

            await Client.SetSettingsAsync(settings);

            var newSettings = await Client.GetSettingsAsync();
            AssertSettigns(settings, newSettings);

            await Client.SetSettingsAsync(oldSettings);

            newSettings = await Client.GetSettingsAsync();
            AssertSettigns(oldSettings, newSettings);
        }

        [Fact]
        public async Task SetSettingsExchangesTest()
        {
            var oldSettings = await Client.GetSettingsAsync();

            var settings = new Settings { Exchanges = new List<string> { "GDAX" } };

            await Client.SetSettingsAsync(settings);

            var newSettings = await Client.GetSettingsAsync();
            AssertSettigns(settings, newSettings);

            await Client.SetSettingsAsync(oldSettings);

            newSettings = await Client.GetSettingsAsync();
            AssertSettigns(oldSettings, newSettings);
        }

        [Fact]
        public async Task SetSettingsMinimumPnLTest()
        {
            var oldSettings = await Client.GetSettingsAsync();

            var settings = new Settings { MinimumPnL = 13 };

            await Client.SetSettingsAsync(settings);

            var newSettings = await Client.GetSettingsAsync();
            AssertSettigns(settings, newSettings);

            await Client.SetSettingsAsync(oldSettings);

            newSettings = await Client.GetSettingsAsync();
            AssertSettigns(oldSettings, newSettings);
        }

        [Fact]
        public async Task SetSettingsMinimumVolumeTest()
        {
            var oldSettings = await Client.GetSettingsAsync();

            var settings = new Settings { MinimumVolume = 17 };

            await Client.SetSettingsAsync(settings);

            var newSettings = await Client.GetSettingsAsync();
            AssertSettigns(settings, newSettings);

            await Client.SetSettingsAsync(oldSettings);

            newSettings = await Client.GetSettingsAsync();
            AssertSettigns(oldSettings, newSettings);
        }

        [Fact]
        public async Task SetSettingsPublicMatrixAssetPairsTest()
        {
            var oldSettings = await Client.GetSettingsAsync();

            var settings = new Settings { PublicMatrixAssetPairs = new List<string> { "ABCUSD" } };

            await Client.SetSettingsAsync(settings);

            var newSettings = await Client.GetSettingsAsync();
            AssertSettigns(settings, newSettings);

            await Client.SetSettingsAsync(oldSettings);

            newSettings = await Client.GetSettingsAsync();
            AssertSettigns(oldSettings, newSettings);
        }

        [Fact]
        public async Task SetSettingsPublicMatrixExchangesTest()
        {
            var oldSettings = await Client.GetSettingsAsync();
            
            var settings = new Settings { PublicMatrixExchanges = new Dictionary<string, string> { { "Bitfinex(e)", "Bitfinex" } } };

            await Client.SetSettingsAsync(settings);

            var newSettings = await Client.GetSettingsAsync();
            AssertSettigns(settings, newSettings);

            await Client.SetSettingsAsync(oldSettings);

            newSettings = await Client.GetSettingsAsync();
            AssertSettigns(oldSettings, newSettings);
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

        private void AssertCrossRate(CrossRate crossRate)
        {
            AssertOrderBook(crossRate);
            Assert.NotEmpty(crossRate.ConversionPath);
            Assert.NotEmpty(crossRate.OriginalOrderBooks);
            foreach (var orderBook in crossRate.OriginalOrderBooks)
                AssertOrderBook(orderBook);
        }

        private void AssertCrossRateRow(CrossRateRow crossRate)
        {
            Assert.NotEmpty(crossRate.Source);
            Assert.False(crossRate.AssetPair.IsEmpty());
            Assert.NotEqual(default, crossRate.BestBid.Value.Price);
            Assert.NotEqual(default, crossRate.BestBid.Value.Volume);
            Assert.NotEqual(default, crossRate.BestAsk.Value.Price);
            Assert.NotEqual(default, crossRate.BestAsk.Value.Volume);
            Assert.NotEmpty(crossRate.ConversionPath);
            Assert.NotEqual(default, crossRate.Timestamp);
        }

        private void AssertArbitrage(Arbitrage arbitrage, bool isActive)
        {
            Assert.False(arbitrage.AssetPair.IsEmpty());
            AssertCrossRate(arbitrage.AskCrossRate);
            AssertCrossRate(arbitrage.BidCrossRate);
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

        private void AssertSettigns(Settings one, Settings another)
        {
            if (one.ExpirationTimeInSeconds != null && another.ExpirationTimeInSeconds != null)
                Assert.Equal(one.ExpirationTimeInSeconds, another.ExpirationTimeInSeconds);

            if (one.MinSpread != null && another.MinSpread != null)
                Assert.Equal(one.MinSpread, another.MinSpread);

            if (one.BaseAssets != null && another.BaseAssets != null)
                Assert.Equal(one.BaseAssets, another.BaseAssets);

            if (one.IntermediateAssets != null && another.IntermediateAssets != null)
                Assert.Equal(one.IntermediateAssets, another.IntermediateAssets);

            if (one.QuoteAsset != null && another.QuoteAsset != null)
                Assert.Equal(one.QuoteAsset, another.QuoteAsset);

            if (one.Exchanges != null && another.Exchanges != null)
                Assert.Equal(one.Exchanges, another.Exchanges);

            if (one.MinimumPnL != null && another.MinimumPnL != null)
                Assert.Equal(one.MinimumPnL, another.MinimumPnL);

            if (one.MinimumVolume != null && another.MinimumVolume != null)
                Assert.Equal(one.MinimumVolume, another.MinimumVolume);
        }
    }
}
