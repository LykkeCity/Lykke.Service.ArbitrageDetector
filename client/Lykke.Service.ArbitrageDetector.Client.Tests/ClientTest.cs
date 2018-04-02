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
        public async Task ArbitrageTest()
        {
            var arbitrages = (await Client.ArbitrageHistoryAsync(DateTime.MinValue, short.MaxValue)).ToList();
            Assert.NotEmpty(arbitrages);

            var conversionPath = arbitrages.First().ConversionPath;
            var arbitrage = await Client.ArbitrageAsync(conversionPath);
            Assert.NotNull(arbitrage);
            Assert.Equal(conversionPath, arbitrage.ConversionPath);
            
            AssertArbitrage(arbitrage);
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
        public async Task GetSettignsTest()
        {
            var settings = await Client.GetSettingsAsync();
            Assert.NotEqual(default, settings.ExpirationTimeInSeconds);
            Assert.NotEmpty(settings.QuoteAsset);
            Assert.NotEmpty(settings.BaseAssets);
            Assert.False(settings.BaseAssets.Where(string.IsNullOrWhiteSpace).Any());
        }

        [Fact]
        public async Task SetSettingsTest()
        {
            var oldSettings = await Client.GetSettingsAsync();

            var settings = new Settings { BaseAssets = new List<string> { "AUD", "CHF" }, IntermediateAssets = new List<string> { "EUR" }, QuoteAsset = "BTC", MinSpread = -97 };

            await Client.SetSettingsAsync(settings);

            var newSettings = await Client.GetSettingsAsync();
            Assert.Equal(settings.BaseAssets, newSettings.BaseAssets);
            Assert.Equal(settings.IntermediateAssets, newSettings.IntermediateAssets);
            Assert.Equal(settings.QuoteAsset, newSettings.QuoteAsset);
            Assert.Equal(settings.MinSpread, newSettings.MinSpread);

            await Client.SetSettingsAsync(oldSettings);

            newSettings = await Client.GetSettingsAsync();
            Assert.Equal(oldSettings.BaseAssets, newSettings.BaseAssets);
            Assert.Equal(oldSettings.IntermediateAssets, newSettings.IntermediateAssets);
            Assert.Equal(oldSettings.QuoteAsset, newSettings.QuoteAsset);
            Assert.Equal(oldSettings.MinSpread, newSettings.MinSpread);
        }

        [Fact]
        public async Task SetSettingsMinSpreadTest()
        {
            var oldSettings = await Client.GetSettingsAsync();

            var settings = new Settings { MinSpread = -97 };

            await Client.SetSettingsAsync(settings);

            var newSettings = await Client.GetSettingsAsync();
            Assert.Equal(settings.MinSpread, newSettings.MinSpread);

            await Client.SetSettingsAsync(oldSettings);

            newSettings = await Client.GetSettingsAsync();
            Assert.Equal(oldSettings.MinSpread, newSettings.MinSpread);
        }



        private void AssertOrderBook(OrderBook orderBook)
        {
            Assert.NotEmpty(orderBook.Source);
            Assert.False(orderBook.AssetPair.IsEmpty());
            Assert.NotEmpty(orderBook.Asks);
            Assert.NotEmpty(orderBook.Bids);
            Assert.NotEqual(default, orderBook.Asks.First().Price);
            Assert.NotEqual(default, orderBook.Asks.First().Volume);
            Assert.NotEqual(default, orderBook.Bids.First().Price);
            Assert.NotEqual(default, orderBook.Bids.First().Volume);
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
            Assert.NotEqual(default, crossRate.BestAsk.Price);
            Assert.NotEqual(default, crossRate.BestAsk.Volume);
            Assert.NotEqual(default, crossRate.BestBid.Price);
            Assert.NotEqual(default, crossRate.BestBid.Volume);
            Assert.NotEmpty(crossRate.ConversionPath);
            Assert.NotEqual(default, crossRate.Timestamp);
        }

        private void AssertArbitrage(Arbitrage arbitrage)
        {
            Assert.False(arbitrage.AssetPair.IsEmpty());
            AssertCrossRate(arbitrage.AskCrossRate);
            AssertCrossRate(arbitrage.BidCrossRate);
            Assert.NotEqual(default, arbitrage.Ask.Price);
            Assert.NotEqual(default, arbitrage.Ask.Volume);
            Assert.NotEqual(default, arbitrage.Bid.Price);
            Assert.NotEqual(default, arbitrage.Bid.Volume);
            Assert.NotEqual(default, arbitrage.Spread);
            Assert.NotEqual(default, arbitrage.Volume);
            Assert.NotEqual(default, arbitrage.PnL);
            Assert.NotEqual(default, arbitrage.StartedAt);
            Assert.NotEqual(default, arbitrage.EndedAt);
        }

        private void AssertArbitrageRow(ArbitrageRow arbitrage, bool ended)
        {
            Assert.False(arbitrage.AssetPair.IsEmpty());
            Assert.NotEmpty(arbitrage.AskSource);
            Assert.NotEmpty(arbitrage.BidSource);
            Assert.NotEmpty(arbitrage.AskConversionPath);
            Assert.NotEmpty(arbitrage.BidConversionPath);
            Assert.NotEqual(default, arbitrage.Ask.Price);
            Assert.NotEqual(default, arbitrage.Ask.Volume);
            Assert.NotEqual(default, arbitrage.Bid.Price);
            Assert.NotEqual(default, arbitrage.Bid.Volume);
            Assert.NotEqual(default, arbitrage.Spread);
            Assert.NotEqual(default, arbitrage.Volume);
            Assert.NotEqual(default, arbitrage.PnL);
            Assert.NotEqual(default, arbitrage.StartedAt);
            Assert.Equal(ended, arbitrage.EndedAt != default);
        }
    }
}
