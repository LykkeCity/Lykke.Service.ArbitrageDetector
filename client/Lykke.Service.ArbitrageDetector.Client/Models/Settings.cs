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
        /// Wanted base assets.
        /// </summary>
        public IEnumerable<string> BaseAssets { get; set; }

        /// <summary>
        /// Quote asset for wanted assets.
        /// </summary>
        public string QuoteAsset { get; set; }

        public Settings()
        {
        }

        public Settings(IEnumerable<string> baseAssets, string quoteAsset)
        {
            BaseAssets = baseAssets ?? throw new ArgumentNullException(nameof(baseAssets));
            QuoteAsset = quoteAsset ?? throw new ArgumentNullException(nameof(quoteAsset));
        }
    }
}
