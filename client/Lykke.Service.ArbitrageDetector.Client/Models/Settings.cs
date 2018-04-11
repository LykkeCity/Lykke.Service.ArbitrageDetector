using System;
using System.Collections.Generic;

namespace Lykke.Service.ArbitrageDetector.Client.Models
{
    /// <summary>
    /// Represents settings of Arbitrage Detector Service.
    /// </summary>
    public class Settings
    {
        /// <summary>
        /// Expiration time in milliseconds for order books and cross rates.
        /// </summary>
        public int? ExpirationTimeInSeconds { get; set; }

        /// <summary>
        /// Minimum PnL.
        /// </summary>
        public decimal? MinimumPnL { get; set; }

        /// <summary>
        /// Minimum volume.
        /// </summary>
        public decimal? MinimumVolume { get; set; }

        /// <summary>
        /// Minimum spread.
        /// </summary>
        public int? MinSpread { get; set; }

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
        /// <param name="intermediateAssets"></param>
        /// <param name="minSpread"></param>
        /// <param name="exchanges"></param>
        /// <param name="minimumPnL"></param>
        /// <param name="minimumVolume"></param>
        public Settings(int? expirationTimeInSeconds, IEnumerable<string> baseAssets, IEnumerable<string> intermediateAssets, string quoteAsset, int? minSpread,
            IEnumerable<string> exchanges, decimal? minimumPnL, decimal? minimumVolume)
        {
            ExpirationTimeInSeconds = expirationTimeInSeconds;
            BaseAssets = baseAssets;
            IntermediateAssets = intermediateAssets;
            QuoteAsset = quoteAsset;
            MinSpread = minSpread;
            Exchanges = exchanges;
            MinimumPnL = minimumPnL;
            MinimumVolume = minimumVolume;
        }
    }
}
