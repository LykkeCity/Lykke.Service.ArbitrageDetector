using System;

namespace Lykke.Service.ArbitrageDetector.Core.Domain
{
    /// <summary>
    /// represents an asset pair aka instrument.
    /// </summary>
    public struct AssetPair
    {
        /// <summary>
        /// Base asset.
        /// </summary>
        public string Base { get; }

        /// <summary>
        /// Quoting asset.
        /// </summary>
        public string Quoting { get; }

        /// <summary>
        /// Name of the asset pair.
        /// </summary>
        public string Name => Base + Quoting;

        /// <summary>
        /// Contructor.
        /// </summary>
        /// <param name="base"></param>
        /// <param name="quoting"></param>
        public AssetPair(string @base, string quoting)
        {
            Base = string.IsNullOrWhiteSpace(@base) ? throw new ArgumentException(nameof(@base)) : @base;
            Quoting = string.IsNullOrWhiteSpace(quoting) ? throw new ArgumentException(nameof(quoting)) : quoting;
        }

        /// <summary>
        /// Returns reversed asset pair.
        /// </summary>
        /// <returns></returns>
        public AssetPair Reverse()
        {
            Validate();

            return new AssetPair(Quoting, Base);
        }

        /// <summary>
        /// Checks if assset pair is revered.
        /// </summary>
        /// <param name="assetPair"></param>
        /// <returns></returns>
        public bool IsReversed(AssetPair assetPair)
        {
            Validate();

            if (assetPair.IsEmpty())
                throw new ArgumentException($"{nameof(assetPair)} is not filled properly.");

            return Base == assetPair.Quoting && Quoting == assetPair.Base;
        }

        /// <summary>
        /// Checks if asset pairs are equal.
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public bool Equals(AssetPair other)
        {
            Validate();

            if (other.IsEmpty())
                throw new ArgumentException($"{nameof(other)} is not filled properly.");

            return Base == other.Base && Quoting == other.Quoting;
        }

        /// <summary>
        /// Checks if equal or reversed.
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public bool IsEqualOrReversed(AssetPair other)
        {
            Validate();

            if (other.IsEmpty())
                throw new ArgumentException($"{nameof(other)} is not filled properly.");

            return Equals(other) || IsReversed(other);
        }

        /// <summary>
        /// Check if has common asset.
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public bool HasCommonAsset(AssetPair other)
        {
            Validate();

            if (other.IsEmpty())
                throw new ArgumentException($"{nameof(other)} is not filled properly.");

            return Base == other.Base || Base == other.Quoting || Quoting == other.Base || Quoting == other.Quoting;
        }

        /// <summary>
        /// Checks if contains asset.
        /// </summary>
        /// <param name="asset"></param>
        /// <returns></returns>
        public bool ContainsAsset(string asset)
        {
            Validate();

            if (string.IsNullOrWhiteSpace(asset))
                throw new ArgumentException(nameof(asset));

            return Base == asset || Quoting == asset;
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

        /// <summary>
        /// Checks if not initialized.
        /// </summary>
        /// <returns></returns>
        public bool IsEmpty()
        {
            return string.IsNullOrWhiteSpace(Base) || string.IsNullOrWhiteSpace(Quoting);
        }

        public override string ToString()
        {
            Validate();

            return Name;
        }

        /// <summary>
        /// Throw ArgumentException if not initialized.
        /// </summary>
        private void Validate()
        {
            if (string.IsNullOrWhiteSpace(Base))
                throw new ArgumentException(nameof(Base));

            if (string.IsNullOrWhiteSpace(Quoting))
                throw new ArgumentException(nameof(Quoting));
        }
    }
}
