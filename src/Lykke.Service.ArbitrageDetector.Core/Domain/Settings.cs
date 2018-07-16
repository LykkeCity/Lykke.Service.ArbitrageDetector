using System;
using System.Collections.Generic;

namespace Lykke.Service.ArbitrageDetector.Core.Domain
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

        public decimal? MatrixAlertSpread { get; set; }

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
            BaseAssets = baseAssets;
            IntermediateAssets = intermediateAssets;
            QuoteAsset = quoteAsset;
            Exchanges = exchanges;
            PublicMatrixAssetPairs = publicMatrixAssetPairs;
            PublicMatrixExchanges = publicMatrixExchanges;
            MatrixAssetPairs = matrixAssetPairs;
            MatrixHistoryInterval = matrixHistoryInterval;
            MatrixHistoryAssetPairs = matrixHistoryAssetPairs;
            MatrixAlertSpread = matrixMinimumSpread;
            MatrixHistoryLykkeName = matrixHistoryLykkeName;
        }

        public static ISettings Default { get; } = new Settings
        {
            HistoryMaxSize = 50,
            ExpirationTimeInSeconds = 10,
            MinimumPnL = 10,
            MinimumVolume = 0.001m,
            MinSpread = -5,
            BaseAssets = new List<string>(),
            IntermediateAssets = new List<string>(),
            QuoteAsset = "USD",
            Exchanges = new List<string>(),
            PublicMatrixAssetPairs = new List<string>(),
            PublicMatrixExchanges = new Dictionary<string, string>(),
            MatrixAssetPairs = new List<string>(),
            MatrixAlertSpread = -1,
            MatrixHistoryInterval = new TimeSpan(0, 0, 5, 0),
            MatrixHistoryAssetPairs = new List<string>(),
            MatrixHistoryLykkeName = "lykke"
        };
    }
}
