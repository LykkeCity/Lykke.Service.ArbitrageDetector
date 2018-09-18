using System;
using System.Diagnostics;

namespace Lykke.Service.ArbitrageDetector.Core.Domain
{
    public class AssetPair : IComparable
    {
        public string Base { get; }

        public string Quote { get; }

        public int Accuracy { get; }

        public int InvertedAccuracy { get; } 

        public string Name => Base + Quote;

        public AssetPair(string @base, string quote, int accuracy, int invertedAccuracy)
        {
            Debug.Assert(!string.IsNullOrWhiteSpace(@base));
            Debug.Assert(!string.IsNullOrWhiteSpace(quote));

            Base = @base;
            Quote = quote;
            Accuracy = accuracy;
            InvertedAccuracy = invertedAccuracy;
        }

        public AssetPair Invert()
        {
            return new AssetPair(Quote, Base, InvertedAccuracy, Accuracy);
        }

        public bool IsInverted(AssetPair assetPair)
        {
            return Base == assetPair.Quote && Quote == assetPair.Base;
        }

        public bool IsEqualOrInverted(AssetPair other)
        {
            return Equals(other) || IsInverted(other);
        }

        public bool HasCommonAsset(AssetPair other)
        {
            return Base == other.Base || Base == other.Quote || Quote == other.Base || Quote == other.Quote;
        }

        public bool ContainsAsset(string asset)
        {
            Debug.Assert(!string.IsNullOrWhiteSpace(asset));

            return Base == asset || Quote == asset;
        }

        public bool ContainsAssets(string one, string another)
        {
            Debug.Assert(!string.IsNullOrWhiteSpace(one));
            Debug.Assert(!string.IsNullOrWhiteSpace(another));

            return (Base == one && Quote == another) || (Base == another && Quote == one);
        }

        public string GetOtherAsset(string one)
        {
            Debug.Assert(!string.IsNullOrWhiteSpace(one));

            if (!ContainsAsset(one))
                return null;

            var result = string.Equals(Base, one, StringComparison.OrdinalIgnoreCase) ? Quote : Base;

            return result;
        }

        public override string ToString()
        {
            return Name;
        }

        #region Equals and GetHashCode

        /// <summary>
        /// Equals.
        /// </summary>
        public bool Equals(AssetPair other)
        {
            return string.Equals(Base, other.Base, StringComparison.InvariantCultureIgnoreCase)
                   && string.Equals(Quote, other.Quote, StringComparison.InvariantCultureIgnoreCase);
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
        public int CompareTo(AssetPair other)
        {
            if (other == null)
                throw new ArgumentOutOfRangeException(nameof(other));

            var baseComparison = string.Compare(Base, other.Base, StringComparison.InvariantCultureIgnoreCase);

            if (baseComparison != 0)
                return baseComparison;

            return string.Compare(Quote, other.Quote, StringComparison.InvariantCultureIgnoreCase);
        }

        #endregion
    }
}
