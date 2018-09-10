using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Lykke.Service.ArbitrageDetector.Core.Domain;
using Lykke.Service.ArbitrageDetector.Core.Repositories;
using Lykke.Service.ArbitrageDetector.Core.Services;

namespace Lykke.Service.ArbitrageDetector.Services
{
    public class SettingsService : ISettingsService
    {
        private readonly object _sync = new object();
        private Settings _settings;
        private Settings Settings { get { lock (_sync) { return _settings; } } set { lock (_sync) { _settings = value; } } }
        private readonly ISettingsRepository _settingsRepository;

        public SettingsService(ISettingsRepository settingsRepository)
        {
            _settingsRepository = settingsRepository;
        }

        public async Task<Settings> GetAsync()
        {
            if (Settings == null)
                Settings = await _settingsRepository.GetAsync();

            if (Settings == null)
            {
                Settings = new Settings();
                await _settingsRepository.InsertOrReplaceAsync(Settings);
            }

            return Settings;
        }

        public async Task SetAsync(Settings settings)
        {
            settings = await MergeSettings(settings);
            await _settingsRepository.InsertOrReplaceAsync(settings);
            Settings = settings;
        }

        public async Task<Settings> MergeSettings(Settings sNew)
        {
            var s = await GetAsync();

            sNew.ExpirationTimeInSeconds = sNew.ExpirationTimeInSeconds < 0 ? 0 : sNew.ExpirationTimeInSeconds;
            if (s.ExpirationTimeInSeconds != sNew.ExpirationTimeInSeconds)
            {
                s.ExpirationTimeInSeconds = sNew.ExpirationTimeInSeconds;
            }

            sNew.MinimumPnL = sNew.MinimumPnL < 0 ? 0 : sNew.MinimumPnL;
            if (s.MinimumPnL != sNew.MinimumPnL)
            {
                s.MinimumPnL = sNew.MinimumPnL;
            }

            sNew.MinimumVolume = sNew.MinimumVolume < 0 ? 0 : sNew.MinimumVolume;
            if (s.MinimumVolume != sNew.MinimumVolume)
            {
                s.MinimumVolume = sNew.MinimumVolume;
            }

            sNew.MinSpread = sNew.MinSpread >= 0 || sNew.MinSpread < -100 ? 0 : sNew.MinSpread;
            if (s.MinSpread != sNew.MinSpread)
            {
                s.MinSpread = sNew.MinSpread;
            }

            if (sNew.IntermediateAssets != null && !s.IntermediateAssets.SequenceEqual(sNew.IntermediateAssets))
            {
                s.IntermediateAssets = sNew.IntermediateAssets.Where(x => !string.IsNullOrWhiteSpace(x)).Select(x => x.Trim()).ToList();
            }

            if (sNew.BaseAssets != null && !s.BaseAssets.SequenceEqual(sNew.BaseAssets))
            {
                s.BaseAssets = sNew.BaseAssets.Where(x => !string.IsNullOrWhiteSpace(x)).Select(x => x.Trim()).ToList();
            }

            if (!string.IsNullOrWhiteSpace(sNew.QuoteAsset) && s.QuoteAsset != sNew.QuoteAsset)
            {
                s.QuoteAsset = sNew.QuoteAsset.Trim();
            }

            if (sNew.Exchanges != null && !s.Exchanges.SequenceEqual(sNew.Exchanges))
            {
                s.Exchanges = sNew.Exchanges.Where(x => !string.IsNullOrWhiteSpace(x)).Select(x => x.Trim()).ToList();
            }

            if (sNew.LykkeArbitragesExecutionInterval != default &&
                s.LykkeArbitragesExecutionInterval != sNew.LykkeArbitragesExecutionInterval)
            {
                s.LykkeArbitragesExecutionInterval = sNew.LykkeArbitragesExecutionInterval;
            }

            if (sNew.PublicMatrixAssetPairs != null && !s.PublicMatrixAssetPairs.SequenceEqual(sNew.PublicMatrixAssetPairs))
            {
                s.PublicMatrixAssetPairs = sNew.PublicMatrixAssetPairs.Where(x => !string.IsNullOrWhiteSpace(x)).Select(x => x.Trim()).ToList();
            }

            if (sNew.PublicMatrixExchanges != null && !s.PublicMatrixExchanges.SequenceEqual(sNew.PublicMatrixExchanges))
            {
                s.PublicMatrixExchanges = sNew.PublicMatrixExchanges;
            }

            if (sNew.MatrixAssetPairs != null && !sNew.MatrixAssetPairs.SequenceEqual(s.MatrixAssetPairs ?? new List<string>()))
            {
                s.MatrixAssetPairs = sNew.MatrixAssetPairs.Where(x => !string.IsNullOrWhiteSpace(x)).Select(x => x.Trim()).ToList();
            }

            sNew.MatrixSignificantSpread = sNew.MatrixSignificantSpread >= 0 || sNew.MatrixSignificantSpread < -100 ? null : sNew.MatrixSignificantSpread;
            if (s.MatrixSignificantSpread != sNew.MatrixSignificantSpread)
            {
                s.MatrixSignificantSpread = sNew.MatrixSignificantSpread;
            }

            if (sNew.MatrixHistoryAssetPairs != null && !sNew.MatrixHistoryAssetPairs.SequenceEqual(s.MatrixHistoryAssetPairs ?? new List<string>()))
            {
                s.MatrixHistoryAssetPairs = sNew.MatrixHistoryAssetPairs.Where(x => !string.IsNullOrWhiteSpace(x)).Select(x => x.Trim()).ToList();
            }

            sNew.MatrixHistoryInterval = (int)sNew.MatrixHistoryInterval.TotalMinutes < 0 ? new TimeSpan(0, 0, 5, 0) : sNew.MatrixHistoryInterval;
            if (s.MatrixHistoryInterval != sNew.MatrixHistoryInterval)
            {
                s.MatrixHistoryInterval = sNew.MatrixHistoryInterval;
            }

            if (!string.IsNullOrWhiteSpace(sNew.MatrixHistoryLykkeName) && s.MatrixHistoryLykkeName != sNew.MatrixHistoryLykkeName)
            {
                s.MatrixHistoryLykkeName = sNew.MatrixHistoryLykkeName.Trim();
            }

            if (sNew.ExchangesFees != null && !s.ExchangesFees.SequenceEqual(sNew.ExchangesFees))
            {
                s.ExchangesFees = sNew.ExchangesFees;
            }

            return s;
        }
    }
}
