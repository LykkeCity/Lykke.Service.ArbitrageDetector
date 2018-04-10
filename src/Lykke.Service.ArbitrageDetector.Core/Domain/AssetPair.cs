using System;

namespace Lykke.Service.ArbitrageDetector.Core.Domain
{
    /// <summary>
    /// represents an asset pair aka instrument.
    /// </summary>
    public struct AssetPair : IComparable
    {
        /// <summary>
        /// Base asset.
        /// </summary>
        public string Base { get; }

        /// <summary>
        /// Quote asset.
        /// </summary>
        public string Quote { get; }

        /// <summary>
        /// Name of the asset pair.
        /// </summary>
        public string Name => Base + Quote;

        /// <summary>
        /// Contructor.
        /// </summary>
        /// <param name="base"></param>
        /// <param name="quote"></param>
        public AssetPair(string @base, string quote)
        {
            Base = string.IsNullOrWhiteSpace(@base) ? throw new ArgumentException(nameof(@base)) : @base;
            Quote = string.IsNullOrWhiteSpace(quote) ? throw new ArgumentException(nameof(quote)) : quote;
        }

        /// <summary>
        /// Returns reversed asset pair.
        /// </summary>
        /// <returns></returns>
        public AssetPair Reverse()
        {
            Validate();

            return new AssetPair(Quote, Base);
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

            return Base == assetPair.Quote && Quote == assetPair.Base;
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

            return Base == other.Base || Base == other.Quote || Quote == other.Base || Quote == other.Quote;
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

            return Base == asset || Quote == asset;
        }

        /// <summary>
        /// Create Asset Pair from string with one of the assets.
        /// </summary>
        /// <param name="assetPair"></param>
        /// <param name="oneOfTheAssets"></param>
        /// <returns></returns>
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
            var quoteAsset = assetPair.Replace(baseAsset, string.Empty);

            var result = new AssetPair(baseAsset, quoteAsset);

            return result;
        }

        /// <summary>
        /// Checks if not initialized.
        /// </summary>
        /// <returns></returns>
        public bool IsEmpty()
        {
            return string.IsNullOrWhiteSpace(Base) || string.IsNullOrWhiteSpace(Quote);
        }

        /// <summary>
        /// Throw ArgumentException if not initialized.
        /// </summary>
        private void Validate()
        {
            if (string.IsNullOrWhiteSpace(Base))
                throw new ArgumentException(nameof(Base));

            if (string.IsNullOrWhiteSpace(Quote))
                throw new ArgumentException(nameof(Quote));
        }

        /// <inheritdoc />
        public override string ToString()
        {
            Validate();

            return Name;
        }

        #region Equals and GetHashCode

        /// <summary>
        /// Equals.
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public bool Equals(AssetPair other)
        {
            return string.Equals(Base, other.Base) && string.Equals(Quote, other.Quote);
        }

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
                return false;

            if (!(obj is AssetPair other))
                throw new InvalidCastException(nameof(obj));

            return Equals(other);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            unchecked
            {
                return ((Base != null ? Base.GetHashCode() : 0) * 397) ^ (Quote != null ? Quote.GetHashCode() : 0);
            }
        }

        #endregion

        #region IComparable

        /// <inheritdoc />
        public int CompareTo(object obj)
        {
            if (obj == null)
                throw new ArgumentNullException(nameof(obj));

            if (!(obj is AssetPair other))
                throw new InvalidCastException(nameof(obj));

            return CompareTo(other);
        }

        /// <summary>
        /// CompareTo.
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public int CompareTo(AssetPair other)
        {
            if (default(AssetPair).Equals(other) )
                throw new ArgumentOutOfRangeException(nameof(other));

            var baseComparison = string.Compare(Base, other.Base, StringComparison.Ordinal);

            if (baseComparison != 0)
                return baseComparison;

            return string.Compare(Quote, other.Quote, StringComparison.Ordinal);
        }

        #endregion
    }
}
