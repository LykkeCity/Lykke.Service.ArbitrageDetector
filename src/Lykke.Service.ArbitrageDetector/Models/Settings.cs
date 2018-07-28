using System;
using System.Collections.Generic;
using System.Linq;
using DomainSettings = Lykke.Service.ArbitrageDetector.Core.Domain.Settings;

namespace Lykke.Service.ArbitrageDetector.Models
{
    public class Settings
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

        // Other

        public IEnumerable<ExchangeFees> ExchangesFees { get; set; }


        public Settings(DomainSettings domain)
        {
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
            MatrixSignificantSpread = domain.MatrixSignificantSpread;
            MatrixHistoryLykkeName = domain.MatrixHistoryLykkeName;
            ExchangesFees = domain.ExchangesFees.Select(x => new ExchangeFees(x)).ToList();
        }

        public DomainSettings ToDomain()
        {
            var result = new DomainSettings
            {
                HistoryMaxSize = HistoryMaxSize,
                ExpirationTimeInSeconds = ExpirationTimeInSeconds,
                MinimumPnL = MinimumPnL,
                MinimumVolume = MinimumVolume,
                MinSpread = MinSpread,
                BaseAssets = BaseAssets,
                IntermediateAssets = IntermediateAssets,
                QuoteAsset = QuoteAsset,
                Exchanges = Exchanges,
                PublicMatrixAssetPairs = PublicMatrixAssetPairs,
                PublicMatrixExchanges = PublicMatrixExchanges,
                MatrixAssetPairs = MatrixAssetPairs,
                MatrixHistoryInterval = MatrixHistoryInterval,
                MatrixHistoryAssetPairs = MatrixHistoryAssetPairs,
                MatrixSignificantSpread = MatrixSignificantSpread,
                MatrixHistoryLykkeName = MatrixHistoryLykkeName,
                ExchangesFees = ExchangesFees.Select(x => x.ToDomain()).ToList()
            };

            return result;
        }
    }
}
