using System;
using System.Collections.Generic;

namespace Lykke.Service.ArbitrageDetector.Core.Domain
{
    /// <summary>
    /// Represents settings of Arbitrage Detector Service.
    /// </summary>
    public interface ISettings
    {
        // Common

        /// <summary>
        /// Expiration time in milliseconds for order books and cross rates.
        /// </summary>
        int ExpirationTimeInSeconds { get; set; }


        // Arbitrages, Synthetics

        /// <summary>
        /// Maximum length of the history of arbitrages.
        /// </summary>
        int HistoryMaxSize { get; set; }

        /// <summary>
        /// Minimum PnL.
        /// </summary>
        decimal MinimumPnL { get; set; }

        /// <summary>
        /// Minimum volume.
        /// </summary>
        decimal MinimumVolume { get; set; }

        /// <summary>
        /// Minimum spread.
        /// </summary>
        int MinSpread { get; set; }

        /// <summary>
        /// Wanted base assets.
        /// </summary>
        IEnumerable<string> BaseAssets { get; set; }

        /// <summary>
        /// Intermediate assets.
        /// </summary>
        IEnumerable<string> IntermediateAssets { get; set; }

        /// <summary>
        /// Quote asset for wanted assets.
        /// </summary>
        string QuoteAsset { get; set; }

        /// <summary>
        /// Wanted exchanges.
        /// </summary>
        IEnumerable<string> Exchanges { get; set; }


        // Matrix

        /// <summary>
        /// Internal matrix asset pairs.
        /// </summary>
        IEnumerable<string> MatrixAssetPairs { get; set; }

        /// <summary>
        /// Significant spread (highlighted with a color, used in MatrixHistory -> "Lykke arbitrages only").
        /// </summary>
        decimal? MatrixSignificantSpread { get; set; }


        // Public Matrix

        /// <summary>
        /// Public matrix asset pairs.
        /// </summary>
        IEnumerable<string> PublicMatrixAssetPairs { get; set; }

        /// <summary>
        /// Public matrix exchanges.
        /// </summary>
        IDictionary<string, string> PublicMatrixExchanges { get; set; }


        // Matrix History

        /// <summary>
        /// Time interval between matrix snapshots.
        /// </summary>
        TimeSpan MatrixHistoryInterval { get; set; }

        /// <summary>
        /// Asset pairs for matrix history.
        /// </summary>
        IEnumerable<string> MatrixHistoryAssetPairs { get; set; }

        /// <summary>
        /// Lykke exchange name for "arbitrages only" option.
        /// </summary>
        string MatrixHistoryLykkeName { get; set; }
    }
}
