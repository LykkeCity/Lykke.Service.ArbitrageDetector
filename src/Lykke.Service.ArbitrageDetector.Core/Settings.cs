using System;
using System.Collections.Generic;
using System.Linq;

namespace Lykke.Service.ArbitrageDetector.Core
{
    /// <summary>
    /// Represents settings that can be changed during service execution.
    /// </summary>
    public class Settings : ISettings
    {
        /// <summary>
        /// Arbitrage calculating execution delay in milliseconds.
        /// </summary>
        public int ExecutionDelayInMilliseconds { get; set; }

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
        /// Constructor.
        /// </summary>
        public Settings()
        {
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="executionDelayInMilliseconds"></param>
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
        public Settings(int executionDelayInMilliseconds, int historyMaxSize, int expirationTimeInSeconds, IEnumerable<string> baseAssets,
            IEnumerable<string> intermediateAssets, string quoteAsset, int minSpread, IEnumerable<string> exchanges, decimal minimumPnL, decimal minimumVolume,
            IEnumerable<string> publicMatrixAssetPairs, IDictionary<string, string> publicMatrixExchanges)
        {
            ExecutionDelayInMilliseconds = executionDelayInMilliseconds;
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
        }

        /// <summary>
        /// Validation.
        /// </summary>
        public void Validate()
        {
            if (BaseAssets == null || !BaseAssets.Any())
                throw new ArgumentOutOfRangeException(nameof(BaseAssets));

            if (IntermediateAssets == null)
                throw new ArgumentOutOfRangeException(nameof(IntermediateAssets));

            if (string.IsNullOrWhiteSpace(QuoteAsset))
                throw new ArgumentOutOfRangeException(nameof(QuoteAsset));

            if (Exchanges == null)
                throw new ArgumentOutOfRangeException(nameof(Exchanges));

            if (PublicMatrixAssetPairs == null)
                throw new ArgumentOutOfRangeException(nameof(PublicMatrixAssetPairs));

            if (PublicMatrixExchanges == null)
                throw new ArgumentOutOfRangeException(nameof(PublicMatrixExchanges));
        }
    }
}
