using System;
using System.Collections.Generic;
using Lykke.AzureStorage.Tables;
using Lykke.AzureStorage.Tables.Entity.Annotation;

namespace Lykke.Service.ArbitrageDetector.AzureRepositories.Models
{
    public class Settings : AzureTableEntity
    {
        // Common

        public int ExpirationTimeInSeconds { get; set; }

        // Arbitrages, Synthetic

        public int HistoryMaxSize { get; set; }

        public decimal MinimumPnL { get; set; }

        public decimal MinimumVolume { get; set; }

        public int MinSpread { get; set; }

        [JsonValueSerializer]
        public IEnumerable<string> BaseAssets { get; set; } = new List<string>();

        [JsonValueSerializer]
        public IEnumerable<string> IntermediateAssets { get; set; } = new List<string>();

        public string QuoteAsset { get; set; }

        [JsonValueSerializer]
        public IEnumerable<string> Exchanges { get; set; } = new List<string>();

        public int SynthMaxDepth { get; set; }

        // Lykke Arbitrages

        public TimeSpan LykkeArbitragesExecutionInterval { get; set; } = new TimeSpan(0, 0, 0, 10);

        // Matrix

        [JsonValueSerializer]
        public IEnumerable<string> MatrixAssetPairs { get; set; } = new List<string>();

        public decimal? MatrixSignificantSpread { get; set; }

        [JsonValueSerializer]
        public IEnumerable<ExchangeFees> ExchangesFees { get; set; } = new List<ExchangeFees>();

        // Public Matrix

        [JsonValueSerializer]
        public IEnumerable<string> PublicMatrixAssetPairs { get; set; } = new List<string>();

        [JsonValueSerializer]
        public IDictionary<string, string> PublicMatrixExchanges { get; set; } = new Dictionary<string, string>();

        // Matrix History

        public TimeSpan MatrixHistoryInterval { get; set; }

        [JsonValueSerializer]
        public IEnumerable<string> MatrixHistoryAssetPairs { get; set; } = new List<string>();

        public string MatrixHistoryLykkeName { get; set; }
    }
}
