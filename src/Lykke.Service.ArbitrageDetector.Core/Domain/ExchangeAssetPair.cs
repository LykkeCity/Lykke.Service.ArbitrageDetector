using System;

namespace Lykke.Service.ArbitrageDetector.Core.Domain
{
    public struct ExchangeAssetPair : IComparable
    {
        public string Exchange { get; }
        
        public AssetPair AssetPair { get; }

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
