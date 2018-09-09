using System;

namespace Lykke.Service.ArbitrageDetector.Core.Domain
{
    public struct AssetPairSource : IComparable
    {
        public string Exchange { get; }
        
        public AssetPair AssetPair { get; }

        public AssetPairSource(string exchange, AssetPair assetPair)
        {
            Exchange = string.IsNullOrEmpty(exchange) ? throw new ArgumentNullException(nameof(exchange)) : exchange;
            AssetPair = string.IsNullOrEmpty(assetPair.Base) || string.IsNullOrEmpty(assetPair.Quote) ? throw new ArgumentNullException(nameof(assetPair)) : assetPair;
        }

        public override string ToString()
        {
            return Exchange + "-" + AssetPair;
        }

        public int CompareTo(object obj)
        {
            if (!(obj is AssetPairSource))
                throw new ArgumentException(nameof(obj));

            return string.Compare(ToString(), ((AssetPairSource)obj).ToString(), StringComparison.Ordinal);
        }
    }
}
