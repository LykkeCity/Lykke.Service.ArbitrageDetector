using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Lykke.Service.ArbitrageDetector.Core.Domain;
using Lykke.Service.ArbitrageDetector.Core.Repositories;
using Lykke.Service.ArbitrageDetector.Services;
using Moq;
using Xunit;

namespace Lykke.Service.ArbitrageDetector.Tests
{
    public class SettingsServiceTests
    {
        [Fact]
        public async Task SettingsSetAllTest()
        {
            var startupSettings = new Settings
            {
                HistoryMaxSize = 77,
                ExpirationTimeInSeconds = 77,
                BaseAssets = new List<string> { "BTC" },
                IntermediateAssets = new List<string> { "BTC" },
                QuoteAsset = "BTC",
                MinSpread = -77,
                Exchanges = new List<string> { "GDAX" },
                MinimumPnL = 77,
                MinimumVolume = 77,
                PublicMatrixAssetPairs = new List<string> { "BTCUSD" },
                PublicMatrixExchanges = new Dictionary<string, string> { { "GDAX(e)", "GDAX" } },
                MatrixAssetPairs = new List<string> { "BTCUSD" },
                MatrixHistoryInterval = new TimeSpan(0, 0, 7, 0),
                MatrixHistoryAssetPairs = new List<string> { "BTCUSD" },
                MatrixSignificantSpread = -77,
                MatrixHistoryLykkeName = "lykke77",
                ExchangesFees = new List<ExchangeFees> { new ExchangeFees("GDAX", 0.77m, 0.77m) }
            };

            var settingsService = new SettingsService(SettingsRepository(startupSettings));

            var settings = new Settings
            {
                HistoryMaxSize = 77, // shouldn't be changed
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
                ExchangesFees = new List<ExchangeFees> { new ExchangeFees ( "Qoinex", 0.33m, 0.33m ) }
            };

            await settingsService.SetAsync(settings);

            var newSettings = await settingsService.GetAsync();
            Assert.Equal(settings.HistoryMaxSize, newSettings.HistoryMaxSize);
            Assert.Equal(settings.ExpirationTimeInSeconds, newSettings.ExpirationTimeInSeconds);
            Assert.True(settings.BaseAssets.SequenceEqual(newSettings.BaseAssets));
            Assert.True(settings.IntermediateAssets.SequenceEqual(newSettings.IntermediateAssets));
            Assert.Equal(settings.QuoteAsset, newSettings.QuoteAsset);
            Assert.Equal(settings.MinSpread, newSettings.MinSpread);
            Assert.True(settings.Exchanges.SequenceEqual(newSettings.Exchanges));
            Assert.Equal(settings.MinimumPnL, newSettings.MinimumPnL);
            Assert.Equal(settings.MinimumVolume, newSettings.MinimumVolume);
            Assert.True(settings.PublicMatrixAssetPairs.SequenceEqual(newSettings.PublicMatrixAssetPairs));
            Assert.True(settings.PublicMatrixExchanges.SequenceEqual(newSettings.PublicMatrixExchanges));
            Assert.True(settings.MatrixAssetPairs.SequenceEqual(newSettings.MatrixAssetPairs));
            Assert.Equal(settings.MatrixHistoryInterval, newSettings.MatrixHistoryInterval);
            Assert.True(settings.MatrixHistoryAssetPairs.SequenceEqual(newSettings.MatrixHistoryAssetPairs));
            Assert.Equal(settings.MatrixSignificantSpread, newSettings.MatrixSignificantSpread);
            Assert.Equal(settings.MatrixHistoryLykkeName, newSettings.MatrixHistoryLykkeName);
            Assert.Equal(settings.ExchangesFees.Count(), newSettings.ExchangesFees.Count());
            Assert.Equal(settings.ExchangesFees.Single().ExchangeName, newSettings.ExchangesFees.Single().ExchangeName);
            Assert.Equal(settings.ExchangesFees.Single().DepositFee, newSettings.ExchangesFees.Single().DepositFee);
            Assert.Equal(settings.ExchangesFees.Single().TradingFee, newSettings.ExchangesFees.Single().TradingFee);
        }

        private ISettingsRepository SettingsRepository(Settings settings)
        {
            var settingsRepository = new Mock<ISettingsRepository>();
            settingsRepository.Setup(x => x.GetAsync()).ReturnsAsync(settings);

            return settingsRepository.Object;
        }
    }
}
