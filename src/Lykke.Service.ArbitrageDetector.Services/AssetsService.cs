using System;
using System.Collections.Generic;
using System.Linq;
using Common.Log;
using Lykke.Service.ArbitrageDetector.Core.Domain;
using Lykke.Service.ArbitrageDetector.Core.Domain.Interfaces;
using Lykke.Service.Assets.Client;
using IAssetsService = Lykke.Service.ArbitrageDetector.Core.Services.IAssetsService;
using ILykkeAssetsService = Lykke.Service.Assets.Client.IAssetsService;
using LykkeAssetPair = Lykke.Service.Assets.Client.Models.AssetPair;

namespace Lykke.Service.ArbitrageDetector.Services
{
    public class AssetsService : IAssetsService
    {
        private readonly IList<LykkeAssetPair> _lykkeAssetPairs;
        private readonly List<string> _assets = new List<string>();
        private readonly List<AssetPair> _assetPairs = new List<AssetPair>();
        private readonly List<IAssetPairAccuracy> _assetPairAccuracies = new List<IAssetPairAccuracy>();

        private readonly ILykkeAssetsService _assetsService;
        private readonly ILog _log;

        public AssetsService(ILykkeAssetsService assetsService, ILog log)
        {
            _assetsService = assetsService ?? throw new ArgumentNullException(nameof(assetsService));
            _log = log ?? throw new ArgumentNullException(nameof(log));

            _lykkeAssetPairs = GetAllLykkeAssetPairs();
            Initialize();
        }

        private void Initialize()
        {
            var assetPairs = new List<AssetPair>();
            var assetPairAccuracies = new List<IAssetPairAccuracy>();
            var assets = new List<string>();

            var goodAssetPairs = _lykkeAssetPairs
                .Where(x => x.Name != null && x.Name.Contains("/")).ToList();

            foreach (var lykkeAssetPair in goodAssetPairs)
            {
                var assetPairName = lykkeAssetPair.Name.Replace("/", "");
                var baseQuote = lykkeAssetPair.Name.Split("/");
                var @base = baseQuote[0];
                var quote = baseQuote[1];
                if (!assetPairs.Any(x => string.Equals(x.Name, assetPairName, StringComparison.OrdinalIgnoreCase)))
                {
                    var assetPair = new AssetPair(@base, quote);
                    assetPairs.Add(assetPair);
                    assetPairAccuracies.Add(new AssetPairAccuracy(assetPair, lykkeAssetPair.Accuracy, lykkeAssetPair.InvertedAccuracy));
                }

                if (!assets.Any(x => string.Equals(x, @base, StringComparison.OrdinalIgnoreCase)))
                    assets.Add(@base);

                if (!assets.Any(x => string.Equals(x, quote, StringComparison.OrdinalIgnoreCase)))
                    assets.Add(quote);
            }

            assetPairs = assetPairs.OrderBy(x => x.Name).ToList();
            _assetPairs.AddRange(assetPairs);

            assetPairAccuracies = assetPairAccuracies.OrderBy(x => x.AssetPair.Name).ToList();
            _assetPairAccuracies.AddRange(assetPairAccuracies);

            assets = assets.OrderBy(x => x).ToList();
            _assets.AddRange(assets);
        }

        public int InferBaseAndQuoteAssets(OrderBook orderBook)
        {
            if (orderBook == null)
                throw new ArgumentNullException(nameof(orderBook));

            var assetPairStr = orderBook.AssetPairStr;

            // The exact asset pair
            var assetPair = _assetPairs.SingleOrDefault(x => string.Equals(x.Name, assetPairStr, StringComparison.OrdinalIgnoreCase));
            if (!assetPair.IsEmpty())
            {
                orderBook.AssetPair = assetPair;
                return 2;
            }

            AssetPair oneInfered;
            // Try to infer by assets
            foreach (var asset in _assets)
            {
                if (assetPairStr.ToUpper().Contains(asset.ToUpper()))
                {
                    var otherAsset = assetPairStr.ToUpper().Replace(asset.ToUpper(), string.Empty);
                    var infered = 1;

                    if (_assets.Any(x => string.Equals(x, otherAsset, StringComparison.OrdinalIgnoreCase)))
                        infered = 2;

                    var @base = assetPairStr.ToUpper().StartsWith(asset.ToUpper()) ? asset : otherAsset;
                    var quote = string.Equals(@base, asset, StringComparison.OrdinalIgnoreCase) ? otherAsset : asset;
                    assetPair = new AssetPair(@base, quote);

                    if (infered == 1)
                    {
                        oneInfered = assetPair;
                        continue; // still try to find both assets
                    }

                    // If found both assets then stop looking
                    orderBook.SetAssetPair(assetPair);
                    return 2;
                }
            }

            // If found only one asset then use it
            if (!oneInfered.IsEmpty())
            {
                orderBook.SetAssetPair(oneInfered);
                return 1;
            }

            return 0;
        }

        public IAssetPairAccuracy GetAccuracy(AssetPair assetPair)
        {
            if (assetPair.IsEmpty())
                throw new ArgumentOutOfRangeException(nameof(assetPair));

            var foundAssetPair = _assetPairAccuracies.SingleOrDefault(x => x.AssetPair.Equals(assetPair));

            return foundAssetPair;
        }

        private IList<LykkeAssetPair> GetAllLykkeAssetPairs()
        {
            return _assetsService.AssetPairGetAll();
        }
    }
}
