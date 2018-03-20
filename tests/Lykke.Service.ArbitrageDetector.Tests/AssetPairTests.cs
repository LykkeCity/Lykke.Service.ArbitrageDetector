using System;
using Lykke.Service.ArbitrageDetector.Core.Domain;
using Xunit;

namespace Lykke.Service.ArbitrageDetector.Tests
{
    public class AssetPairTests
    {
        [Fact]
        public void AssetPairConstructorTest()
        {
            const string @base = "BTC";
            const string quoting = "USD";

            var assetPair = new AssetPair(@base, quoting);

            Assert.Equal(@base, assetPair.Base);
            Assert.Equal(quoting, assetPair.Quoting);

            void Construct1() => new AssetPair(null, quoting);
            Assert.Throws<ArgumentException>((Action) Construct1);

            void Construct2() => new AssetPair("", quoting);
            Assert.Throws<ArgumentException>((Action)Construct2);

            void Construct3() => new AssetPair(@base, null);
            Assert.Throws<ArgumentException>((Action)Construct3);

            void Construct4() => new AssetPair(@base, "");
            Assert.Throws<ArgumentException>((Action)Construct4);
        }

        [Fact]
        public void AssetPairReverseTest()
        {
            const string @base = "BTC";
            const string quoting = "USD";

            var assetPair = new AssetPair(@base, quoting);

            var reversed = assetPair.Reverse();

            Assert.Equal(@base, reversed.Quoting);
            Assert.Equal(quoting, reversed.Base);
        }

        [Fact]
        public void AssetPairIsReversedTest()
        {
            const string @base = "BTC";
            const string quoting = "USD";

            var assetPair = new AssetPair(@base, quoting);
            var reversed = assetPair.Reverse();

            Assert.True(assetPair.IsReversed(reversed));
            Assert.True(reversed.IsReversed(assetPair));

            void IsReversed1() => assetPair.IsReversed(new AssetPair(null, quoting));
            Assert.Throws<ArgumentException>((Action)IsReversed1);

            void IsReversed2() => assetPair.IsReversed(new AssetPair("", quoting));
            Assert.Throws<ArgumentException>((Action)IsReversed2);

            void IsReversed3() => assetPair.IsReversed(new AssetPair(@base, null));
            Assert.Throws<ArgumentException>((Action)IsReversed3);

            void IsReversed4() => assetPair.IsReversed(new AssetPair(@base, ""));
            Assert.Throws<ArgumentException>((Action)IsReversed4);
        }

        [Fact]
        public void AssetPairIsEqualTest()
        {
            const string @base = "BTC";
            const string quoting = "USD";

            var assetPair = new AssetPair(@base, quoting);
            var equalAssetPair = new AssetPair(@base, quoting);

            Assert.True(assetPair.Equals(equalAssetPair));
            Assert.True(equalAssetPair.Equals(assetPair));

            void IsEqual1() => assetPair.Equals(new AssetPair(null, quoting));
            Assert.Throws<ArgumentException>((Action)IsEqual1);

            void IsEqual2() => assetPair.Equals(new AssetPair("", quoting));
            Assert.Throws<ArgumentException>((Action)IsEqual2);

            void IsEqual3() => assetPair.Equals(new AssetPair(@base, null));
            Assert.Throws<ArgumentException>((Action)IsEqual3);

            void IsEqual4() => assetPair.Equals(new AssetPair(@base, ""));
            Assert.Throws<ArgumentException>((Action)IsEqual4);
        }

        [Fact]
        public void AssetPairIsEqualOrReversedTest()
        {
            const string @base = "BTC";
            const string quoting = "USD";

            var assetPair = new AssetPair(@base, quoting);
            var equalAssetPair = new AssetPair(@base, quoting);
            var reversed = assetPair.Reverse();

            Assert.True(assetPair.IsEqualOrReversed(equalAssetPair));
            Assert.True(equalAssetPair.IsEqualOrReversed(assetPair));
            Assert.True(assetPair.IsEqualOrReversed(reversed));
            Assert.True(reversed.IsEqualOrReversed(assetPair));

            void IsEqualOrReversed1() => assetPair.IsEqualOrReversed(new AssetPair(null, quoting));
            Assert.Throws<ArgumentException>((Action)IsEqualOrReversed1);

            void IsEqualOrReversed2() => assetPair.IsEqualOrReversed(new AssetPair("", quoting));
            Assert.Throws<ArgumentException>((Action)IsEqualOrReversed2);

            void IsEqualOrReversed3() => assetPair.IsEqualOrReversed(new AssetPair(@base, null));
            Assert.Throws<ArgumentException>((Action)IsEqualOrReversed3);

            void IsEqualOrReversed4() => assetPair.IsEqualOrReversed(new AssetPair(@base, ""));
            Assert.Throws<ArgumentException>((Action)IsEqualOrReversed4);
        }

        [Fact]
        public void AssetPairHasCommonAssetTest()
        {
            const string @base = "BTC";
            const string quoting = "USD";
            const string third = "EUR";

            var assetPair = new AssetPair(@base, quoting);
            var assetPair2 = new AssetPair(@base, third);
            var assetPair3 = new AssetPair(third, @base);

            Assert.True(assetPair.HasCommonAsset(assetPair2));
            Assert.True(assetPair2.HasCommonAsset(assetPair));
            Assert.True(assetPair.HasCommonAsset(assetPair3));
            Assert.True(assetPair3.HasCommonAsset(assetPair));

            void HasCommonAsset1() => assetPair.HasCommonAsset(new AssetPair(null, quoting));
            Assert.Throws<ArgumentException>((Action)HasCommonAsset1);

            void HasCommonAsset2() => assetPair.HasCommonAsset(new AssetPair("", quoting));
            Assert.Throws<ArgumentException>((Action)HasCommonAsset2);

            void HasCommonAsset3() => assetPair.HasCommonAsset(new AssetPair(@base, null));
            Assert.Throws<ArgumentException>((Action)HasCommonAsset3);

            void HasCommonAsset4() => assetPair.HasCommonAsset(new AssetPair(@base, ""));
            Assert.Throws<ArgumentException>((Action)HasCommonAsset4);
        }

        [Fact]
        public void AssetPairContainsTest()
        {
            const string btc = "BTC";
            const string usd = "USD";
            const string eur = "EUR";

            var btcusd = new AssetPair(btc, usd);

            Assert.True(btcusd.ContainsAsset(btc));
            Assert.True(btcusd.ContainsAsset(usd));
            Assert.False(btcusd.ContainsAsset(eur));

            void ContainsAsset1() => btcusd.ContainsAsset(null);
            Assert.Throws<ArgumentException>((Action)ContainsAsset1);

            void ContainsAsset2() => btcusd.ContainsAsset("");
            Assert.Throws<ArgumentException>((Action)ContainsAsset2);
        }

        [Fact]
        public void AssetPairFromStringTest()
        {
            const string btcusd = "BTCUSD";
            const string btc = "btc";
            const string usd = "USD";

            var assetPair = AssetPair.FromString(btcusd, btc);
            var assetPair2 = AssetPair.FromString(btcusd, usd);

            Assert.True(assetPair.Equals(assetPair2));

            void FromString1() => AssetPair.FromString(null, usd);
            Assert.Throws<ArgumentException>((Action)FromString1);

            void FromString2() => AssetPair.FromString("", usd);
            Assert.Throws<ArgumentException>((Action)FromString2);

            void FromString3() => AssetPair.FromString(btcusd, null);
            Assert.Throws<ArgumentException>((Action)FromString3);

            void FromString4() => AssetPair.FromString(btcusd, "");
            Assert.Throws<ArgumentException>((Action)FromString4);

            void FromString5() => AssetPair.FromString(btcusd, "chf");
            Assert.Throws<ArgumentOutOfRangeException>((Action)FromString5);            
        }

        [Fact]
        public void AssetPairIsEmptyTest()
        {
            var assetPair = new AssetPair();

            Assert.True(assetPair.IsEmpty());
        }
    }
}
