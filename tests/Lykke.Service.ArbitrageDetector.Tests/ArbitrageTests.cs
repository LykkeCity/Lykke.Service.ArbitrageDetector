using System;
using System.Collections.Generic;
using Lykke.Service.ArbitrageDetector.Core.Domain;
using Xunit;

namespace Lykke.Service.ArbitrageDetector.Tests
{
    public class ArbitrageTests
    {
        [Fact]
        public void ArbitrageVolume_NoArbitrage_Test()
        {
            const string exchangeName = "FE";
            const string assetPair = "BTCUSD";
            var timestamp = DateTime.UtcNow;

            var asks = new List<VolumePrice>
            {
                new VolumePrice(9000, 10),
                new VolumePrice(8999.95m, 7),
                new VolumePrice(8900.12345677m, 3)
            };
            var bids = new List<VolumePrice>
            {
                new VolumePrice(8825, 9),
                new VolumePrice(8823, 5)
            };

            var orderBook1 = new OrderBook(exchangeName, assetPair, bids, asks, timestamp);
            var orderBook2 = new OrderBook(exchangeName, assetPair, bids, asks, timestamp);

            var volume = Arbitrage.GetArbitrageVolume(orderBook1, orderBook2);
            Assert.Null(volume);
        }

        [Fact]
        public void ArbitrageVolume_Simple1_Test()
        {
            const string exchangeName = "FE";
            const string assetPair = "BTCUSD";
            var timestamp = DateTime.UtcNow;

            var asks1 = new List<VolumePrice>
            {
                new VolumePrice(9000, 10),
                new VolumePrice(8999.95m, 7), // <-
                new VolumePrice(8900.12345677m, 3) // <-
            };
            var bids1 = new List<VolumePrice>
            {
                new VolumePrice(8825, 9),
                new VolumePrice(8823, 5)
            };

            var asks2 = new List<VolumePrice>
            {
                new VolumePrice(9200, 10),
                new VolumePrice(9100, 10)
            };
            var bids2 = new List<VolumePrice>
            {
                new VolumePrice(9000, 9), // <-
                new VolumePrice(8900, 5)
            };

            var orderBook1 = new OrderBook(exchangeName, assetPair, bids1, asks1, timestamp);
            var orderBook2 = new OrderBook(exchangeName, assetPair, bids2, asks2, timestamp);

            var volume = Arbitrage.GetArbitrageVolume(orderBook1, orderBook2);
            Assert.Equal(9, volume);
        }
    }
}
