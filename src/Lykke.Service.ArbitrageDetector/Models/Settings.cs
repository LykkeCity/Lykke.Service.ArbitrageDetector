using System.Collections.Generic;

namespace Lykke.Service.ArbitrageDetector.Models
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

        /// <summary>
        /// Constructor.
        /// </summary>
        public Settings()
        {
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="settings">Domain model</param>
        public Settings(Core.Settings settings)
        {
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
                BaseAssets = BaseAssets,
                QuoteAsset = QuoteAsset,
            };

            return domain;
        }
    }
}
