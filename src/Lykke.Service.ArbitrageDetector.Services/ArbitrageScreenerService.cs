using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Common;
using Common.Log;
using Lykke.Service.ArbitrageDetector.Core.Domain;
using Lykke.Service.ArbitrageDetector.Core.Services.Infrastructure;
using Lykke.Service.Assets.Client;
using Lykke.Service.RateCalculator.Client;
using Lykke.Service.RateCalculator.Client.AutorestClient.Models;
using AssetPair = Lykke.Service.ArbitrageDetector.Core.Domain.AssetPair;
using AssetsAssetPair = Lykke.Service.Assets.Client.Models.AssetPair;

namespace Lykke.Service.ArbitrageDetector.Services
{
    public class ArbitrageScreenerService : TimerPeriod
    {
        private readonly IAssetsService _assetsService;
        private readonly IRateCalculatorClient _rateCalculatorClient;
        private readonly ILog _log;

        public ArbitrageScreenerService(IAssetsService assetsService, IRateCalculatorClient rateCalculatorClient, ILog log, IShutdownManager shutdownManager)
            : base(60 * 1000, log)
        {
            _assetsService = assetsService ?? throw new ArgumentNullException(nameof(assetsService));
            _rateCalculatorClient = rateCalculatorClient ?? throw new ArgumentNullException(nameof(rateCalculatorClient));
            _log = log ?? throw new ArgumentNullException(nameof(log));
            shutdownManager?.Register(this);
        }

        public override async Task Execute()
        {
            IEnumerable<string> arbitragesFrom2Syn = null;
            try
            {
                var allAssetPirs = await _assetsService.AssetPairGetAllWithHttpMessagesAsync();
                var allTickPrices = await _rateCalculatorClient.GetMarketProfileAsync();

                var main = GetMain(allAssetPirs.Body);
                var assets = GetAssets(main);

                arbitragesFrom2Syn = GetArbitragesFrom2SyntheticLists(main, assets, allTickPrices.Profile);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
            
        }

        private IList<AssetPair> GetMain(IEnumerable<AssetsAssetPair> assetPairs)
        {
            var result = new List<AssetPair>();

            var goodAssetPairs = assetPairs.Where(x => !x.IsDisabled && x.Name.Contains("/")).ToList();

            foreach (var assetPair in goodAssetPairs)
            {
                var baseQuote = assetPair.Name.Split("/");
                var @base = baseQuote[0];
                var quote = baseQuote[1];
                result.Add(new AssetPair(@base, quote));
            }

            result = result.OrderBy(x => x.Name).ToList();

            return result;
        }

        private IEnumerable<string> GetAssets(IEnumerable<AssetPair> main)
        {
            var result = main.Select(x => x.Base).ToList();
            result.AddRange(main.Select(x => x.Quote).ToList());
            result = result.Distinct().ToList();

            result = result.OrderBy(x => x).ToList();

            return result;
        }

        private IEnumerable<string> GetArbitragesFrom2SyntheticLists(IList<AssetPair> main, IEnumerable<string> assets, IList<FeedData> tickPrices)
        {
            var result = new List<string>();

            foreach (var mainAmainB in main)
            {
                // TickPrice for current mainAmainB
                var mainAmainBTickPrice = tickPrices.FirstOrDefault(x => x.Asset == mainAmainB.Name);
                if (mainAmainBTickPrice == null)
                    continue;

                // Syn1 generation
                var syn1 = new List<AssetPair>();
                var assetsExceptMainA = assets.Where(x => x != mainAmainB.Base).ToList();
                foreach (var a in assetsExceptMainA)
                {
                    //TODO: should looking for existed asset pairs only?
                    syn1.Add(new AssetPair(mainAmainB.Base, a));
                }

                // Syn2 generation
                var syn2 = new List<AssetPair>();
                //TODO: should iterate only through asset pairs existed in syn1?
                foreach (var a in assetsExceptMainA)
                {
                    syn2.Add(new AssetPair(a, mainAmainB.Quote));
                }

                // Length must be equal
                if (syn1.Count != syn2.Count)
                    throw new InvalidOperationException("syn1.Count != syn2.Count");

                var synLenght = syn1.Count;
                for (var i = 0; i < synLenght; i++)
                {
                    // Find current syn1 and syn2 asset pairs (streight or reversed) in main
                    var syn1Pair = main.Where(x => x.IsEqualOrReversed(syn1[i])).OrderBy(x => x.Name).FirstOrDefault();
                    var syn2Pair = main.Where(x => x.IsEqualOrReversed(syn2[i])).OrderBy(x => x.Name).FirstOrDefault();

                    // Find current syn1 and syn2 asset pairs (streight or reversed) in tickPrices
                    var syn1TP = tickPrices.FirstOrDefault(x => x.Asset == syn1[i].Name || x.Asset == syn1[i].Reverse().Name);
                    var syn2TP = tickPrices.FirstOrDefault(x => x.Asset == syn2[i].Name || x.Asset == syn2[i].Reverse().Name);

                    // If all found
                    if (!syn1Pair.IsEmpty() && !syn2Pair.IsEmpty() && syn1TP != null && syn2TP != null)
                    {
                        // Create two order books for syn1 and syn2
                        var syn1OrderBook = new OrderBook("lykke", syn1Pair.Name, new[] { new VolumePrice((decimal)syn1TP.Bid, 1) }, new[] { new VolumePrice((decimal)syn1TP.Ask, 1) }, syn1TP.DateTime);
                        syn1OrderBook.SetAssetPair(syn1Pair.Base);
                        var syn2OrderBook = new OrderBook("lykke", syn2Pair.Name, new[] { new VolumePrice((decimal)syn2TP.Bid, 1) }, new[] { new VolumePrice((decimal)syn2TP.Ask, 1) }, syn2TP.DateTime);
                        syn2OrderBook.SetAssetPair(syn2Pair.Base);

                        // Check if created order books are valid
                        if (syn1OrderBook.BestAsk == null || syn1OrderBook.BestAsk.Value.Price == 0
                            || syn2OrderBook.BestAsk == null || syn2OrderBook.BestAsk.Value.Price == 0)
                            continue;

                        // Create cross rate from both order books
                        var crossRate = CrossRate.FromOrderBooks(syn1OrderBook, syn2OrderBook, mainAmainB);

                        // Detecting arbitrages
                        var arbitrage1 = "";
                        var arbitrage2 = "";
                        var conversionPath = crossRate.ConversionPath.Replace("lykke-", "");

                        if (crossRate.BestBid.HasValue
                            && (decimal)mainAmainBTickPrice.Ask < crossRate.BestBid.Value.Price
                            && crossRate.BestBid.Value.Price != 0)
                            arbitrage1 = $"{mainAmainBTickPrice.Asset}.Ask < ({conversionPath}).Bid = {mainAmainBTickPrice.Ask.ToString("0.######")} < {crossRate.BestBid.Value.Price.ToString("0.######")}, {mainAmainBTickPrice.DateTime}, {syn1TP.DateTime}, {syn2TP.DateTime}";

                        if (crossRate.BestAsk.HasValue
                            && (decimal)mainAmainBTickPrice.Bid > crossRate.BestAsk.Value.Price
                            && crossRate.BestAsk.Value.Price != 0)
                            arbitrage2 = $"{mainAmainBTickPrice.Asset}.Bid > ({conversionPath}).Ask = {mainAmainBTickPrice.Bid.ToString("0.######")} > {crossRate.BestAsk.Value.Price.ToString("0.######")}, {mainAmainBTickPrice.DateTime}, {syn1TP.DateTime}, {syn2TP.DateTime}";

                        if (!string.IsNullOrWhiteSpace(arbitrage1))
                            result.Add(arbitrage1);

                        if (!string.IsNullOrWhiteSpace(arbitrage2))
                            result.Add(arbitrage2);
                    }
                }
            }

            return result;
        }
    }
}
