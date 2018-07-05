using System;
using System.Collections.Generic;
using Lykke.Service.ArbitrageDetector.Core.Domain.Interfaces;

namespace Lykke.Service.ArbitrageDetector.Models
{
    public class Settings : ISettings
    {
        public int HistoryMaxSize { get; set; }

        public int ExpirationTimeInSeconds { get; set; }

        public decimal MinimumPnL { get; set; }

        public decimal MinimumVolume { get; set; }
        
        public int MinSpread { get; set; }


        public IEnumerable<string> BaseAssets { get; set; }

        public IEnumerable<string> IntermediateAssets { get; set; }

        public string QuoteAsset { get; set; }

        public IEnumerable<string> Exchanges { get; set; }


        public IEnumerable<string> PublicMatrixAssetPairs { get; set; }

        public IDictionary<string, string> PublicMatrixExchanges { get; set; }


        public IEnumerable<string> MatrixAssetPairs { get; set; }


        public TimeSpan MatrixSnapshotInterval { get; set; }

        public IEnumerable<string> MatrixSnapshotAssetPairs { get; set; }


        public Settings()
        {
        }

        public Settings(int historyMaxSize, int expirationTimeInSeconds, IEnumerable<string> baseAssets,
            IEnumerable<string> intermediateAssets, string quoteAsset, int minSpread, IEnumerable<string> exchanges, decimal minimumPnL, decimal minimumVolume,
            IEnumerable<string> publicMatrixAssetPairs, IDictionary<string, string> publicMatrixExchanges, IEnumerable<string> matrixAssetPairs,
            TimeSpan matrixSnapshotInterval, IEnumerable<string> matrixSnapshotAssetPairs)
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
            MatrixSnapshotInterval = matrixSnapshotInterval;
            MatrixSnapshotAssetPairs = matrixSnapshotAssetPairs;
        }

        public Settings(ISettings settings)
            : this(settings.HistoryMaxSize, settings.ExpirationTimeInSeconds, settings.BaseAssets,
                settings.IntermediateAssets, settings.QuoteAsset, settings.MinSpread, settings.Exchanges, settings.MinimumPnL, settings.MinimumVolume,
                settings.PublicMatrixAssetPairs, settings.PublicMatrixExchanges, settings.MatrixAssetPairs,
                settings.MatrixSnapshotInterval, settings.MatrixSnapshotAssetPairs)
        {
        }

        public ISettings ToModel()
        {
            var domain = new Core.Domain.Settings(HistoryMaxSize, ExpirationTimeInSeconds, BaseAssets, IntermediateAssets, QuoteAsset,
                MinSpread, Exchanges, MinimumPnL, MinimumVolume, PublicMatrixAssetPairs, PublicMatrixExchanges, MatrixAssetPairs,
                MatrixSnapshotInterval, MatrixSnapshotAssetPairs);

            return domain;
        }
    }
}
