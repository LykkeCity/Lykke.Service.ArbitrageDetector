using System;

namespace Lykke.Service.ArbitrageDetector.Client.Models
{
    /// <summary>
    /// Represents a pair of an exchange and an asset pair.
    /// </summary>
    public struct ExchangeAssetPair : IComparable
    {
        /// <summary>
        /// Exchange.
        /// </summary>
        public string Exchange { get; }
        
        /// <summary>
        /// Asset pair.
        /// </summary>
        public AssetPair AssetPair { get; }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="exchange"></param>
        /// <param name="assetPair"></param>
        public ExchangeAssetPair(string exchange, AssetPair assetPair)
        {
            Exchange = string.IsNullOrEmpty(exchange) ? throw new ArgumentNullException(nameof(exchange)) : exchange;
            AssetPair = string.IsNullOrEmpty(assetPair.Base) || string.IsNullOrEmpty(assetPair.Quoting) ? throw new ArgumentNullException(nameof(assetPair)) : assetPair;
        }

        public override string ToString()
        {
            return $"{Exchange}-{AssetPair}";
        }

        public int CompareTo(object obj)
        {
            if (obj == null || !(obj is ExchangeAssetPair))
                throw new ArgumentException(nameof(obj));

            return string.Compare(ToString(), ((ExchangeAssetPair)obj).ToString(), StringComparison.Ordinal);
        }
    }
}
