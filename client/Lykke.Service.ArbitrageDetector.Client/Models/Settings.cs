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
        public IEnumerable<string> BaseAssets { get; set; }

        /// <summary>
        /// Intermediate assets.
        /// </summary>
        public IEnumerable<string> IntermediateAssets { get; set; }

        /// <summary>
        /// Quote asset for wanted assets.
        /// </summary>
        public string QuoteAsset { get; set; }

        /// <summary>
        /// Wanted exchanges.
        /// </summary>
        public IEnumerable<string> Exchanges { get; set; }


        // Public Matrix

        /// <summary>
        /// Public matrix asset pairs.
        /// </summary>
        public IEnumerable<string> PublicMatrixAssetPairs { get; set; }

        /// <summary>
        /// Public matrix exchanges
        /// </summary>
        public IDictionary<string, string> PublicMatrixExchanges { get; set; }


        // Matrix

        /// <summary>
        /// Internal matrix asset pairs.
        /// </summary>
        public IEnumerable<string> MatrixAssetPairs { get; set; }

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
        public IEnumerable<string> MatrixHistoryAssetPairs { get; set; }

        /// <summary>
        /// Lykke exchange name for "arbitrages only" option.
        /// </summary>
        public string MatrixHistoryLykkeName { get; set; }

        /// <summary>
        /// Constructor.
        /// </summary>
        public Settings()
        {
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="historyMaxSize"></param>
        /// <param name="expirationTimeInSeconds"></param>
        /// <param name="baseAssets"></param>
        /// <param name="intermediateAssets"></param>
        /// <param name="quoteAsset"></param>
        /// <param name="minSpread"></param>
        /// <param name="exchanges"></param>
        /// <param name="minimumPnL"></param>
        /// <param name="minimumVolume"></param>
        /// <param name="publicMatrixAssetPairs"></param>
        /// <param name="publicMatrixExchanges"></param>
        /// <param name="matrixAssetPairs"></param>
        /// <param name="matrixHistoryInterval"></param>
        /// <param name="matrixHistoryAssetPairs"></param>
        /// <param name="matrixMinimumSpread"></param>
        /// <param name="matrixHistoryLykkeName"></param>
        public Settings(int historyMaxSize, int expirationTimeInSeconds, IEnumerable<string> baseAssets,
            IEnumerable<string> intermediateAssets, string quoteAsset, int minSpread, IEnumerable<string> exchanges, decimal minimumPnL, decimal minimumVolume,
            IEnumerable<string> publicMatrixAssetPairs, IDictionary<string, string> publicMatrixExchanges, IEnumerable<string> matrixAssetPairs,
            TimeSpan matrixHistoryInterval, IEnumerable<string> matrixHistoryAssetPairs, decimal? matrixMinimumSpread, string matrixHistoryLykkeName)
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
            MatrixHistoryInterval = matrixHistoryInterval;
            MatrixHistoryAssetPairs = matrixHistoryAssetPairs;
            MatrixSignificantSpread = matrixMinimumSpread;
            MatrixHistoryLykkeName = matrixHistoryLykkeName;
        }
    }
}
