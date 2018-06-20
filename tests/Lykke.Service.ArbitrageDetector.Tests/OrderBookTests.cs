using System;
using System.Collections.Generic;
using System.Linq;
using Lykke.Service.ArbitrageDetector.Core.Domain;
using Xunit;

namespace Lykke.Service.ArbitrageDetector.Tests
{
    public class OrderBookTests
    {
        [Fact]
        public void ConstructorTest()
        {
            const string exchangeName = "FakeExchange";
            const string assetPair = "BTCUSD";
            var timestamp = DateTime.UtcNow;

            var bids = new List<VolumePrice>
            {
                new VolumePrice(8825, 9),
                new VolumePrice(8823, 5)
            };
            var asks = new List<VolumePrice>
            {
                new VolumePrice(9000, 10),
                new VolumePrice(8999.95m, 7),
                new VolumePrice(8900.12345677m, 3)
            };

            void Construct1() => new OrderBook("", assetPair, bids, asks, timestamp);
            Assert.Throws<ArgumentException>((Action)Construct1);

            void Construct2() => new OrderBook(null, assetPair, bids, asks, timestamp);
            Assert.Throws<ArgumentException>((Action)Construct2);

            void Construct3() => new OrderBook(exchangeName, "", bids, asks, timestamp);
            Assert.Throws<ArgumentException>((Action)Construct3);

            void Construct4() => new OrderBook(exchangeName, null, bids, asks, timestamp);
            Assert.Throws<ArgumentException>((Action)Construct4);
        }

        [Fact]
        public void SetAssetPairTest()
        {
            const string exchangeName = "FakeExchange";
            const string assetPair = "BTCUSD";
            var timestamp = DateTime.UtcNow;

            var orderBook = new OrderBook(exchangeName, assetPair,
                new List<VolumePrice> // bids
                {
                    new VolumePrice(8825, 9), new VolumePrice(8823, 5)
                },
                new List<VolumePrice> // asks
                {
                    new VolumePrice(9000, 10), new VolumePrice(8999.95m, 7), new VolumePrice(8900.12345677m, 3)
                },
                timestamp);

            Assert.Null(orderBook.AssetPair.Base);
            Assert.Null(orderBook.AssetPair.Quote);
            orderBook.SetAssetPair("USD");
            Assert.Equal("BTC", orderBook.AssetPair.Base);
            Assert.Equal("USD", orderBook.AssetPair.Quote);
            Assert.Equal(assetPair, orderBook.AssetPair.Name);
        }

        [Fact]
        public void ReverseTest()
        {
            const string exchangeName = "FakeExchange";
            const string assetPair = "BTCUSD";
            const string reversedPair = "USDBTC";
            var timestamp = DateTime.UtcNow;

            var orderBook = new OrderBook(exchangeName, assetPair,
                new List<VolumePrice> // bids
                {
                    new VolumePrice(8825, 9), new VolumePrice(8823, 5)
                },
                new List<VolumePrice> // asks
                {
                    new VolumePrice(9000, 10), new VolumePrice(8999.95m, 7), new VolumePrice(8900.12345677m, 3)
                },
                timestamp);
            orderBook.SetAssetPair("USD");

            var reversed = orderBook.Reverse();
            Assert.NotNull(reversed);
            Assert.Equal(exchangeName, reversed.Source);
            Assert.Equal(reversedPair, reversed.AssetPairStr);
            Assert.Equal(orderBook.Bids.Count, reversed.Asks.Count);
            Assert.Equal(orderBook.Asks.Count, reversed.Bids.Count);

            var bidVolumePrice1 = reversed.Bids.Single(x => x.Volume == 26700.37037031m);
            var bidVolumePrice2 = reversed.Bids.Single(x => x.Volume == 62999.65m);
            var bidVolumePrice3 = reversed.Bids.Single(x => x.Volume == 90000);
            Assert.Equal(bidVolumePrice1.Price, 1 / 8900.12345677m, 8);
            Assert.Equal(bidVolumePrice2.Price, 1 / 8999.95m, 8);
            Assert.Equal(bidVolumePrice3.Price, 1 / 9000m, 8);

            var askVolumePrice1 = reversed.Asks.Single(x => x.Volume == 79425);
            var askVolumePrice2 = reversed.Asks.Single(x => x.Volume == 44115);
            Assert.Equal(askVolumePrice1.Price, 1 / 8825m, 8);
            Assert.Equal(askVolumePrice2.Price, 1 / 8823m, 8);

            Assert.Equal(timestamp, reversed.Timestamp);
        }
    }
}
