using System;

namespace Lykke.Service.ArbitrageDetector.Client.Models
{
    /// <summary>
    /// Represents an asset pair.
    /// </summary>
    public struct AssetPair
    {
        /// <summary>
        /// Base asset.
        /// </summary>
        public string Base{ get; }
        
        /// <summary>
        /// Quoting asset.
        /// </summary>
        public string Quoting { get; }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="_base">Base asset.</param>
        /// <param name="quoting">Quoting asset.</param>
        public AssetPair(string _base, string quoting)
        {
            Base = string.IsNullOrEmpty(_base) ? throw new ArgumentNullException(nameof(_base)) : _base;
            Quoting = string.IsNullOrEmpty(quoting) ? throw new ArgumentNullException(nameof(quoting)) : quoting;
        }
    }
}
