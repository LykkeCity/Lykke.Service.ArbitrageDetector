using System;
using System.Collections.Generic;

namespace Lykke.Service.ArbitrageDetector.Client.Models
{
    public class CrossRate : OrderBook
    {
        public string ConversionPath { get; }

        public IList<OrderBook> OriginalOrderBooks { get; }

        public CrossRate(string source, AssetPair assetPair, IReadOnlyCollection<VolumePrice> asks, IReadOnlyCollection<VolumePrice> bids,
            string conversionPath, IList<OrderBook> originalOrderBooks, DateTime timestamp)
            : base(source, assetPair.Base + assetPair.Quoting, asks, bids, DateTime.MinValue)
        {
            if (assetPair.IsEmpty())
                throw new ArgumentOutOfRangeException($"{nameof(assetPair)}. Base: {assetPair.Base}, Quoting: {assetPair.Quoting}.");

            AssetPair = assetPair;

            ConversionPath = string.IsNullOrEmpty(conversionPath)
                ? throw new ArgumentException(nameof(conversionPath))
                : conversionPath;
            OriginalOrderBooks = originalOrderBooks ?? throw new ArgumentNullException(nameof(originalOrderBooks));

            Timestamp = timestamp;
        }

        public static CrossRate FromOrderBook(OrderBook orderBook, AssetPair targetAssetPair)
        {
            if (orderBook == null)
                throw new ArgumentNullException(nameof(orderBook));

            if (string.IsNullOrWhiteSpace(targetAssetPair.Base))
                throw new ArgumentException(nameof(targetAssetPair.Base));

            if (string.IsNullOrWhiteSpace(targetAssetPair.Quoting))
                throw new ArgumentException(nameof(targetAssetPair.Quoting));

            if (orderBook.AssetPair.IsEmpty())
                throw new ArgumentException(nameof(orderBook.AssetPair));

            if (!targetAssetPair.IsEqualOrReversed(orderBook.AssetPair))
                throw new ArgumentOutOfRangeException($"{nameof(orderBook.AssetPair)} and {nameof(targetAssetPair)} aren't semantically equal.");

            var originalOrderBooks = new List<OrderBook>();
            OrderBook orderBookResult = null;
            string conversionPath = null;
            // Streight
            if (orderBook.AssetPair.Base == targetAssetPair.Base && orderBook.AssetPair.Quoting == targetAssetPair.Quoting)
            {
                conversionPath = $"{orderBook.Source}-{orderBook.AssetPairStr}";
                orderBookResult = orderBook;
            }

            // Reversed
            if (orderBook.AssetPair.Base == targetAssetPair.Quoting && orderBook.AssetPair.Quoting == targetAssetPair.Base)
            {
                conversionPath = $"{orderBook.Source}-{orderBook.AssetPairStr}";
                orderBookResult = orderBook.Reverse();
                originalOrderBooks.Add(orderBook);
            }
            
            var result = new CrossRate(orderBookResult.Source, targetAssetPair, orderBookResult.Asks, orderBookResult.Bids, conversionPath, originalOrderBooks, orderBook.Timestamp);

            return result;
        }

        public static CrossRate FromOrderBooks(OrderBook one, OrderBook another, AssetPair targetAssetPair)
        {
            if (one == null)
                throw new ArgumentNullException(nameof(one));

            if (another == null)
                throw new ArgumentNullException(nameof(another));

            if (string.IsNullOrWhiteSpace(targetAssetPair.Base))
                throw new ArgumentException(nameof(targetAssetPair.Base));

            if (string.IsNullOrWhiteSpace(targetAssetPair.Quoting))
                throw new ArgumentException(nameof(targetAssetPair.Quoting));

            if (!targetAssetPair.HasCommonAsset(one.AssetPair))
                throw new ArgumentOutOfRangeException($"{nameof(one)} and {nameof(targetAssetPair)} don't have common asset pair.");

            if (!targetAssetPair.HasCommonAsset(another.AssetPair))
                throw new ArgumentOutOfRangeException($"{nameof(another)} and {nameof(targetAssetPair)} don't have common asset pair.");

            if (one.AssetPair.IsEqualOrReversed(another.AssetPair))
                throw new ArgumentOutOfRangeException($"{nameof(one)} and {nameof(another)} are semantically equal.");

            if (!one.AssetPair.HasCommonAsset(another.AssetPair))
                throw new ArgumentOutOfRangeException($"{nameof(one)} and {nameof(another)} don't have common asset.");

            // Find left and right order book
            OrderBook left = null;
            OrderBook right = null;

            if (targetAssetPair.Base == one.AssetPair.Base)
                left = one;

            if (targetAssetPair.Base == one.AssetPair.Quoting)
                left = one.Reverse();

            if (targetAssetPair.Base == another.AssetPair.Base)
                left = another;

            if (targetAssetPair.Base == another.AssetPair.Quoting)
                left = another.Reverse();

            if (targetAssetPair.Quoting == one.AssetPair.Base)
                right = one.Reverse();

            if (targetAssetPair.Quoting == one.AssetPair.Quoting)
                right = one;

            if (targetAssetPair.Quoting == another.AssetPair.Base)
                right = another.Reverse();

            if (targetAssetPair.Quoting == another.AssetPair.Quoting)
                right = another;

            if (left == null)
                throw new ArgumentNullException(nameof(left));

            if (right == null)
                throw new ArgumentNullException(nameof(right));

            if (left.AssetPair.Base != targetAssetPair.Base
                || right.AssetPair.Quoting != targetAssetPair.Quoting
                || left.AssetPair.Quoting != right.AssetPair.Base)
                throw new InvalidOperationException($"{nameof(left)} and {nameof(right)} don't correspond to {nameof(targetAssetPair)}");

            var asks = new List<VolumePrice>();
            var bids = new List<VolumePrice>();

            foreach (var leftAsk in left.Asks)
            {
                foreach (var rightAsk in right.Asks)
                {
                    var newVolumePrice = new VolumePrice(leftAsk.Price * rightAsk.Price,
                        Math.Min(leftAsk.Volume, rightAsk.Volume));

                    asks.Add(newVolumePrice);
                }
            }

            foreach (var leftBid in left.Bids)
            {
                foreach (var rightBid in right.Bids)
                {
                    var newVolumePrice = new VolumePrice(leftBid.Price * rightBid.Price,
                        Math.Min(leftBid.Volume, rightBid.Volume));

                    bids.Add(newVolumePrice);
                }
            }

            var source = $"{one.Source}-{another.Source}";
            var conversionPath = $"{one.Source}-{one.AssetPairStr} & {another.Source}-{another.AssetPairStr}";
            var originalOrderBooks = new List<OrderBook> { one, another };
            var timestamp = left.Timestamp < right.Timestamp ? left.Timestamp : right.Timestamp;

            var result = new CrossRate(source, targetAssetPair, asks, bids, conversionPath, originalOrderBooks, timestamp);

            return result;
        }
    }
}
