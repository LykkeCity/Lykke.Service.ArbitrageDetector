﻿using System;
using System.Collections.Generic;
using Lykke.AzureStorage.Tables;
using Lykke.AzureStorage.Tables.Entity.Annotation;
using Lykke.Service.ArbitrageDetector.Core.Domain.Interfaces;

namespace Lykke.Service.ArbitrageDetector.AzureRepositories.Models
{
    public class Settings : AzureTableEntity, ISettings
    {
        public int HistoryMaxSize { get; set; }

        public int ExpirationTimeInSeconds { get; set; }

        public decimal MinimumPnL { get; set; }

        public decimal MinimumVolume { get; set; }

        public int MinSpread { get; set; }

        [JsonValueSerializer]
        public IEnumerable<string> BaseAssets { get; set; }

        [JsonValueSerializer]
        public IEnumerable<string> IntermediateAssets { get; set; }

        public string QuoteAsset { get; set; }

        [JsonValueSerializer]
        public IEnumerable<string> Exchanges { get; set; }

        [JsonValueSerializer]
        public IEnumerable<string> PublicMatrixAssetPairs { get; set; }

        [JsonValueSerializer]
        public IDictionary<string, string> PublicMatrixExchanges { get; set; }

        [JsonValueSerializer]
        public IEnumerable<string> MatrixAssetPairs { get; set; }

        public TimeSpan MatrixSnapshotInterval { get; set; }

        [JsonValueSerializer]
        public IEnumerable<string> MatrixSnapshotAssetPairs { get; set; }

        public Settings()
        {
        }

        public Settings(ISettings domain)
        {
            PartitionKey = "";
            RowKey = "";
            HistoryMaxSize = domain.HistoryMaxSize;
            ExpirationTimeInSeconds = domain.ExpirationTimeInSeconds;
            MinimumPnL = domain.MinimumPnL;
            MinimumVolume = domain.MinimumVolume;
            MinSpread = domain.MinSpread;
            BaseAssets = domain.BaseAssets;
            IntermediateAssets = domain.IntermediateAssets;
            QuoteAsset = domain.QuoteAsset;
            Exchanges = domain.Exchanges;
            PublicMatrixAssetPairs = domain.PublicMatrixAssetPairs;
            PublicMatrixExchanges = domain.PublicMatrixExchanges;
            MatrixAssetPairs = domain.MatrixAssetPairs;
            MatrixSnapshotInterval = domain.MatrixSnapshotInterval;
            MatrixSnapshotAssetPairs = domain.MatrixSnapshotAssetPairs;
        }
    }
}