using System;
using System.Collections.Generic;
using System.Linq;

namespace Lykke.Service.ArbitrageDetector.Core.Domain
{
    public class Settings
    {
        // Common

        public int ExpirationTimeInSeconds { get; set; } = 10;

        // Arbitrages, Synthetic

        public int HistoryMaxSize { get; set; } = 50;

        public decimal MinimumPnL { get; set; } = 0;

        public decimal MinimumVolume { get; set; } = 0m;

        public int MinSpread { get; set; } = 0;

        public IEnumerable<string> BaseAssets { get; set; } = new List<string>();

        public string QuoteAsset { get; set; } = "USD";

        public IEnumerable<string> IntermediateAssets { get; set; } = new List<string>();

        public IEnumerable<string> Exchanges { get; set; }  = new List<string>();

        public int SynthMaxDepth { get; set; } = 30;

        // Matrix

        public IEnumerable<string> MatrixAssetPairs { get; set; } = new List<string>();

        public decimal? MatrixSignificantSpread { get; set; } = -1;

        public IEnumerable<ExchangeFees> ExchangesFees { get; set; } = new List<ExchangeFees>();

        // Public Matrix

        public IEnumerable<string> PublicMatrixAssetPairs { get; set; } = new List<string>();

        public IDictionary<string, string> PublicMatrixExchanges { get; set; } = new Dictionary<string, string>();

        // Matrix History

        public TimeSpan MatrixHistoryInterval { get; set; } = new TimeSpan(0, 0, 5, 0);

        public IEnumerable<string> MatrixHistoryAssetPairs { get; set; } = new List<string>();

        public string MatrixHistoryLykkeName { get; set; } = "lykke";
    }
}
