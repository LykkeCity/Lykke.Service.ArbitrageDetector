using System;
using System.Collections.Generic;
using System.Linq;
using Lykke.AzureStorage.Tables;
using Lykke.AzureStorage.Tables.Entity.Annotation;
using DomainSettings = Lykke.Service.ArbitrageDetector.Core.Domain.Settings;

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

        // Matrix

        [JsonValueSerializer]
        public IEnumerable<string> MatrixAssetPairs { get; set; } = new List<string>();

        public decimal? MatrixSignificantSpread { get; set; }

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

        // Other

        [JsonValueSerializer]
        public IEnumerable<ExchangeFees> ExchangesFees { get; set; } = new List<ExchangeFees>();


        public Settings()
        {
        }

        public Settings(DomainSettings domain)
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
            SynthMaxDepth = domain.SynthMaxDepth;
            PublicMatrixAssetPairs = domain.PublicMatrixAssetPairs;
            PublicMatrixExchanges = domain.PublicMatrixExchanges;
            MatrixAssetPairs = domain.MatrixAssetPairs;
            MatrixHistoryInterval = domain.MatrixHistoryInterval;
            MatrixHistoryAssetPairs = domain.MatrixHistoryAssetPairs;
            MatrixSignificantSpread = domain.MatrixSignificantSpread;
            MatrixHistoryLykkeName = domain.MatrixHistoryLykkeName;
            ExchangesFees = domain.ExchangesFees.Select(x => new ExchangeFees(x)).ToList();
        }

        public DomainSettings ToDomain()
        {
            var result = new DomainSettings();

            result.HistoryMaxSize = HistoryMaxSize;
            result.ExpirationTimeInSeconds = ExpirationTimeInSeconds;
            result.MinimumPnL = MinimumPnL;
            result.MinimumVolume = MinimumVolume;
            result.MinSpread = MinSpread;
            result.BaseAssets = BaseAssets;
            result.IntermediateAssets = IntermediateAssets;
            result.QuoteAsset = QuoteAsset;
            result.Exchanges = Exchanges;
            result.SynthMaxDepth = SynthMaxDepth;
            result.PublicMatrixAssetPairs = PublicMatrixAssetPairs;
            result.PublicMatrixExchanges = PublicMatrixExchanges;
            result.MatrixAssetPairs = MatrixAssetPairs;
            result.MatrixHistoryInterval = MatrixHistoryInterval;
            result.MatrixHistoryAssetPairs = MatrixHistoryAssetPairs;
            result.MatrixSignificantSpread = MatrixSignificantSpread;
            result.MatrixHistoryLykkeName = MatrixHistoryLykkeName;
            result.ExchangesFees = ExchangesFees.Select(x => x.ToDomain()).ToList();

            return result;
        }
    }
}
