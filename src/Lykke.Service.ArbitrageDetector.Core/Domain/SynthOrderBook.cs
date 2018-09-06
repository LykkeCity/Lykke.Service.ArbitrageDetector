using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Lykke.Service.ArbitrageDetector.Core.Domain
{
    public class SynthOrderBook
    {
        public string Source => string.Join("-", OriginalOrderBooks.Select(x => x.Source));

        public AssetPair AssetPair { get; }

        public IEnumerable<VolumePrice> Bids => OrderedVolumePrices(GetBids);

        public IEnumerable<VolumePrice> Asks => OrderedVolumePrices(GetAsks);

        public VolumePrice? BestBid => Bids.FirstOrDefault();

        public VolumePrice? BestAsk => Asks.FirstOrDefault();

        public IReadOnlyList<OrderBook> OriginalOrderBooks { get; }

        public string ConversionPath => string.Join(" * ", OriginalOrderBooks.Select(x => $"{x.Source}-{x.AssetPair.Name}"));

        public DateTime Timestamp => OriginalOrderBooks.Select(x => x.Timestamp).Min();


        public SynthOrderBook(AssetPair assetPair, IReadOnlyList<OrderBook> originalOrderBooks)
        {
            AssetPair = assetPair;
            OriginalOrderBooks = originalOrderBooks;
        }


        public static SynthOrderBook FromOrderBook(OrderBook orderBook, AssetPair target)
        {
            Debug.Assert(orderBook != null);
            Debug.Assert(!target.IsEmpty());
            Debug.Assert(target.IsEqualOrReversed(orderBook.AssetPair));

            var result = new SynthOrderBook(target, new List<OrderBook> { orderBook });

            return result;
        }

        public static SynthOrderBook FromOrderBooks(OrderBook first, OrderBook second, AssetPair target)
        {
            Debug.Assert(first != null);
            Debug.Assert(!first.AssetPair.IsEmpty());
            Debug.Assert(second != null);
            Debug.Assert(!second.AssetPair.IsEmpty());
            Debug.Assert(!target.IsEmpty());

            var result = new SynthOrderBook(target, GetOrdered(new List<OrderBook> { first, second }, target));

            return result;
        }

        public static SynthOrderBook FromOrderBooks(OrderBook first, OrderBook second, OrderBook third, AssetPair target)
        {
            Debug.Assert(first != null);
            Debug.Assert(!first.AssetPair.IsEmpty());
            Debug.Assert(second != null);
            Debug.Assert(!second.AssetPair.IsEmpty());
            Debug.Assert(third != null);
            Debug.Assert(!third.AssetPair.IsEmpty());
            Debug.Assert(!target.IsEmpty());

            var result = new SynthOrderBook(target, GetOrdered(new List<OrderBook> { first, second, third }, target));

            return result;
        }


        public static IReadOnlyList<SynthOrderBook> GetSynthsFrom1(AssetPair target,
            IReadOnlyList<OrderBook> sourceOrderBooks, IReadOnlyList<OrderBook> allOrderBooks)
        {
            var result = new List<SynthOrderBook>();

            // Trying to find base asset in current source's asset pair
            var withBaseOrQuoteOrderBooks = sourceOrderBooks.Where(x => x.AssetPair.ContainsAsset(target.Base) ||
                                                                        x.AssetPair.ContainsAsset(target.Quote)).ToList();

            foreach (var withBaseOrQuoteOrderBook in withBaseOrQuoteOrderBooks)
            {
                var withBaseOrQuoteAssetPair = withBaseOrQuoteOrderBook.AssetPair;

                // Get intermediate asset
                var intermediateId = withBaseOrQuoteAssetPair.GetOtherAsset(target.Base)
                                  ?? withBaseOrQuoteAssetPair.GetOtherAsset(target.Quote);

                // If current is target or inverted then just use it
                if (intermediateId == target.Base || intermediateId == target.Quote)
                {
                    if (!withBaseOrQuoteOrderBook.Asks.Any() && !withBaseOrQuoteOrderBook.Bids.Any())
                        continue;

                    var synthOrderBook = FromOrderBook(withBaseOrQuoteOrderBook, target);
                    result.Add(synthOrderBook);
                }
            }

            return result;
        }

        public static IReadOnlyList<SynthOrderBook> GetSynthsFrom2(AssetPair target,
            IReadOnlyList<OrderBook> sourceOrderBooks, IReadOnlyList<OrderBook> allOrderBooks)
        {
            var result = new List<SynthOrderBook>();

            // Trying to find base asset in current source's asset pair
            var withBaseOrQuoteOrderBooks = sourceOrderBooks.Where(x => x.AssetPair.ContainsAsset(target.Base) ||
                                                                        x.AssetPair.ContainsAsset(target.Quote)).ToList();

            foreach (var withBaseOrQuoteOrderBook in withBaseOrQuoteOrderBooks)
            {
                var withBaseOrQuoteAssetPair = withBaseOrQuoteOrderBook.AssetPair;

                // Get intermediate asset
                var intermediateId = withBaseOrQuoteAssetPair.GetOtherAsset(target.Base)
                                ?? withBaseOrQuoteAssetPair.GetOtherAsset(target.Quote);

                // 1. If current is target or inverted then just use it
                if (intermediateId == target.Base || intermediateId == target.Quote)
                    continue; // The pairs are the same or inverted (it is from 1 order book)

                // 1. If current is base&intermediate then find quote&intermediate
                if (withBaseOrQuoteAssetPair.ContainsAsset(target.Base))
                {
                    var baseAndIntermediate = withBaseOrQuoteOrderBook;
                    // Trying to find quote/intermediate or intermediate/quote pair (quote&intermediate)
                    var intermediateQuoteOrderBooks = allOrderBooks
                        .Where(x => x.AssetPair.ContainsAsset(intermediateId) && x.AssetPair.ContainsAsset(target.Quote))
                        .ToList();

                    foreach (var intermediateQuoteOrderBook in intermediateQuoteOrderBooks)
                    {
                        if (!baseAndIntermediate.Asks.Any() && !baseAndIntermediate.Bids.Any()
                            || !intermediateQuoteOrderBook.Asks.Any() && !intermediateQuoteOrderBook.Bids.Any())
                            continue;

                        var synthOrderBook = FromOrderBooks(baseAndIntermediate, intermediateQuoteOrderBook, target);
                        result.Add(synthOrderBook);
                    }
                }

                // 2. If current is quote&intermediate then find base&intermediate
                if (withBaseOrQuoteAssetPair.ContainsAsset(target.Quote))
                {
                    var quoteAndIntermediate = withBaseOrQuoteOrderBook;
                    // Trying to find base/intermediate or intermediate/base pair (base&intermediate)
                    var intermediateBaseOrderBooks = allOrderBooks
                        .Where(x => x.AssetPair.ContainsAsset(intermediateId) && x.AssetPair.ContainsAsset(target.Base))
                        .ToList();

                    foreach (var intermediateBaseOrderBook in intermediateBaseOrderBooks)
                    {
                        if (!intermediateBaseOrderBook.Asks.Any() && !intermediateBaseOrderBook.Bids.Any()
                            || !quoteAndIntermediate.Asks.Any() && !quoteAndIntermediate.Bids.Any())
                            continue;

                        var synthOrderBook = FromOrderBooks(intermediateBaseOrderBook, quoteAndIntermediate, target);
                        result.Add(synthOrderBook);
                    }
                }
            }

            return result;
        }

        public static IReadOnlyList<SynthOrderBook> GetSynthsFrom3(AssetPair target,
            IReadOnlyList<OrderBook> sourceOrderBooks, IReadOnlyList<OrderBook> allOrderBooks)
        {
            var result = new List<SynthOrderBook>();

            var woBaseAndQuoteOrderBooks = sourceOrderBooks
                .Where(x => !x.AssetPair.ContainsAsset(target.Base)
                         && !x.AssetPair.ContainsAsset(target.Quote)).ToList();

            foreach (var woBaseAndQuoteOrderBook in woBaseAndQuoteOrderBooks)
            {
                // Get assets from order book
                var @base = woBaseAndQuoteOrderBook.AssetPair.Base;
                var quote = woBaseAndQuoteOrderBook.AssetPair.Quote;

                // Trying to find pair from @base to target.Base and quote to target.Quote
                var baseTargetBaseOrderBooks = allOrderBooks.Where(x => x.AssetPair.ContainsAssets(@base, target.Base)).ToList();
                foreach (var baseTargetBaseOrderBook in baseTargetBaseOrderBooks)
                {
                    var quoteTargetQuoteOrderBooks = allOrderBooks.Where(x => x.AssetPair.ContainsAssets(quote, target.Quote)).ToList();
                    foreach (var quoteTargetQuoteOrderBook in quoteTargetQuoteOrderBooks)
                    {
                        if (!baseTargetBaseOrderBook.Asks.Any() && !baseTargetBaseOrderBook.Bids.Any()
                            || !woBaseAndQuoteOrderBook.Asks.Any() && !woBaseAndQuoteOrderBook.Bids.Any()
                            || !quoteTargetQuoteOrderBook.Asks.Any() && !quoteTargetQuoteOrderBook.Bids.Any())
                            continue;

                        var synthOrderBook = FromOrderBooks(baseTargetBaseOrderBook, woBaseAndQuoteOrderBook, quoteTargetQuoteOrderBook, target);
                        result.Add(synthOrderBook);
                    }
                }

                // Trying to find pair from @base to target.Quote and quote to target.Base
                var baseTargetQuoteOrderBooks = allOrderBooks.Where(x => x.AssetPair.ContainsAssets(@base, target.Quote)).ToList();
                foreach (var baseTargetQuoteOrderBook in baseTargetQuoteOrderBooks)
                {
                    var quoteTargetBaseOrderBooks = allOrderBooks.Where(x => x.AssetPair.ContainsAssets(quote, target.Base)).ToList();
                    foreach (var quoteTargetBaseOrderBook in quoteTargetBaseOrderBooks)
                    {
                        if (!quoteTargetBaseOrderBook.Asks.Any() && !quoteTargetBaseOrderBook.Bids.Any()
                            || !woBaseAndQuoteOrderBook.Asks.Any() && !woBaseAndQuoteOrderBook.Bids.Any()
                            || !baseTargetQuoteOrderBook.Asks.Any() && !baseTargetQuoteOrderBook.Bids.Any())
                            continue;

                        var synthOrderBook = FromOrderBooks(quoteTargetBaseOrderBook, woBaseAndQuoteOrderBook, baseTargetQuoteOrderBook, target);
                        result.Add(synthOrderBook);
                    }
                }
            }

            return result;
        }

        public static IReadOnlyList<SynthOrderBook> GetSynthsFromAll(AssetPair target,
            IReadOnlyList<OrderBook> sourceOrderBooks, IReadOnlyList<OrderBook> allOrderBooks)
        {
            var result = new List<SynthOrderBook>();

            var synthOrderBookFrom1Pair = GetSynthsFrom1(target, sourceOrderBooks, allOrderBooks);
            result.AddRange(synthOrderBookFrom1Pair);
            var synthOrderBookFrom2Pairs = GetSynthsFrom2(target, sourceOrderBooks, allOrderBooks);
            result.AddRange(synthOrderBookFrom2Pairs);
            var synthOrderBookFrom3Pairs = GetSynthsFrom3(target, sourceOrderBooks, allOrderBooks);
            result.AddRange(synthOrderBookFrom3Pairs);

            return result;
        }

        public static IReadOnlyList<SynthOrderBook> GetSynthsFromAll(AssetPair target, OrderBook source,
            IReadOnlyList<OrderBook> allOrderBooks)
        {
            return GetSynthsFromAll(target, new List<OrderBook> { source }, allOrderBooks);
        }


        public static IDictionary<AssetPair, OrderBook> PrepareForEnumeration(IReadOnlyList<OrderBook> orderBooks, AssetPair target)
        {
            var result = new Dictionary<AssetPair, OrderBook>();

            var chainedAssetPairs = GetChained(orderBooks, target);
            var orderedOrderBooks = GetOrdered(orderBooks, target);
            Debug.Assert(orderBooks.Count == orderedOrderBooks.Count && orderedOrderBooks.Count == chainedAssetPairs.Count);

            for (var i = 0; i < orderBooks.Count; i++)
                result.Add(chainedAssetPairs[i], orderedOrderBooks[i]);

            return result;
        }

        public override string ToString()
        {
            return ConversionPath;
        }


        public IEnumerable<VolumePrice> OrderedVolumePrices(Func<OrderBook, AssetPair, IEnumerable<VolumePrice>> getOrdersMethod)
        {
            var prepared = PrepareForEnumeration(OriginalOrderBooks, AssetPair);

            if (prepared.Count == 1)
            {
                var keyValue = prepared.ElementAt(0);
                foreach (var limitOrder in getOrdersMethod(keyValue.Value, keyValue.Key))
                    yield return limitOrder;
            }

            if (prepared.Count == 2)
            {
                var left = prepared.ElementAt(0);
                var right = prepared.ElementAt(1);

                var leftOrders = getOrdersMethod(left.Value, left.Key);
                var rightOrders = getOrdersMethod(right.Value, right.Key);

                foreach (var order in GetOrderedVolumePrices(leftOrders, rightOrders))
                    yield return order;
            }

            if (prepared.Count == 3)
            {
                var left = prepared.ElementAt(0);
                var middle = prepared.ElementAt(1);
                var right = prepared.ElementAt(2);

                var leftOrders = getOrdersMethod(left.Value, left.Key);
                var middleOrders = getOrdersMethod(middle.Value, middle.Key);
                var rightOrders = getOrdersMethod(right.Value, right.Key);

                foreach (var order in GetOrderedVolumePrices(leftOrders, middleOrders, rightOrders))
                    yield return order;
            }
        }

        private static IEnumerable<VolumePrice> GetOrderedVolumePrices(IEnumerable<VolumePrice> leftOrders,
            IEnumerable<VolumePrice> rightOrders)
        {
            var leftEnumerator = leftOrders.GetEnumerator();
            var rightEnumerator = rightOrders.GetEnumerator();

            if (!leftEnumerator.MoveNext() || !rightEnumerator.MoveNext())
                yield break;

            var currentLeftOrder = leftEnumerator.Current;
            var currentRightOrder = rightEnumerator.Current;

            // Just return first generated order
            yield return SynthVolumePrice(currentLeftOrder, currentRightOrder);

            while (true)
            {
                var whichOrders = GetWithMinVolumeInBaseAsset(currentLeftOrder, currentRightOrder);

                if (whichOrders.Contains(WhichOrder.Left))
                {
                    if (!leftEnumerator.MoveNext())
                        break;
                    currentLeftOrder = leftEnumerator.Current;
                }

                if (whichOrders.Contains(WhichOrder.Right))
                {
                    if (!rightEnumerator.MoveNext())
                        break;
                    currentRightOrder = rightEnumerator.Current;
                }

                yield return SynthVolumePrice(currentLeftOrder, currentRightOrder);
            }

            leftEnumerator.Dispose();
            rightEnumerator.Dispose();
        }

        private static IEnumerable<VolumePrice> GetOrderedVolumePrices(IEnumerable<VolumePrice> leftOrders,
            IEnumerable<VolumePrice> middleOrders, IEnumerable<VolumePrice> rightOrders)
        {
            var leftEnumerator = leftOrders.GetEnumerator();
            var middleEnumerator = middleOrders.GetEnumerator();
            var rightEnumerator = rightOrders.GetEnumerator();

            if (!leftEnumerator.MoveNext() || !middleEnumerator.MoveNext() || !rightEnumerator.MoveNext())
                yield break;

            var currentLeftOrder = leftEnumerator.Current;
            var currentMiddleOrder = middleEnumerator.Current;
            var currentRightOrder = rightEnumerator.Current;

            // Just return first generated order
            yield return SynthVolumePrice(currentLeftOrder, currentMiddleOrder, currentRightOrder);

            while (true)
            {
                var whichOrders = GetWithMinVolumeInBaseAsset(currentLeftOrder, currentMiddleOrder, currentRightOrder);

                if (whichOrders.Contains(WhichOrder.Left))
                {
                    if (!leftEnumerator.MoveNext())
                        break;
                    currentLeftOrder = leftEnumerator.Current;
                }

                if (whichOrders.Contains(WhichOrder.Middle))
                {
                    if (!middleEnumerator.MoveNext())
                        break;
                    currentMiddleOrder = middleEnumerator.Current;
                }

                if (whichOrders.Contains(WhichOrder.Right))
                {
                    if (!rightEnumerator.MoveNext())
                        break;
                    currentRightOrder = rightEnumerator.Current;
                }

                yield return SynthVolumePrice(currentLeftOrder, currentMiddleOrder, currentRightOrder);
            }

            leftEnumerator.Dispose();
            middleEnumerator.Dispose();
            rightEnumerator.Dispose();
        }


        public static IEnumerable<VolumePrice> GetBids(OrderBook orderBook, AssetPair target)
        {
            Debug.Assert(orderBook != null);
            Debug.Assert(!target.IsEmpty());
            Debug.Assert(target.IsEqualOrReversed(orderBook.AssetPair));

            var bids = orderBook.Bids;
            var asks = orderBook.Asks;

            // Streight
            if (orderBook.AssetPair.Base == target.Base &&
                orderBook.AssetPair.Quote == target.Quote)
            {
                foreach (var bid in bids)
                {
                    yield return bid;
                }
            }

            // Inverted
            if (orderBook.AssetPair.Base == target.Quote &&
                orderBook.AssetPair.Quote == target.Base)
            {
                foreach (var ask in asks)
                {
                    var bid = ask.Reciprocal();
                    yield return bid;
                }
            }
        }

        public static IEnumerable<VolumePrice> GetAsks(OrderBook orderBook, AssetPair target)
        {
            Debug.Assert(orderBook != null);
            Debug.Assert(!target.IsEmpty());
            Debug.Assert(target.IsEqualOrReversed(orderBook.AssetPair));

            var bids = orderBook.Bids;
            var asks = orderBook.Asks;

            // Streight
            if (orderBook.AssetPair.Base == target.Base &&
                orderBook.AssetPair.Quote == target.Quote)
            {
                foreach (var ask in asks)
                {
                    yield return ask;
                }
            }

            // Inverted
            if (orderBook.AssetPair.Base == target.Quote &&
                orderBook.AssetPair.Quote == target.Base)
            {
                foreach (var bid in bids)
                {
                    var ask = bid.Reciprocal();
                    yield return ask;
                }
            }
        }

        public static IReadOnlyList<OrderBook> GetOrdered(IReadOnlyCollection<OrderBook> orderBooks, AssetPair target)
        {
            Debug.Assert(orderBooks != null);
            Debug.Assert(orderBooks.Any());
            Debug.Assert(!target.IsEmpty());

            var result = new List<OrderBook>();

            var @base = target.Base;
            var quote = target.Quote;

            var first = orderBooks.Single(x => x.AssetPair.ContainsAsset(@base));
            result.Add(first);

            if (orderBooks.Count == 1)
                return result;

            var nextAsset = first.AssetPair.GetOtherAsset(@base);
            var second = orderBooks.Single(x => x.AssetPair.ContainsAsset(nextAsset) && !x.AssetPair.IsEqualOrReversed(first.AssetPair));
            result.Add(second);

            if (orderBooks.Count == 2)
                return result;

            nextAsset = second.AssetPair.GetOtherAsset(nextAsset);
            var third = orderBooks.Single(x => x.AssetPair.ContainsAsset(nextAsset) && x.AssetPair.ContainsAsset(quote));
            result.Add(third);

            return result;
        }

        public static IReadOnlyList<AssetPair> GetChained(IReadOnlyCollection<OrderBook> orderBooks, AssetPair target)
        {
            Debug.Assert(orderBooks != null);
            Debug.Assert(orderBooks.Any());
            Debug.Assert(!target.IsEmpty());

            var result = new List<AssetPair>();

            var @base = target.Base;
            var quote = target.Quote;

            var first = orderBooks.Single(x => x.AssetPair.ContainsAsset(@base)).AssetPair;
            if (first.Quote == @base)
                first = first.Reverse();
            result.Add(first);

            if (orderBooks.Count == 1)
            {
                Debug.Assert(first.Quote == quote);
                return result;
            }

            var nextAsset = first.GetOtherAsset(@base);
            var second = orderBooks.Single(x => x.AssetPair.ContainsAsset(nextAsset) && !x.AssetPair.IsEqualOrReversed(first)).AssetPair;
            if (second.Quote == nextAsset)
                second = second.Reverse();
            result.Add(second);

            if (orderBooks.Count == 2)
            {
                Debug.Assert(second.Quote == quote);
                return result;
            }

            nextAsset = second.GetOtherAsset(nextAsset);
            var third = orderBooks.Single(x => x.AssetPair.ContainsAsset(nextAsset) && x.AssetPair.ContainsAsset(quote)).AssetPair;
            if (third.Quote == nextAsset)
                third = third.Reverse();
            result.Add(third);

            Debug.Assert(third.Quote == quote);
            return result;
        }


        private static VolumePrice SynthVolumePrice(VolumePrice left, VolumePrice right)
        {
            var newPrice = left.Price * right.Price;

            var rightVolumeInBaseAsset = right.Volume / left.Price;
            var minVolume = Math.Min(left.Volume, rightVolumeInBaseAsset);

            var result = new VolumePrice(newPrice, minVolume);

            return result;
        }

        private static WhichOrder[] GetWithMinVolumeInBaseAsset(VolumePrice left, VolumePrice right)
        {
            var rightVolumeInBaseAsset = right.Volume / left.Price;
            var minVolume = Math.Min(left.Volume, rightVolumeInBaseAsset);

            var result = new List<WhichOrder>();

            if (left.Volume == minVolume)
                result.Add(WhichOrder.Left);

            if (rightVolumeInBaseAsset == minVolume)
                result.Add(WhichOrder.Right);

            return result.ToArray();
        }

        private static VolumePrice SynthVolumePrice(VolumePrice left, VolumePrice middle, VolumePrice right)
        {
            var newPrice = left.Price * middle.Price * right.Price;

            var middleVolumeInBaseAsset = middle.Volume / left.Price;

            var interimBidPrice = left.Price * middle.Price;
            var rightVolumeInBaseAsset = right.Volume / interimBidPrice;

            var minVolume = Math.Min(Math.Min(left.Volume, middleVolumeInBaseAsset), rightVolumeInBaseAsset);

            var result = new VolumePrice(newPrice, minVolume);

            return result;
        }

        private static WhichOrder[] GetWithMinVolumeInBaseAsset(VolumePrice left, VolumePrice middle, VolumePrice right)
        {
            var middleVolumeInBaseAsset = middle.Volume / left.Price;

            var interimBidPrice = left.Price * middle.Price;
            var rightVolumeInBaseAsset = right.Volume / interimBidPrice;

            var minVolume = Math.Min(Math.Min(left.Volume, middleVolumeInBaseAsset), rightVolumeInBaseAsset);

            var result = new List<WhichOrder>();

            if (left.Volume == minVolume)
                result.Add(WhichOrder.Left);

            if (middleVolumeInBaseAsset == minVolume)
                result.Add(WhichOrder.Middle);

            if (rightVolumeInBaseAsset == minVolume)
                result.Add(WhichOrder.Right);

            return result.ToArray();
        }

        private enum WhichOrder { Left, Middle, Right }
    }
}
