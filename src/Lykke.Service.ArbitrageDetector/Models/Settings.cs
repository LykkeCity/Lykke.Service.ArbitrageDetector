using System.Collections.Generic;
using Lykke.Service.ArbitrageDetector.Core.Domain.Interfaces;

namespace Lykke.Service.ArbitrageDetector.Models
{
    /// <summary>
    /// Represents settings of Arbitrage Detector Service.
    /// </summary>
    public class Settings
    {
        /// <summary>
        /// Maximum length of the history of arbitrages.
        /// </summary>
        public int HistoryMaxSize { get; set; }

        /// <summary>
        /// Expiration time in milliseconds for order books and cross rates.
        /// </summary>
        public int ExpirationTimeInSeconds { get; set; }

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

        /// <summary>
        /// Public matrix asset pairs.
        /// </summary>
        public IEnumerable<string> PublicMatrixAssetPairs { get; set; }

        /// <summary>
        /// Public matrix exchanges
        /// </summary>
        public IDictionary<string, string> PublicMatrixExchanges { get; set; }

        /// <summary>
        /// Internal matrix asset pairs.
        /// </summary>
        public IEnumerable<string> MatrixAssetPairs { get; set; }

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
        public Settings(int historyMaxSize, int expirationTimeInSeconds, IEnumerable<string> baseAssets,
            IEnumerable<string> intermediateAssets, string quoteAsset, int minSpread, IEnumerable<string> exchanges, decimal minimumPnL, decimal minimumVolume,
            IEnumerable<string> publicMatrixAssetPairs, IDictionary<string, string> publicMatrixExchanges, IEnumerable<string> matrixAssetPairs)
        {
            HistoryMaxSize = historyMaxSize;
            ExpirationTimeInSeconds = expirationTimeInSeconds;
            MinimumPnL = minimumPnL;
            MinimumVolume = minimumVolume;
            MinSpread = minSpread;
            BaseAssets = baseAssets ?? new List<string>();
            IntermediateAssets = intermediateAssets ?? new List<string>();
            QuoteAsset = quoteAsset;
            Exchanges = exchanges ?? new List<string>();
            PublicMatrixAssetPairs = publicMatrixAssetPairs ?? new List<string>();
            PublicMatrixExchanges = publicMatrixExchanges ?? new Dictionary<string, string>();
            MatrixAssetPairs = matrixAssetPairs ?? new List<string>();
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="settings">Domain model</param>
        public Settings(ISettings settings)
            : this(settings.HistoryMaxSize, settings.ExpirationTimeInSeconds, settings.BaseAssets,
                settings.IntermediateAssets, settings.QuoteAsset, settings.MinSpread, settings.Exchanges, settings.MinimumPnL, settings.MinimumVolume,
                settings.PublicMatrixAssetPairs, settings.PublicMatrixExchanges, settings.MatrixAssetPairs)
        {
        }

        /// <summary>
        /// Converts object to domain model.
        /// </summary>
        /// <returns>Domain model</returns>
        public ISettings ToModel()
        {
            var domain = new Core.Domain.Settings(HistoryMaxSize, ExpirationTimeInSeconds, BaseAssets, IntermediateAssets, QuoteAsset,
                MinSpread, Exchanges, MinimumPnL, MinimumVolume, PublicMatrixAssetPairs, PublicMatrixExchanges, MatrixAssetPairs);

            return domain;
        }
    }
}
