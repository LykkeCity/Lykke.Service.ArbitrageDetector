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
            var orderBooks = await Client.OrderBooksAsync(string.Empty, string.Empty);

            Assert.NotNull(orderBooks);
            Assert.NotEmpty(orderBooks);
            Assert.NotEmpty(orderBooks.First().Source);
            Assert.True(!orderBooks.First().AssetPair.IsEmpty());
            Assert.NotEqual(default(DateTime), orderBooks.First().Timestamp);
            Assert.NotEmpty(orderBooks.First().Asks);
            Assert.NotEmpty(orderBooks.First().Bids);
            Assert.NotEqual(default(decimal), orderBooks.First().Asks.First().Price);
            Assert.NotEqual(default(decimal), orderBooks.First().Asks.First().Volume);
        }

        [Fact]
        public async Task OrderBooksFilterExchangeTest()
        {
            var orderBooks = await Client.OrderBooksAsync("lykke", string.Empty);

            Assert.NotNull(orderBooks);
            Assert.NotEmpty(orderBooks);
            Assert.NotEmpty(orderBooks.First().Source);
            Assert.True(!orderBooks.First().AssetPair.IsEmpty());
            Assert.NotEqual(default(DateTime), orderBooks.First().Timestamp);
            Assert.NotEmpty(orderBooks.First().Asks);
            Assert.NotEmpty(orderBooks.First().Bids);
            Assert.NotEqual(default(decimal), orderBooks.First().Asks.First().Price);
            Assert.NotEqual(default(decimal), orderBooks.First().Asks.First().Volume);
        }

        [Fact]
        public async Task OrderBooksFilterInstrumentTest()
        {
            var orderBooks = await Client.OrderBooksAsync(string.Empty, "USD");

            Assert.NotNull(orderBooks);
            Assert.NotEmpty(orderBooks);
            Assert.NotEmpty(orderBooks.First().Source);
            Assert.True(!orderBooks.First().AssetPair.IsEmpty());
            Assert.NotEqual(default(DateTime), orderBooks.First().Timestamp);
            Assert.NotEmpty(orderBooks.First().Asks);
            Assert.NotEmpty(orderBooks.First().Bids);
            Assert.NotEqual(default(decimal), orderBooks.First().Asks.First().Price);
            Assert.NotEqual(default(decimal), orderBooks.First().Asks.First().Volume);
        }

        [Fact]
        public async Task CrossRatesTest()
        {
            var crossRates = await Client.CrossRatesAsync();

            Assert.NotNull(crossRates);
            Assert.NotEmpty(crossRates);
            Assert.NotEmpty(crossRates.First().Source);
            Assert.True(!crossRates.First().AssetPair.IsEmpty());
            Assert.NotEqual(default(DateTime), crossRates.First().Timestamp);
            Assert.NotEqual(default(decimal), crossRates.First().BestAsk.Price);
            Assert.NotEqual(default(decimal), crossRates.First().BestAsk.Volume);
            Assert.NotEqual(default(decimal), crossRates.First().BestBid.Price);
            Assert.NotEqual(default(decimal), crossRates.First().BestBid.Volume);
        }

        [Fact]
        public async Task ArbitragesTest()
        {
            var arbitrages = await Client.ArbitragesAsync();
            Assert.NotNull(arbitrages);
        }

        [Fact]
        public async Task ArbitrageHistoryTest()
        {
            var arbitrageHistory = await Client.ArbitrageHistoryAsync(DateTime.UtcNow, short.MaxValue);
            Assert.NotNull(arbitrageHistory);
        }

        [Fact]
        public async Task GetSettignsTest()
        {
            var settings = await Client.GetSettingsAsync();
            Assert.NotEmpty(settings.QuoteAsset);
            Assert.NotEmpty(settings.BaseAssets);
        }

        [Fact]
        public async Task SetSettingsTest()
        {
            var oldSettings = await Client.GetSettingsAsync();

            var settings = new Settings { BaseAssets = new List<string> { "AUD", "CHF" }, QuoteAsset = "BTC" };
            await Client.SetSettingsAsync(settings);

            var newSettings = await Client.GetSettingsAsync();
            Assert.Equal(settings.BaseAssets, newSettings.BaseAssets);
            Assert.Equal(settings.QuoteAsset, newSettings.QuoteAsset);

            newSettings = await Client.GetSettingsAsync();
            Assert.Equal(oldSettings.BaseAssets, newSettings.BaseAssets);
            Assert.Equal(oldSettings.QuoteAsset, newSettings.QuoteAsset);
        }
    }
}
