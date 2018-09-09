using System;
using Lykke.Service.ArbitrageDetector.Core.Domain;
using Xunit;

namespace Lykke.Service.ArbitrageDetector.Tests
{
    public class AssetPairTests
    {
        private const string @base = "BTC";
        private const string quote = "USD";
        private AssetPair assetPair = new AssetPair(@base, quote, 3, 5);

        [Fact]
        public void AssetPairConstructorTest()
        {
            Assert.Equal(@base, assetPair.Base);
            Assert.Equal(quote, assetPair.Quote);
        }

        [Fact]
        public void AssetPairReverseTest()
        {
            var reversed = assetPair.Invert();

            Assert.Equal(@base, reversed.Quote);
            Assert.Equal(quote, reversed.Base);
            Assert.Equal(assetPair.Accuracy, reversed.InvertedAccuracy);
            Assert.Equal(assetPair.InvertedAccuracy, reversed.Accuracy);
        }

        [Fact]
        public void AssetPairIsReversedTest()
        {
            var reversed = assetPair.Invert();

            Assert.True(assetPair.IsInverted(reversed));
            Assert.True(reversed.IsInverted(assetPair));
        }

        [Fact]
        public void AssetPairIsEqualTest()
        {
            var equalAssetPair = new AssetPair(@base, quote, 3, 5);

            Assert.True(assetPair.Equals(equalAssetPair));
            Assert.True(equalAssetPair.Equals(assetPair));
        }

        [Fact]
        public void AssetPairIsEqualOrReversedTest()
        {
            var equalAssetPair = new AssetPair(@base, quote, 3, 5);
            var reversed = assetPair.Invert();

            Assert.True(assetPair.IsEqualOrInverted(equalAssetPair));
            Assert.True(equalAssetPair.IsEqualOrInverted(assetPair));
            Assert.True(assetPair.IsEqualOrInverted(reversed));
            Assert.True(reversed.IsEqualOrInverted(assetPair));
        }

        [Fact]
        public void AssetPairHasCommonAssetTest()
        {
            const string third = "EUR";

            var assetPair2 = new AssetPair(@base, third, 3, 5);
            var assetPair3 = new AssetPair(third, @base, 2, 4);

            Assert.True(assetPair.HasCommonAsset(assetPair2));
            Assert.True(assetPair2.HasCommonAsset(assetPair));
            Assert.True(assetPair.HasCommonAsset(assetPair3));
            Assert.True(assetPair3.HasCommonAsset(assetPair));
        }

        [Fact]
        public void AssetPairContainsTest()
        {
            const string btc = "BTC";
            const string usd = "USD";
            const string eur = "EUR";

            var btcusd = new AssetPair(btc, usd, 3, 5);

            Assert.True(btcusd.ContainsAsset(btc));
            Assert.True(btcusd.ContainsAsset(usd));
            Assert.False(btcusd.ContainsAsset(eur));
        }
    }
}
