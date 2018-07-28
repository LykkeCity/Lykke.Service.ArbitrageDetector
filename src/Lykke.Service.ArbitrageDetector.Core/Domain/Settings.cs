using System;
using System.Collections.Generic;
using System.Linq;

namespace Lykke.Service.ArbitrageDetector.Core.Domain
{
    public class Settings
    {
        // Common

        public int ExpirationTimeInSeconds { get; set; } = 10;

        // Arbitrages, Synthetic

        public int HistoryMaxSize { get; set; } = 50;

        public decimal MinimumPnL { get; set; } = 0;

        public decimal MinimumVolume { get; set; } = 0m;

        public int MinSpread { get; set; } = 0;

        public IEnumerable<string> BaseAssets { get; set; } = new List<string>();

        public string QuoteAsset { get; set; } = "USD";

        public IEnumerable<string> IntermediateAssets { get; set; } = new List<string>();

        public IEnumerable<string> Exchanges { get; set; }  = new List<string>();

        // Matrix

        public IEnumerable<string> MatrixAssetPairs { get; set; } = new List<string>();

        public decimal? MatrixSignificantSpread { get; set; } = -1;

        // Public Matrix

        public IEnumerable<string> PublicMatrixAssetPairs { get; set; } = new List<string>();

        public IDictionary<string, string> PublicMatrixExchanges { get; set; } = new Dictionary<string, string>();

        // Matrix History

        public TimeSpan MatrixHistoryInterval { get; set; } = new TimeSpan(0, 0, 5, 0);

        public IEnumerable<string> MatrixHistoryAssetPairs { get; set; } = new List<string>();

        public string MatrixHistoryLykkeName { get; set; } = "lykke";

        // Other

        public IEnumerable<ExchangeFees> ExchangesFees { get; set; } = new List<ExchangeFees>();

        #region Equals

        protected bool Equals(Settings other)
        {
            return ExpirationTimeInSeconds == other.ExpirationTimeInSeconds &&
                   HistoryMaxSize == other.HistoryMaxSize &&
                   MinimumPnL == other.MinimumPnL &&
                   MinimumVolume == other.MinimumVolume &&
                   MinSpread == other.MinSpread &&
                   BaseAssets.SequenceEqual(other.BaseAssets) &&
                   string.Equals(QuoteAsset, other.QuoteAsset) &&
                   IntermediateAssets.SequenceEqual(other.IntermediateAssets) &&
                   Exchanges.SequenceEqual(other.Exchanges) &&
                   MatrixAssetPairs.SequenceEqual(other.MatrixAssetPairs) &&
                   MatrixSignificantSpread == other.MatrixSignificantSpread &&
                   PublicMatrixAssetPairs.SequenceEqual(other.PublicMatrixAssetPairs) &&
                   PublicMatrixExchanges.SequenceEqual(other.PublicMatrixExchanges) &&
                   MatrixHistoryInterval.Equals(other.MatrixHistoryInterval) &&
                   MatrixHistoryAssetPairs.SequenceEqual(other.MatrixHistoryAssetPairs) &&
                   string.Equals(MatrixHistoryLykkeName, other.MatrixHistoryLykkeName) &&
                   ExchangesFees.SequenceEqual(other.ExchangesFees);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((Settings) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = ExpirationTimeInSeconds;
                hashCode = (hashCode * 397) ^ HistoryMaxSize;
                hashCode = (hashCode * 397) ^ MinimumPnL.GetHashCode();
                hashCode = (hashCode * 397) ^ MinimumVolume.GetHashCode();
                hashCode = (hashCode * 397) ^ MinSpread;
                hashCode = (hashCode * 397) ^ (BaseAssets != null ? BaseAssets.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (QuoteAsset != null ? QuoteAsset.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (IntermediateAssets != null ? IntermediateAssets.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (Exchanges != null ? Exchanges.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (MatrixAssetPairs != null ? MatrixAssetPairs.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ MatrixSignificantSpread.GetHashCode();
                hashCode = (hashCode * 397) ^ (PublicMatrixAssetPairs != null ? PublicMatrixAssetPairs.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (PublicMatrixExchanges != null ? PublicMatrixExchanges.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ MatrixHistoryInterval.GetHashCode();
                hashCode = (hashCode * 397) ^ (MatrixHistoryAssetPairs != null ? MatrixHistoryAssetPairs.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (MatrixHistoryLykkeName != null ? MatrixHistoryLykkeName.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (ExchangesFees != null ? ExchangesFees.GetHashCode() : 0);
                return hashCode;
            }
        }

        #endregion
    }
}
