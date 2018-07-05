﻿using System;
using System.Collections.Generic;
using Lykke.Service.ArbitrageDetector.Core.Domain.Interfaces;

namespace Lykke.Service.ArbitrageDetector.Core.Domain
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
            BaseAssets = baseAssets;
            IntermediateAssets = intermediateAssets;
            QuoteAsset = quoteAsset;
            Exchanges = exchanges;
            PublicMatrixAssetPairs = publicMatrixAssetPairs;
            PublicMatrixExchanges = publicMatrixExchanges;
            MatrixAssetPairs = matrixAssetPairs;
            MatrixSnapshotInterval = matrixSnapshotInterval;
            MatrixSnapshotAssetPairs = matrixSnapshotAssetPairs;
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
            MatrixSnapshotInterval = new TimeSpan(0, 0, 5, 0),
            MatrixSnapshotAssetPairs = new List<string>()
        };
    }
}
