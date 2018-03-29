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
        /// <param name="baseAssets"></param>
        /// <param name="quoteAsset"></param>
        public StartupSettings(int executionDelayInMilliseconds, int expirationTimeInSeconds, int historyMaxSize,
            IEnumerable<string> baseAssets, string quoteAsset)
            : base(expirationTimeInSeconds, baseAssets, quoteAsset)
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
        /// Quote asset for wanted assets.
        /// </summary>
        public string QuoteAsset { get; set; }

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
        public Settings(int expirationTimeInSeconds, IEnumerable<string> baseAssets, string quoteAsset)
        {
            ExpirationTimeInSeconds = expirationTimeInSeconds;
            BaseAssets = baseAssets ?? throw new ArgumentNullException(nameof(baseAssets));
            QuoteAsset = string.IsNullOrWhiteSpace(quoteAsset) ? throw new ArgumentNullException(nameof(quoteAsset)) : quoteAsset;
        }
    }
}
