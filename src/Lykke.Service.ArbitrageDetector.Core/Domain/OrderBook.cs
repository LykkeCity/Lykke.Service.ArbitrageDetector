using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using MoreLinq;

namespace Lykke.Service.ArbitrageDetector.Core.Domain
{
    public class OrderBook
    {
        public string Source { get; }

        public AssetPair AssetPair { get; set; }

        public IReadOnlyList<VolumePrice> Bids { get; protected set; }

        public IReadOnlyList<VolumePrice> Asks { get; protected set; }

        public VolumePrice? BestBid => Bids.Any() ? Bids.MaxBy(x => x.Price) : (VolumePrice?)null;

        public VolumePrice? BestAsk => Asks.Any() ? Asks.MinBy(x => x.Price) : (VolumePrice?)null;

        public decimal BidsVolume => Bids.Sum(x => x.Volume);

        public decimal AsksVolume => Asks.Sum(x => x.Volume);

        public DateTime Timestamp { get; protected set; }


        public OrderBook(string source, AssetPair assetPair, IReadOnlyList<VolumePrice> bids, IReadOnlyList<VolumePrice> asks, DateTime timestamp)
        {
            Debug.Assert(!string.IsNullOrEmpty(source));
            Debug.Assert(assetPair != null);

            Source = source;
            AssetPair = assetPair;
            Bids = bids.Where(x => x.Price != 0 && x.Volume != 0)
                       .OrderByDescending(x => x.Price).ToList();
            Asks = asks.Where(x => x.Price != 0 && x.Volume != 0)
                       .OrderBy(x => x.Price).ToList();
            Timestamp = timestamp;
        }

        public OrderBook Reverse()
        {
            var result = new OrderBook(Source, AssetPair,
                Asks.Select(x => x.Reciprocal()).OrderBy(x => x.Price).ToList(),
                Bids.Select(x => x.Reciprocal()).OrderByDescending(x => x.Price).ToList(),
                Timestamp);
            result.AssetPair = AssetPair.Invert();

            return result;
        }

        public static string FormatSourceAssetPair(string source, string assetPair)
        {
            return $"{source}-{assetPair}";
        }

        public OrderBook DeepClone(decimal fee = 0)
        {
            var result = (OrderBook)MemberwiseClone();

            if (fee == 0)
            {
                Bids = new List<VolumePrice>(Bids);
                Asks = new List<VolumePrice>(Asks);
            }
            else
            {
                var bids = new List<VolumePrice>();
                foreach (var bid in Bids)
                {
                    var newVolumePrice = new VolumePrice(bid.Price - (bid.Price / 100 * fee), bid.Volume);
                    bids.Add(newVolumePrice);
                }
                result.Bids = bids;

                var asks = new List<VolumePrice>();
                foreach (var ask in Asks)
                {
                    var newVolumePrice = new VolumePrice(ask.Price + (ask.Price / 100 * fee), ask.Volume);
                    asks.Add(newVolumePrice);
                }
                result.Asks = asks;
            }

            return result;
        }
        
        public override string ToString()
        {
            return FormatSourceAssetPair(Source, AssetPair.Name);
        }
    }
}
