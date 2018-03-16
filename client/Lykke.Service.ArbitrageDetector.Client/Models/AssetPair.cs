using System;

namespace Lykke.Service.ArbitrageDetector.Client.Models
{
    public struct AssetPair
    {
        public string Base { get; }

        public string Quoting { get; }

        public string Name => Base + Quoting;

        public AssetPair(string @base, string quoting)
        {
            Base = string.IsNullOrWhiteSpace(@base) ? throw new ArgumentException(nameof(@base)) : @base;
            Quoting = string.IsNullOrWhiteSpace(quoting) ? throw new ArgumentException(nameof(quoting)) : quoting;
        }

        public AssetPair Reverse()
        {
            return new AssetPair(Quoting, Base);
        }

        public bool IsReversed(AssetPair assetPair)
        {
            if (assetPair.IsEmpty())
                throw new ArgumentException($"{nameof(assetPair)} is not filled properly.");

            return Base == assetPair.Quoting && Quoting == assetPair.Base;
        }

        public bool IsEqual(AssetPair assetPair)
        {
            if (assetPair.IsEmpty())
                throw new ArgumentException($"{nameof(assetPair)} is not filled properly.");

            return Base == assetPair.Base && Quoting == assetPair.Quoting;
        }

        public bool IsEqualOrReversed(AssetPair assetPair)
        {
            if (assetPair.IsEmpty())
                throw new ArgumentException($"{nameof(assetPair)} is not filled properly.");

            return IsEqual(assetPair) || IsReversed(assetPair);
        }

        public bool HasCommonAsset(AssetPair assetPair)
        {
            if (assetPair.IsEmpty())
                throw new ArgumentException($"{nameof(assetPair)} is not filled properly.");

            return Base == assetPair.Base || Base == assetPair.Quoting || Quoting == assetPair.Base || Quoting == assetPair.Quoting;
        }

        public static AssetPair FromString(string assetPair, string oneOfTheAssets)
        {
            if (string.IsNullOrWhiteSpace(assetPair))
                throw new ArgumentException(nameof(assetPair));

            if (string.IsNullOrWhiteSpace(oneOfTheAssets))
                throw new ArgumentException(nameof(oneOfTheAssets));

            oneOfTheAssets = oneOfTheAssets.ToUpper().Trim();
            assetPair = assetPair.ToUpper().Trim();

            if (!assetPair.Contains(oneOfTheAssets))
                throw new ArgumentOutOfRangeException($"{nameof(assetPair)} doesn't contain {nameof(oneOfTheAssets)}");

            var otherAsset = assetPair.ToUpper().Trim().Replace(oneOfTheAssets, string.Empty);

            var baseAsset = assetPair.StartsWith(oneOfTheAssets) ? oneOfTheAssets : otherAsset;
            var quotingAsset = assetPair.Replace(baseAsset, string.Empty);

            var result = new AssetPair(baseAsset, quotingAsset);

            return result;
        }

        public bool IsEmpty()
        {
            return string.IsNullOrWhiteSpace(Base) || string.IsNullOrWhiteSpace(Quoting);
        }

        public override string ToString()
        {
            return Name;
        }
    }
}
