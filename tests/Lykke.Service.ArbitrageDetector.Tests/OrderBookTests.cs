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
        public void ReverseTest()
        {
            const string exchangeName = "FakeExchange";
            var assetPair = new AssetPair("BTC", "USD");
            var reversedPair = new AssetPair("USD", "BTC");
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

            var reversed = orderBook.Reverse();
            Assert.NotNull(reversed);
            Assert.Equal(exchangeName, reversed.Source);
            Assert.Equal(reversedPair.Name, reversed.AssetPair.Name);
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
