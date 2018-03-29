using System;
using System.Collections.Generic;

namespace Lykke.Service.ArbitrageDetector.Models
{
    /// <summary>
    /// Represents settings of Arbitrage Detector Service.
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
            QuoteAsset = quoteAsset ?? throw new ArgumentNullException(nameof(quoteAsset));
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="settings">Domain model</param>
        public Settings(Core.Settings settings)
        {
            ExpirationTimeInSeconds = settings.ExpirationTimeInSeconds;
            BaseAssets = settings.BaseAssets;
            QuoteAsset = settings.QuoteAsset;
        }

        /// <summary>
        /// Converts object to domain model.
        /// </summary>
        /// <returns>Domain model</returns>
        public Core.Settings ToModel()
        {
            var domain = new Core.Settings
            {
                ExpirationTimeInSeconds = ExpirationTimeInSeconds,
                BaseAssets = BaseAssets,
                QuoteAsset = QuoteAsset,
            };

            return domain;
        }
    }
}
