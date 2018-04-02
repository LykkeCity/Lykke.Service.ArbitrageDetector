using System;
using System.Collections.Generic;

namespace Lykke.Service.ArbitrageDetector.Core
{
    /// <summary>
    /// Represents settings of Arbitrage Detector Service.
    /// </summary>
    /// <inheritdoc />
    public class StartupSettings : Settings
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
        /// Constructor.
        /// </summary>
        public StartupSettings()
        {
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="executionDelayInMilliseconds"></param>
        /// <param name="expirationTimeInSeconds"></param>
        /// <param name="historyMaxSize"></param>
        /// <param name="minSpread"></param>
        /// <param name="baseAssets"></param>
        /// <param name="quoteAsset"></param>
        /// <param name="intermediateAssets"></param>
        public StartupSettings(int executionDelayInMilliseconds, int expirationTimeInSeconds, int historyMaxSize, int minSpread,
            IEnumerable<string> baseAssets, IEnumerable<string> intermediateAssets, string quoteAsset)
            : base(expirationTimeInSeconds, baseAssets, intermediateAssets, quoteAsset, minSpread)
        {
            ExecutionDelayInMilliseconds = executionDelayInMilliseconds;
            ExpirationTimeInSeconds = expirationTimeInSeconds;
            HistoryMaxSize = historyMaxSize;
        }
    }

    /// <summary>
    /// Represents settings that can be changed during service execution.
    /// </summary>
    public class Settings
    {
        /// <summary>
        /// Expiration time in milliseconds for order books and cross rates.
        /// </summary>
        public int ExpirationTimeInSeconds { get; set; }

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
        /// Minimum spread.
        /// </summary>
        public int MinSpread { get; set; }

        /// <summary>
        /// Constructor.
        /// </summary>
        public Settings()
        {
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="expirationTimeInSeconds"></param>
        /// <param name="baseAssets"></param>
        /// <param name="quoteAsset"></param>
        /// <param name="minSpread"></param>
        /// <param name="intermediateAssets"></param>
        public Settings(int expirationTimeInSeconds, IEnumerable<string> baseAssets, IEnumerable<string> intermediateAssets, string quoteAsset, int minSpread)
        {
            ExpirationTimeInSeconds = expirationTimeInSeconds;
            BaseAssets = baseAssets ?? throw new ArgumentNullException(nameof(baseAssets));
            IntermediateAssets = intermediateAssets ?? throw new ArgumentNullException(nameof(intermediateAssets));
            QuoteAsset = quoteAsset ?? throw new ArgumentNullException(nameof(quoteAsset));
            MinSpread = minSpread;
        }
    }
}
