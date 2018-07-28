using System;
using System.Collections.Generic;

namespace Lykke.Service.ArbitrageDetector.Client.Models
{
    /// <summary>
    /// Represents settings of Arbitrage Detector Service.
    /// </summary>
    public class Settings
    {
        // Common

        /// <summary>
        /// Expiration time in milliseconds for order books and synthetic order books.
        /// </summary>
        public int ExpirationTimeInSeconds { get; set; }


        // Arbitrages, Synthetic

        /// <summary>
        /// Maximum length of the history of arbitrages.
        /// </summary>
        public int HistoryMaxSize { get; set; }

        /// <summary>
        /// Minimum PnL.
        /// </summary>
        public decimal MinimumPnL { get; set; }

        /// <summary>
        /// Minimum volume.
        /// </summary>
        public decimal MinimumVolume { get; set; }

        /// <summary>
        /// Minimum spread.
        /// </summary>
        public int MinSpread { get; set; }

        /// <summary>
        /// Wanted base assets.
        /// </summary>
        public IEnumerable<string> BaseAssets { get; set; } = new List<string>();

        /// <summary>
        /// Intermediate assets.
        /// </summary>
        public IEnumerable<string> IntermediateAssets { get; set; } = new List<string>();

        /// <summary>
        /// Quote asset for wanted assets.
        /// </summary>
        public string QuoteAsset { get; set; }

        /// <summary>
        /// Wanted exchanges.
        /// </summary>
        public IEnumerable<string> Exchanges { get; set; } = new List<string>();


        // Public Matrix

        /// <summary>
        /// Public matrix asset pairs.
        /// </summary>
        public IEnumerable<string> PublicMatrixAssetPairs { get; set; } = new List<string>();

        /// <summary>
        /// Public matrix exchanges
        /// </summary>
        public IDictionary<string, string> PublicMatrixExchanges { get; set; }


        // Matrix

        /// <summary>
        /// Internal matrix asset pairs.
        /// </summary>
        public IEnumerable<string> MatrixAssetPairs { get; set; } = new List<string>();

        /// <summary>
        /// Alert spread (highlighted with a color).
        /// </summary>
        public decimal? MatrixSignificantSpread { get; set; }


        // Matrix History

        /// <summary>
        /// Time interval for matrix snapshots to database.
        /// </summary>
        public TimeSpan MatrixHistoryInterval { get; set; }

        /// <summary>
        /// Asset pairs for matrix snapshots.
        /// </summary>
        public IEnumerable<string> MatrixHistoryAssetPairs { get; set; } = new List<string>();

        /// <summary>
        /// Lykke exchange name for "arbitrages only" option.
        /// </summary>
        public string MatrixHistoryLykkeName { get; set; }

        // Other

        /// <summary>
        /// Exchanges fees.
        /// </summary>
        public IEnumerable<ExchangeFees> ExchangesFees { get; set; } = new List<ExchangeFees>();
    }
}
