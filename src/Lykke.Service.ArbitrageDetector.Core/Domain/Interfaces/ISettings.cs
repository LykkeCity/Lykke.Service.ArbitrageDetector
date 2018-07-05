using System;
using System.Collections.Generic;

namespace Lykke.Service.ArbitrageDetector.Core.Domain.Interfaces
{
    /// <summary>
    /// Represents settings of Arbitrage Detector Service.
    /// </summary>
    public interface ISettings
    {
        /// <summary>
        /// Maximum length of the history of arbitrages.
        /// </summary>
        int HistoryMaxSize { get; set; }

        /// <summary>
        /// Expiration time in milliseconds for order books and cross rates.
        /// </summary>
        int ExpirationTimeInSeconds { get; set; }

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

        /// <summary>
        /// Public matrix asset pairs.
        /// </summary>
        IEnumerable<string> PublicMatrixAssetPairs { get; set; }

        /// <summary>
        /// Public matrix exchanges.
        /// </summary>
        IDictionary<string, string> PublicMatrixExchanges { get; set; }

        /// <summary>
        /// Internal matrix asset pairs.
        /// </summary>
        IEnumerable<string> MatrixAssetPairs { get; set; }

        /// <summary>
        /// Time interval for matrix snapshots to database.
        /// </summary>
        TimeSpan MatrixSnapshotInterval { get; set; }

        /// <summary>
        /// Asset pairs for matrix snapshots.
        /// </summary>
        IEnumerable<string> MatrixSnapshotAssetPairs { get; set; }
    }
}
