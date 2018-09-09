using System;
using System.Diagnostics;

namespace Lykke.Service.ArbitrageDetector.Core.Domain
{
    public struct AssetPairSource : IComparable
    {
        public string Exchange { get; }
        
        public AssetPair AssetPair { get; }

        public AssetPairSource(string exchange, AssetPair assetPair)
        {
            Debug.Assert(!string.IsNullOrEmpty(exchange));
            Debug.Assert(!string.IsNullOrEmpty(assetPair.Base));
            Debug.Assert(!string.IsNullOrEmpty(assetPair.Quote));

            Exchange = exchange;
            AssetPair = assetPair;
        }

        public override string ToString()
        {
            return$"{Exchange}-{AssetPair}";
        }

        public int CompareTo(object obj)
        {
            if (!(obj is AssetPairSource))
                throw new ArgumentException(nameof(obj));

            return string.Compare(ToString(), ((AssetPairSource)obj).ToString(), StringComparison.Ordinal);
        }
    }
}
