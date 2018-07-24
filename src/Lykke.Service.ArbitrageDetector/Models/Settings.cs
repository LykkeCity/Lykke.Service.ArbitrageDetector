using System;
using System.Collections.Generic;
using Lykke.Service.ArbitrageDetector.Core.Domain;

namespace Lykke.Service.ArbitrageDetector.Models
{
    public class Settings : ISettings
    {
        // Common

        public int ExpirationTimeInSeconds { get; set; }

        // Arbitrages, Synthetics

        public int HistoryMaxSize { get; set; }

        public decimal MinimumPnL { get; set; }

        public decimal MinimumVolume { get; set; }

        public int MinSpread { get; set; }

        public IEnumerable<string> BaseAssets { get; set; }

        public IEnumerable<string> IntermediateAssets { get; set; }

        public string QuoteAsset { get; set; }

        public IEnumerable<string> Exchanges { get; set; }

        // Matrix

        public IEnumerable<string> MatrixAssetPairs { get; set; }

        public decimal? MatrixSignificantSpread { get; set; }

        // Public Matrix

        public IEnumerable<string> PublicMatrixAssetPairs { get; set; }

        public IDictionary<string, string> PublicMatrixExchanges { get; set; }

        // Matrix History

        public TimeSpan MatrixHistoryInterval { get; set; }

        public IEnumerable<string> MatrixHistoryAssetPairs { get; set; }

        public string MatrixHistoryLykkeName { get; set; }


        public Settings()
        {
        }

        public Settings(int historyMaxSize, int expirationTimeInSeconds, IEnumerable<string> baseAssets,
            IEnumerable<string> intermediateAssets, string quoteAsset, int minSpread, IEnumerable<string> exchanges, decimal minimumPnL, decimal minimumVolume,
            IEnumerable<string> publicMatrixAssetPairs, IDictionary<string, string> publicMatrixExchanges, IEnumerable<string> matrixAssetPairs,
            TimeSpan matrixHistoryInterval, IEnumerable<string> matrixHistoryAssetPairs, decimal? matrixMinimumSpread, string matrixHistoryLykkeName)
        {
            HistoryMaxSize = historyMaxSize;
            ExpirationTimeInSeconds = expirationTimeInSeconds;
            MinimumPnL = minimumPnL;
            MinimumVolume = minimumVolume;
            MinSpread = minSpread;
            BaseAssets = baseAssets ?? new List<string>();
            IntermediateAssets = intermediateAssets ?? new List<string>();
            QuoteAsset = quoteAsset;
            Exchanges = exchanges ?? new List<string>();
            PublicMatrixAssetPairs = publicMatrixAssetPairs ?? new List<string>();
            PublicMatrixExchanges = publicMatrixExchanges ?? new Dictionary<string, string>();
            MatrixAssetPairs = matrixAssetPairs ?? new List<string>();
            MatrixHistoryInterval = matrixHistoryInterval;
            MatrixHistoryAssetPairs = matrixHistoryAssetPairs;
            MatrixSignificantSpread = matrixMinimumSpread;
            MatrixHistoryLykkeName = matrixHistoryLykkeName;
        }

        public Settings(ISettings settings)
            : this(settings.HistoryMaxSize, settings.ExpirationTimeInSeconds, settings.BaseAssets,
                settings.IntermediateAssets, settings.QuoteAsset, settings.MinSpread, settings.Exchanges, settings.MinimumPnL, settings.MinimumVolume,
                settings.PublicMatrixAssetPairs, settings.PublicMatrixExchanges, settings.MatrixAssetPairs,
                settings.MatrixHistoryInterval, settings.MatrixHistoryAssetPairs, settings.MatrixSignificantSpread, settings.MatrixHistoryLykkeName)
        {
        }

        public ISettings ToModel()
        {
            var domain = new Core.Domain.Settings(HistoryMaxSize, ExpirationTimeInSeconds, BaseAssets, IntermediateAssets, QuoteAsset,
                MinSpread, Exchanges, MinimumPnL, MinimumVolume, PublicMatrixAssetPairs, PublicMatrixExchanges, MatrixAssetPairs,
                MatrixHistoryInterval, MatrixHistoryAssetPairs, MatrixSignificantSpread, MatrixHistoryLykkeName);

            return domain;
        }
    }
}
