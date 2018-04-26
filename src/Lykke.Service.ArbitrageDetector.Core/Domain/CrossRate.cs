﻿using System;
using System.Collections.Generic;

namespace Lykke.Service.ArbitrageDetector.Core.Domain
{
    /// <summary>
    /// Represents a cross rate.
    /// </summary>
    public class CrossRate : OrderBook
    {
        /// <summary>
        /// Conversion path.
        /// </summary>
        public string ConversionPath { get; }

        /// <summary>
        /// Original order books.
        /// </summary>
        public IList<OrderBook> OriginalOrderBooks { get; }

        /// <inheritdoc />
        /// <summary>
        /// Contructor.
        /// </summary>
        /// <param name="source"></param>
        /// <param name="assetPair"></param>
        /// <param name="bids"></param>
        /// <param name="asks"></param>
        /// <param name="conversionPath"></param>
        /// <param name="originalOrderBooks"></param>
        /// <param name="timestamp"></param>
        public CrossRate(string source, AssetPair assetPair,
            IReadOnlyCollection<VolumePrice> bids, IReadOnlyCollection<VolumePrice> asks,
            string conversionPath, IList<OrderBook> originalOrderBooks, DateTime timestamp)
            : base(source, assetPair.Name, bids, asks, timestamp)
        {
            if (assetPair.IsEmpty())
                throw new ArgumentOutOfRangeException($"{nameof(assetPair)}. Base: {assetPair.Base}, Quote: {assetPair.Quote}.");

            AssetPair = assetPair;

            ConversionPath = string.IsNullOrEmpty(conversionPath)
                ? throw new ArgumentException(nameof(conversionPath))
                : conversionPath;

            OriginalOrderBooks = originalOrderBooks ?? throw new ArgumentNullException(nameof(originalOrderBooks));
        }

        /// <summary>
        /// From one order book if equal or reversed.
        /// </summary>
        /// <param name="orderBook"></param>
        /// <param name="targetAssetPair"></param>
        /// <returns></returns>
        public static CrossRate FromOrderBook(OrderBook orderBook, AssetPair targetAssetPair)
        {
            #region Checking arguments

            if (orderBook == null)
                throw new ArgumentNullException(nameof(orderBook));

            if (string.IsNullOrWhiteSpace(targetAssetPair.Base))
                throw new ArgumentException(nameof(targetAssetPair.Base));

            if (string.IsNullOrWhiteSpace(targetAssetPair.Quote))
                throw new ArgumentException(nameof(targetAssetPair.Quote));

            if (orderBook.AssetPair.IsEmpty())
                throw new ArgumentException(nameof(orderBook.AssetPair));

            if (!targetAssetPair.IsEqualOrReversed(orderBook.AssetPair))
                throw new ArgumentOutOfRangeException($"{nameof(orderBook.AssetPair)} and {nameof(targetAssetPair)} aren't semantically equal.");

            #endregion

            var originalOrderBooks = new List<OrderBook>();
            OrderBook orderBookResult = null;
            var conversionPath = orderBook.ToString();
            // Streight
            if (orderBook.AssetPair.Base == targetAssetPair.Base && orderBook.AssetPair.Quote == targetAssetPair.Quote)
            {
                orderBookResult = orderBook;
                originalOrderBooks.Add(orderBook);
            }

            // Reversed
            if (orderBook.AssetPair.Base == targetAssetPair.Quote && orderBook.AssetPair.Quote == targetAssetPair.Base)
            {
                orderBookResult = orderBook.Reverse();
                originalOrderBooks.Add(orderBook);
            }
            
            if (orderBookResult == null)
                throw new InvalidOperationException("AssetPairs must be the same or reversed)");

            var result = new CrossRate(orderBookResult.Source, targetAssetPair, orderBookResult.Bids, orderBookResult.Asks, conversionPath, originalOrderBooks, orderBook.Timestamp);

            return result;
        }

        /// <summary>
        /// From two order books.
        /// </summary>
        /// <param name="one"></param>
        /// <param name="another"></param>
        /// <param name="targetAssetPair"></param>
        /// <returns></returns>
        public static CrossRate FromOrderBooks(OrderBook one, OrderBook another, AssetPair targetAssetPair)
        {
            #region Checking arguments 

            if (one == null)
                throw new ArgumentNullException(nameof(one));

            if (another == null)
                throw new ArgumentNullException(nameof(another));

            if (string.IsNullOrWhiteSpace(targetAssetPair.Base))
                throw new ArgumentException(nameof(targetAssetPair.Base));

            if (string.IsNullOrWhiteSpace(targetAssetPair.Quote))
                throw new ArgumentException(nameof(targetAssetPair.Quote));

            if (!targetAssetPair.HasCommonAsset(one.AssetPair))
                throw new ArgumentOutOfRangeException($"{nameof(one)} and {nameof(targetAssetPair)} don't have common asset pair.");

            if (!targetAssetPair.HasCommonAsset(another.AssetPair))
                throw new ArgumentOutOfRangeException($"{nameof(another)} and {nameof(targetAssetPair)} don't have common asset pair.");

            if (one.AssetPair.IsEqualOrReversed(another.AssetPair))
                throw new ArgumentOutOfRangeException($"{nameof(one)} and {nameof(another)} are semantically equal.");

            if (!one.AssetPair.HasCommonAsset(another.AssetPair))
                throw new ArgumentOutOfRangeException($"{nameof(one)} and {nameof(another)} don't have common asset.");

            #endregion

            // Prepare left and right order books for calculating targetAssetPair
            OrderBook left = null;
            OrderBook right = null;

            if (targetAssetPair.Base == one.AssetPair.Base)
                left = one;

            if (targetAssetPair.Base == one.AssetPair.Quote)
                left = one.Reverse();

            if (targetAssetPair.Base == another.AssetPair.Base)
                left = another;

            if (targetAssetPair.Base == another.AssetPair.Quote)
                left = another.Reverse();

            if (targetAssetPair.Quote == one.AssetPair.Base)
                right = one.Reverse();

            if (targetAssetPair.Quote == one.AssetPair.Quote)
                right = one;

            if (targetAssetPair.Quote == another.AssetPair.Base)
                right = another.Reverse();

            if (targetAssetPair.Quote == another.AssetPair.Quote)
                right = another;

            #region Checking left and right

            if (left == null)
                throw new InvalidOperationException($"{nameof(left)}: {nameof(left)}");

            if (right == null)
                throw new InvalidOperationException($"{nameof(right)}: {nameof(right)}");

            if (left.AssetPair.Base != targetAssetPair.Base
                || right.AssetPair.Quote != targetAssetPair.Quote
                || left.AssetPair.Quote != right.AssetPair.Base)
                throw new InvalidOperationException($"{nameof(left)} and {nameof(right)} don't correspond to {nameof(targetAssetPair)}");

            #endregion

            var bids = new List<VolumePrice>();
            var asks = new List<VolumePrice>();

            // Calculating new bids
            foreach (var leftBid in left.Bids)
            {
                foreach (var rightBid in right.Bids)
                {
                    var newBidPrice = leftBid.Price * rightBid.Price;
                    var rightBidVolumeInBaseAsset = rightBid.Volume / leftBid.Price;
                    var newBidVolume = Math.Min(leftBid.Volume, rightBidVolumeInBaseAsset);

                    var newBidVolumePrice = new VolumePrice(newBidPrice, newBidVolume);
                    bids.Add(newBidVolumePrice);
                }
            }

            // Calculating new asks
            foreach (var leftAsk in left.Asks)
            {
                foreach (var rightAsk in right.Asks)
                {
                    var newAskPrice = leftAsk.Price * rightAsk.Price;
                    var rightAskVolumeInBaseAsset = rightAsk.Volume / leftAsk.Price;
                    var newAskVolume = Math.Min(leftAsk.Volume, rightAskVolumeInBaseAsset);

                    var newAskVolumePrice = new VolumePrice(newAskPrice, newAskVolume);
                    asks.Add(newAskVolumePrice);
                }
            }

            var source = GetSourcesPath(one.Source, another.Source);
            var conversionPath = GetConversionPath(one, another);
            var originalOrderBooks = new List<OrderBook> { one, another };
            var timestamp = left.Timestamp < right.Timestamp ? left.Timestamp : right.Timestamp;

            var result = new CrossRate(source, targetAssetPair, bids, asks, conversionPath, originalOrderBooks, timestamp);

            return result;
        }

        /// <summary>
        /// Formats conversion path.
        /// </summary>
        /// <param name="left"></param>
        /// <param name="right"></param>
        public static string GetConversionPath(OrderBook left, OrderBook right)
        {
            return left + " * " + right;
        }

        /// <summary>
        /// Formats conversion path.
        /// </summary>
        /// <param name="leftSource"></param>
        /// <param name="leftAssetPair"></param>
        /// <param name="rightSource"></param>
        /// <param name="rightAssetPair"></param>
        public static string GetConversionPath(string leftSource, string leftAssetPair, string rightSource, string rightAssetPair)
        {
            return leftSource + "-" + leftAssetPair + " * " + rightSource + "-" + rightAssetPair;
        }

        /// <summary>
        /// Formats source - source path.
        /// </summary>
        /// <param name="leftSource"></param>
        /// <param name="rightSource"></param>
        /// <returns></returns>]
        public static string GetSourcesPath(string leftSource, string rightSource)
        {
            return leftSource + "-" + rightSource;
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return ConversionPath;
        }
    }
}
