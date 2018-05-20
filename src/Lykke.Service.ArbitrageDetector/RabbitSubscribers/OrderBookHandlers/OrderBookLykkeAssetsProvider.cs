using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Common.Log;
using Lykke.Service.ArbitrageDetector.Core.Domain;
using Lykke.Service.Assets.Client;
using Lykke.Service.RateCalculator.Client;

namespace Lykke.Service.ArbitrageDetector.RabbitSubscribers.OrderBookHandlers
{
    internal sealed class OrderBookLykkeAssetsProvider
    {
        private const string LykkeExchangeName = "lykke";
        
        private readonly Dictionary<string, AssetPair> _assetPairs = new Dictionary<string, AssetPair>();

        private readonly IAssetsService _assetsService;
        private readonly IRateCalculatorClient _rateCalculatorClient;
        private readonly ILog _log;

        public OrderBookLykkeAssetsProvider(IAssetsService assetsService, IRateCalculatorClient rateCalculatorClient, ILog log)
        {
            _assetsService = assetsService ?? throw new ArgumentNullException(nameof(assetsService));
            _rateCalculatorClient = rateCalculatorClient ?? throw new ArgumentNullException(nameof(rateCalculatorClient));
            _log = log ?? throw new ArgumentNullException(nameof(log));
            Task.Run(() => Init()).Wait();
        }

        public async Task ProvideAssetsIfLykke(OrderBook orderBook)
        {
            if (orderBook.Source == LykkeExchangeName && _assetPairs.Count > 0)
            {
                var key = _assetPairs.Keys.SingleOrDefault(x => x == orderBook.AssetPairStr);
                if (key != null)
                {
                    orderBook.AssetPair = _assetPairs[key];
                }
            }
        }

        private async Task Init()
        {
            var allAssetPirs = await _assetsService.AssetPairGetAllWithHttpMessagesAsync();
            var main = allAssetPirs.Body.Where(x => !x.IsDisabled).ToList();

            var assets = main.Select(x => x.BaseAssetId).ToList();
            assets.AddRange(main.Select(x => x.QuotingAssetId).ToList());
            assets = assets.Distinct().ToList();

            var btcusd = main.Where(x => x.Id == "BTCUSD").ToList();

            foreach (var mainAmainB in btcusd)
            {
                var syn1 = new List<Assets.Client.Models.AssetPair>();
                var assetsExceptMainA = assets.Where(x => x != mainAmainB.BaseAssetId).ToList();
                foreach (var a in assetsExceptMainA)
                {
                    //TODO: тут надо искать существующую пару
                    syn1.Add(new Assets.Client.Models.AssetPair { BaseAssetId = mainAmainB.BaseAssetId, QuotingAssetId = a });
                }

                var syn2 = new List<Assets.Client.Models.AssetPair>();
                //TODO: тут надо итерироваться только по найденным парам из syn1
                foreach (var a in assetsExceptMainA)
                {
                    syn2.Add(new Assets.Client.Models.AssetPair { BaseAssetId = a, QuotingAssetId = mainAmainB.QuotingAssetId });
                }

                if (syn1.Count != syn2.Count)
                    throw new InvalidOperationException("syn1.Count != syn2.Count");

                var syn1Found = 0;
                foreach (var syn in syn1)
                {
                    var found = main.Where(x => x.BaseAssetId == syn.BaseAssetId && x.QuotingAssetId == syn.QuotingAssetId).OrderBy(x => x.Id).FirstOrDefault();
                    if (found != null)
                        syn1Found++;
                }

                var syn2Found = 0;
                foreach (var syn in syn2)
                {
                    var found = main.Where(x => x.BaseAssetId == syn.BaseAssetId && x.QuotingAssetId == syn.QuotingAssetId).OrderBy(x => x.Id).FirstOrDefault();
                    if (found != null)
                        syn2Found++;
                }

                var allTickPrices = await _rateCalculatorClient.GetMarketProfileAsync();

                var tickPricesFound = 0;
                foreach (var tickPrice in allTickPrices.Profile)
                {
                    var found = main.SingleOrDefault(x => x.Id == tickPrice.Asset);
                    if (found != null)
                        tickPricesFound++;
                }

                var syn1TickPricesFound = 0;
                foreach (var syn in syn1)
                {
                    var found = allTickPrices.Profile.SingleOrDefault(x => x.Asset == syn.Id);
                    if (found != null)
                        syn1TickPricesFound++;
                }

                var syn2TickPricesFound = 0;
                foreach (var syn in syn2)
                {
                    var found = allTickPrices.Profile.SingleOrDefault(x => x.Asset == syn.Id);
                    if (found != null)
                        syn2TickPricesFound++;
                }

                var temp = 0;
            }


            //var goodAssetPairs = assetPirs.Body
            //    .Where(x => x.Name.Contains("/")).ToList();

            //var strangeAssetPairs = assetPirs.Body
            //    .Where(x => !x.Name.Contains("/")).ToList();


            //foreach (var assetPair in goodAssetPairs)
            //{
            //    var key = assetPair.Name.Replace("/", "");
            //    var baseQuote = assetPair.Name.Split("/");
            //    var @base = baseQuote[0];
            //    var quote = baseQuote[1];
            //    if (!_assetPairs.ContainsKey(key))
            //        _assetPairs.Add(key, new AssetPair(@base, quote));
            //}

            //var all = _assetPairs.Values.Select(x => x.Base).ToList();
            //all.AddRange(_assetPairs.Values.Select(x => x.Quote));
            //all = all.Distinct().ToList();

            var temp1 = 0;
        }
    }
}
