using System;
using System.Collections.Generic;
using Lykke.AzureStorage.Tables;
using Lykke.AzureStorage.Tables.Entity.Annotation;
using Lykke.Service.ArbitrageDetector.Core.Domain;

namespace Lykke.Service.ArbitrageDetector.AzureRepositories.Models
{
    public class Settings : AzureTableEntity, ISettings
    {
        // Common

        public int ExpirationTimeInSeconds { get; set; }

        // Arbitrages, Synthetic

        public int HistoryMaxSize { get; set; }

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

        // Matrix

        [JsonValueSerializer]
        public IEnumerable<string> MatrixAssetPairs { get; set; }

        public decimal? MatrixAlertSpread { get; set; }

        // Public Matrix

        [JsonValueSerializer]
        public IEnumerable<string> PublicMatrixAssetPairs { get; set; }

        [JsonValueSerializer]
        public IDictionary<string, string> PublicMatrixExchanges { get; set; }

        // Matrix History

        public TimeSpan MatrixHistoryInterval { get; set; }

        [JsonValueSerializer]
        public IEnumerable<string> MatrixHistoryAssetPairs { get; set; }

        public string MatrixHistoryLykkeName { get; set; }

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
            MatrixHistoryInterval = domain.MatrixHistoryInterval;
            MatrixHistoryAssetPairs = domain.MatrixHistoryAssetPairs;
            MatrixAlertSpread = domain.MatrixAlertSpread;
            MatrixHistoryLykkeName = domain.MatrixHistoryLykkeName;
        }
    }
}
