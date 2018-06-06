using System;
using System.Collections.Generic;
using Lykke.Service.ArbitrageDetector.Core.Domain;
using Xunit;

namespace Lykke.Service.ArbitrageDetector.Tests
{
    public class ArbitrageTests
    {
        [Fact]
        public void ArbitrageVolume_NoArbitrage_EmptyOrderBooks_Test()
        {
            const string exchangeName = "FE";
            const string assetPair = "BTCUSD";
            var timestamp = DateTime.UtcNow;

            var asks = new List<VolumePrice>();
            var bids = new List<VolumePrice>();

            var orderBook1 = new OrderBook(exchangeName, assetPair, bids, asks, timestamp);
            var orderBook2 = new OrderBook(exchangeName, assetPair, bids, asks, timestamp);

            var volume = Arbitrage.GetArbitrageVolume(orderBook1, orderBook2);
            Assert.Null(volume);
        }

        [Fact]
        public void ArbitrageVolume_NoArbitrage_TheSameOrderBook_Test()
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
            
            var asks = new List<VolumePrice>
            {
                new VolumePrice(9000, 10),
                new VolumePrice(8999.95m, 7), // <-
                new VolumePrice(8900.12345677m, 3) // <-
            };

            var bids = new List<VolumePrice>
            {
                new VolumePrice(9000, 9), // <-
                new VolumePrice(8900, 5)
            };

            var bidsOrderBook = new OrderBook(exchangeName, assetPair, bids, new List<VolumePrice>(), timestamp);
            var asksOrderBook = new OrderBook(exchangeName, assetPair, new List<VolumePrice>(), asks, timestamp);

            var volume = Arbitrage.GetArbitrageVolume(bidsOrderBook, asksOrderBook);
            Assert.Equal(9, volume);
        }

        [Fact]
        public void ArbitrageVolume_Complex1_Test()
        {
            // https://docs.google.com/spreadsheets/d/1plnbQSS-WP6ykTv8wIi_hbAhk_aSz_tllXFIE3jhFpU/edit#gid=0

            const string exchangeName = "FE";
            const string assetPair = "BTCUSD";
            var timestamp = DateTime.UtcNow;

            var asks = new List<VolumePrice>
            {
                new VolumePrice(1000, 10),
                new VolumePrice(950, 10),
                new VolumePrice(850, 10), // <-
                new VolumePrice(800, 10), // <-
                new VolumePrice(700, 10), // <-
                new VolumePrice(650, 10), // <-
                new VolumePrice(600, 10), // <-
                new VolumePrice(550, 1),  // <-
                new VolumePrice(500, 10)  // <-
            };

            var bids = new List<VolumePrice>
            {
                new VolumePrice(900, 5),   // <-
                new VolumePrice(750, 100), // <-
                new VolumePrice(550, 1)    // <-
            };

            var bidsOrderBook = new OrderBook(exchangeName, assetPair, bids, new List<VolumePrice>(), timestamp);
            var asksOrderBook = new OrderBook(exchangeName, assetPair, new List<VolumePrice>(), asks, timestamp);

            var volume = Arbitrage.GetArbitrageVolume(bidsOrderBook, asksOrderBook);
            Assert.Equal(41, volume);
        }

        [Fact]
        public void ArbitrageVolume_Complex2_Test()
        {
            // https://docs.google.com/spreadsheets/d/1plnbQSS-WP6ykTv8wIi_hbAhk_aSz_tllXFIE3jhFpU/edit#gid=2011486790
            const string exchangeName = "FE";
            const string assetPair = "BTCUSD";
            var timestamp = DateTime.UtcNow;

            var asks = new List<VolumePrice>
            {
                new VolumePrice(2.9m, 10),
                new VolumePrice(2.8m, 10),
                new VolumePrice(2.7m, 10),
                new VolumePrice(2.6m, 10),
                new VolumePrice(2.4m, 10),
                new VolumePrice(2.3m, 10),
                new VolumePrice(2.2m, 10),
                new VolumePrice(2.0m, 10),
                new VolumePrice(1.9m, 10),
                new VolumePrice(1.8m, 10),
                new VolumePrice(1.7m, 10),
                new VolumePrice(1.3m, 10),
                new VolumePrice(1.2m, 10),
                new VolumePrice(1.1m, 10),
            };

            var bids = new List<VolumePrice>
            {
                new VolumePrice(3.2m, 1),
                new VolumePrice(3.1m, 1),
                new VolumePrice(3m, 1),
                new VolumePrice(2.5m, 1),
                new VolumePrice(2.1m, 100),
                new VolumePrice(1.6m, 5),
                new VolumePrice(1.5m, 5),
                new VolumePrice(1.4m, 5)
            };

            var bidsOrderBook = new OrderBook(exchangeName, assetPair, bids, new List<VolumePrice>(), timestamp);
            var asksOrderBook = new OrderBook(exchangeName, assetPair, new List<VolumePrice>(), asks, timestamp);

            var volume = Arbitrage.GetArbitrageVolume(bidsOrderBook, asksOrderBook);
            Assert.Equal(70, volume);
        }
    }
}
