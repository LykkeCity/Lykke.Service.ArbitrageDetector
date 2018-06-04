using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Common;
using Common.Log;
using Lykke.Service.ArbitrageDetector.AzureRepositories;
using Lykke.Service.ArbitrageDetector.Core;
using Lykke.Service.ArbitrageDetector.Core.Utils;
using Lykke.Service.ArbitrageDetector.Core.Domain;
using Lykke.Service.ArbitrageDetector.Core.Repositories;
using Lykke.Service.ArbitrageDetector.Core.Services;
using Lykke.Service.ArbitrageDetector.Services.Models;
using MoreLinq;
using Settings = Lykke.Service.ArbitrageDetector.Core.Settings;

namespace Lykke.Service.ArbitrageDetector.Services
{
    public class ArbitrageDetectorService : TimerPeriod, IArbitrageDetectorService
    {
        private readonly ConcurrentDictionary<AssetPairSource, OrderBook> _orderBooks;
        private readonly ConcurrentDictionary<AssetPairSource, CrossRate> _crossRates;
        private readonly ConcurrentDictionary<string, Arbitrage> _arbitrages;
        private readonly ConcurrentDictionary<string, Arbitrage> _arbitrageHistory;
        private bool _restartNeeded;
        private ISettings _s;
        private readonly ILog _log;
        private readonly ISettingsRepository _settingsRepository;


        public ArbitrageDetectorService(ILog log, IShutdownManager shutdownManager, ISettingsRepository settingsRepository)
            : base(100, log)
        {
            _orderBooks = new ConcurrentDictionary<AssetPairSource, OrderBook>();
            _crossRates = new ConcurrentDictionary<AssetPairSource, CrossRate>();
            _arbitrages = new ConcurrentDictionary<string, Arbitrage>();
            _arbitrageHistory = new ConcurrentDictionary<string, Arbitrage>();

            _log = log;
            shutdownManager?.Register(this);
            _settingsRepository = settingsRepository;

            Task.Run(InitSettings).Wait();
        }

        private async Task InitSettings()
        {
            var dbSettings = await _settingsRepository.GetAsync();

            if (dbSettings == null)
            {
                dbSettings = Settings.Default;
                await _settingsRepository.InsertOrReplaceAsync(Settings.Default);
            }

            _s = dbSettings;
        }


        public void Process(OrderBook orderBook)
        {
            var assets = new List<string>();
            assets.Add(_s.QuoteAsset);
            assets.AddRange(_s.BaseAssets);
            assets.AddRange(_s.IntermediateAssets);

            foreach (var asset in assets)
            {
                if (!orderBook.AssetPairStr.Contains(asset))
                    continue;

                orderBook.SetAssetPair(asset);

                var key = new AssetPairSource(orderBook.Source, orderBook.AssetPair);
                _orderBooks.AddOrUpdate(key, orderBook);

                return;
            }
        }

        public override async Task Execute()
        {
            await CalculateCrossRates();
            await RefreshArbitrages();                                        

            await RestartIfNeeded();
        }

        public async Task<IEnumerable<CrossRate>> CalculateCrossRates()
        {
            var watch = Stopwatch.StartNew();

            var newActualCrossRates = new Dictionary<AssetPairSource, CrossRate>();
            var wantedActualOrderBooks = GetWantedActualOrderBooks();

            foreach (var @base in _s.BaseAssets)
            {
                var baseAssetKeys = wantedActualOrderBooks.Keys.Where(x => x.AssetPair.ContainsAsset(@base)).ToList();
                foreach (var baseAssetkey in baseAssetKeys)
                {
                    var baseOrderBook = wantedActualOrderBooks[baseAssetkey];

                    // Trying to find wanted asset in current orderBook's asset pair
                    var wantedIntermediate = AssetPair.FromString(baseOrderBook.AssetPairStr, @base);

                    // Get intermediate currency
                    var intermediate = wantedIntermediate.Base == @base
                        ? wantedIntermediate.Quote
                        : wantedIntermediate.Base;

                    // If settings contains any and current intermediate not in the settings then ignore
                    if (_s.IntermediateAssets.Any() && !_s.IntermediateAssets.Contains(intermediate))
                        continue;

                    // If original wanted/base or base/wanted pair then just save it
                    if (intermediate == _s.QuoteAsset)
                    {
                        var intermediateBaseCrossRate = CrossRate.FromOrderBook(baseOrderBook, new AssetPair(@base, _s.QuoteAsset));

                        var key = new AssetPairSource(intermediateBaseCrossRate.ConversionPath, intermediateBaseCrossRate.AssetPair);
                        newActualCrossRates[key] = intermediateBaseCrossRate;

                        continue;
                    }

                    // Trying to find intermediate/base or base/intermediate pair from any exchange
                    var intermediateBaseKeys = wantedActualOrderBooks.Keys
                        .Where(x => x.AssetPair.ContainsAsset(intermediate) && x.AssetPair.ContainsAsset(_s.QuoteAsset))
                        .ToList();

                    foreach (var intermediateBaseKey in intermediateBaseKeys)
                    {
                        // Calculating cross rate for base/wanted pair
                        var baseIntermediateOrderBook = baseOrderBook;
                        var intermediateBaseOrderBook = wantedActualOrderBooks[intermediateBaseKey];

                        var targetAssetPair = new AssetPair(@base, _s.QuoteAsset);
                        var crossRate = CrossRate.FromOrderBooks(baseIntermediateOrderBook, intermediateBaseOrderBook, targetAssetPair);

                        var key = new AssetPairSource(crossRate.ConversionPath, crossRate.AssetPair);
                        newActualCrossRates[key] = crossRate;
                    }
                }
            }

            _crossRates.AddOrUpdateRange(newActualCrossRates);

            watch.Stop();
            if (watch.ElapsedMilliseconds > 200)
                await _log.WriteInfoAsync(GetType().Name, nameof(CalculateCrossRates), $"{watch.ElapsedMilliseconds} ms, {_crossRates.Count} cross rates, {wantedActualOrderBooks.Count} order books.");

            return _crossRates.Select(x => x.Value).ToList().AsReadOnly();
        }

        private (IList<CrossRateLine> bids, IList<CrossRateLine> asks)? CalculateCrossRateLines(IList<CrossRate> crossRates)
        {
            // If no asks or bids then return empty list
            if (!crossRates.SelectMany(x => x.Bids).Any() || !crossRates.SelectMany(x => x.Asks).Any())
                return null;

            // 1. Calculate minAsk and maxBid
            var maxBid = crossRates.SelectMany(x => x.Bids).Max(x => x.Price);
            var minAsk = crossRates.SelectMany(x => x.Asks).Min(x => x.Price);

            // No arbitrages
            if (minAsk >= maxBid)
                return null;

            // 2. Collect only arbitrages lines
            var bids = new List<CrossRateLine>();
            var asks = new List<CrossRateLine>();
            foreach (var crossRate in crossRates)
            {
                bids.AddRange(
                    crossRate.Bids
                        .Where(x => x.Price > minAsk && (_s.MinimumVolume == 0 || x.Volume >= _s.MinimumVolume))
                        .Select(x => new CrossRateLine(crossRate, x)));

                asks.AddRange(
                    crossRate.Asks
                        .Where(x => x.Price < maxBid && (_s.MinimumVolume == 0 || x.Volume >= _s.MinimumVolume))
                        .Select(x => new CrossRateLine(crossRate, x)));
            }

            // 3. Order by Price
            bids = bids.OrderByDescending(x => x.Price).ToList();
            asks = asks.OrderByDescending(x => x.Price).ToList();

            return (bids, asks);
        }

        public async Task<Dictionary<string, Arbitrage>> CalculateArbitrages()
        {
            var newArbitrages = new Dictionary<string, Arbitrage>();
            var actualCrossRates = GetActualCrossRates();

            // For each asset pair
            var uniqueAssetPairs = actualCrossRates.Select(x => x.AssetPair).Distinct().ToList();
            foreach (var assetPair in uniqueAssetPairs)
            {
                var watch = Stopwatch.StartNew();

                var assetPairCrossRates = actualCrossRates.Where(x => x.AssetPair.Equals(assetPair)).ToList();

                // For each cross rate make a line for every ask and every bid
                var bidsAndAsks = CalculateCrossRateLines(assetPairCrossRates);
                var bidsAndAsksMs = watch.ElapsedMilliseconds;

                if (!bidsAndAsks.HasValue)
                    return newArbitrages;

                var totalItarations = 0;
                var possibleArbitrages = 0;
                var bids = bidsAndAsks.Value.bids;
                var bidsCount = bids.Count;
                var asks = bidsAndAsks.Value.asks;
                var asksCount = asks.Count;
                // Calculate arbitrage for every ask and every higher bid
                for (var b = 0; b < bidsCount; b++)
                {
                    var bid = bids[b];
                    var bidPrice = bid.Price;

                    for (var a = 0; a < asksCount; a++)
                    {
                        totalItarations++;

                        var ask = asks[a];
                        var askPrice = ask.Price;

                        if (askPrice >= bidPrice)
                            continue;

                        possibleArbitrages++;

                        var spread = Arbitrage.GetSpread(bidPrice, askPrice);
                        if (_s.MinSpread < 0 && spread < _s.MinSpread)
                            continue;

                        var bidVolume = bid.Volume;
                        var askVolume = ask.Volume;
                        var volume = askVolume < bidVolume ? askVolume : bidVolume;
                        var pnL = Arbitrage.GetPnL(bidPrice, askPrice, volume);
                        if (_s.MinimumPnL > 0 && pnL < _s.MinimumPnL)
                            continue;

                        var key = Arbitrage.FormatConversionPath(bid.CrossRate.ConversionPath, ask.CrossRate.ConversionPath);
                        if (newArbitrages.TryGetValue(key, out var existed))
                        {
                            var newpnL = Arbitrage.GetPnL(bidPrice, askPrice, volume);
                            if (newpnL <= existed.PnL)
                                continue;

                            var arbitrage = new Arbitrage(assetPair, bid.CrossRate, new VolumePrice(bid.Price, bid.Volume), ask.CrossRate, new VolumePrice(ask.Price, ask.Volume));
                            newArbitrages[key] = arbitrage;
                        }
                        else
                        {
                            var arbitrage = new Arbitrage(assetPair, bid.CrossRate, new VolumePrice(bid.Price, bid.Volume), ask.CrossRate, new VolumePrice(ask.Price, ask.Volume));
                            newArbitrages.Add(key, arbitrage);
                        }
                    }
                }

                watch.Stop();
                if (watch.ElapsedMilliseconds > 1000)
                    await _log.WriteInfoAsync(GetType().Name, nameof(CalculateArbitrages), $"{watch.ElapsedMilliseconds} ms, {newArbitrages.Count} arbitrages, {actualCrossRates.Count} actual cross rates, {bidsAndAsksMs} ms for bids and asks, {bids.Count} bids, {asks.Count} asks, {totalItarations} iterations, {possibleArbitrages} possible arbitrages.");
            }

            return newArbitrages;
        }

        public async Task RefreshArbitrages()
        {
            var watch = Stopwatch.StartNew();

            var newArbitrages = await CalculateArbitrages(); // One per conversion path (with best PnL)
            var calculateArbitragesMs = watch.ElapsedMilliseconds;

            var removed = 0;
            // Remove every ended arbitrage and move it to the history
            foreach (var oldArbitrage in _arbitrages)
            {
                // If still present then do nothing
                if (newArbitrages.Keys.Contains(oldArbitrage.Key))
                    continue;

                removed++;
                MoveFromActualToHistory(oldArbitrage.Value);
            }

            // Add new arbitrages or replace existed if new PnL is better
            var added = 0;
            foreach (var newArbitrage in newArbitrages)
            {
                // New
                if (!_arbitrages.TryGetValue(newArbitrage.Key, out var oldArbitrage))
                {
                    added++;
                    _arbitrages.Add(newArbitrage.Key, newArbitrage.Value);
                }
                // Existed
                else
                {
                    if (newArbitrage.Value.PnL > oldArbitrage.PnL)
                    {
                        removed++;
                        MoveFromActualToHistory(oldArbitrage);

                        _arbitrages.Add(newArbitrage.Key, newArbitrage.Value);
                    }
                }
            }

            var beforeCleaning = _arbitrageHistory.Count;
            CleanHistory();

            watch.Stop();
            if (watch.ElapsedMilliseconds - calculateArbitragesMs > 100)
                await _log.WriteInfoAsync(GetType().Name, nameof(RefreshArbitrages), $"{watch.ElapsedMilliseconds} ms, new {newArbitrages.Count} arbitrages, {removed} removed, {added} added, {beforeCleaning - _arbitrageHistory.Count} cleaned, {_arbitrages.Count} active, {_arbitrageHistory.Count} in history.");
        }

        private void CleanHistory()
        {
            var remained = new ConcurrentDictionary<string, Arbitrage>();

            // Get distinct paths and for each path remain only %_historyMaxSize% of the best
            var uniqueAssetPairs = _arbitrageHistory.Values.Select(x => x.AssetPair).Distinct().ToList();

            foreach (var assetPair in uniqueAssetPairs)
            {
                _arbitrageHistory.Where(x => x.Value.AssetPair.Equals(assetPair))
                    .OrderByDescending(x => x.Value.PnL)
                    .Take(_s.HistoryMaxSize)
                    .ForEach(x => remained.Add(x.Key, x.Value));
            }

            _arbitrageHistory.Clear();
            _arbitrageHistory.AddRange(remained);
        }


        private Dictionary<AssetPairSource, OrderBook> GetWantedActualOrderBooks()
        {
            var result = new Dictionary<AssetPairSource, OrderBook>();

            foreach (var keyValue in _orderBooks)
            {
                // Filter by exchanges
                if (_s.Exchanges.Any() && !_s.Exchanges.Contains(keyValue.Key.Exchange))
                    continue;

                // Filter by base, quote and intermediate assets
                var assetPair = keyValue.Key.AssetPair;
                var passed = (_s.BaseAssets.Contains(assetPair.Base)
                  || _s.BaseAssets.Contains(assetPair.Quote)
                  || assetPair.ContainsAsset(_s.QuoteAsset))
                 && (!_s.IntermediateAssets.Any()
                  || (_s.IntermediateAssets.Contains(assetPair.Base)
                  || _s.IntermediateAssets.Contains(assetPair.Quote)));
                if (!passed)
                    continue;

                // Filter by expiration time
                if (DateTime.UtcNow - keyValue.Value.Timestamp < new TimeSpan(0, 0, 0, _s.ExpirationTimeInSeconds))
                {
                    result.Add(keyValue.Key, keyValue.Value);
                }
            }

            return result;
        }

        private IList<CrossRate> GetActualCrossRates()
        {
            var result = new List<CrossRate>();

            foreach (var crossRate in _crossRates)
            {
                if (DateTime.UtcNow - crossRate.Value.Timestamp < new TimeSpan(0, 0, 0, _s.ExpirationTimeInSeconds))
                {
                    result.Add(crossRate.Value);
                }
            }

            return result;
        }

        private void MoveFromActualToHistory(Arbitrage arbitrage)
        {
            var key = arbitrage.ToString();

            // Remove from actual arbitrages
            arbitrage.EndedAt = DateTime.UtcNow;
            _arbitrages.Remove(key);

            // If found in history and old PnL is better then don't replace it
            var found = _arbitrageHistory.TryGetValue(key, out var oldArbitrage);
            if (found && arbitrage.PnL < oldArbitrage.PnL)
                return;

            // Otherwise add or update
            _arbitrageHistory.AddOrUpdate(key, arbitrage);
        }

        private async Task RestartIfNeeded()
        {
            if (_restartNeeded)
            {
                _restartNeeded = false;

                _crossRates.Clear();
                _arbitrages.Clear();
                _arbitrageHistory.Clear();

                await _log.WriteInfoAsync(GetType().Name, nameof(RestartIfNeeded), $"Restarted");
            }
        }


        #region IArbitrageDetectorService

        public IEnumerable<OrderBook> GetOrderBooks()
        {
            if (!_orderBooks.Any())
                return new List<OrderBook>();

            return _orderBooks.Select(x => x.Value)
                .OrderByDescending(x => x.Timestamp)
                .ToList();
        }

        public IEnumerable<OrderBook> GetOrderBooks(string exchange, string instrument)
        {
            if (!_orderBooks.Any())
                return new List<OrderBook>();

            var result = _orderBooks.Select(x => x.Value).ToList();

            if (!string.IsNullOrWhiteSpace(exchange))
                result = result.Where(x => x.Source.ToUpper().Trim().Contains(exchange.ToUpper().Trim())).ToList();

            if (!string.IsNullOrWhiteSpace(instrument))
                result = result.Where(x => x.AssetPairStr.ToUpper().Trim().Contains(instrument.ToUpper().Trim())).ToList();

            return result.OrderByDescending(x => x.Timestamp).ToList();
        }

        public IEnumerable<CrossRate> GetCrossRates()
        {
            if (!_crossRates.Any())
                return new List<CrossRate>();

            var result = _crossRates.Select(x => x.Value)
                .OrderByDescending(x => x.Timestamp)
                .ToList();

            return result;
        }

        public IEnumerable<Arbitrage> GetArbitrages()
        {
            if (!_arbitrages.Any())
                return new List<Arbitrage>();

            return _arbitrages.Select(x => x.Value)
                .OrderByDescending(x => x.PnL)
                .ToList();
        }

        public Arbitrage GetArbitrageFromHistory(string conversionPath)
        {
            if (string.IsNullOrWhiteSpace(conversionPath))
                throw new ArgumentNullException(nameof(conversionPath));

            var bestArbitrage = _arbitrageHistory.FirstOrDefault(x => string.Equals(x.Value.ConversionPath, conversionPath, StringComparison.CurrentCultureIgnoreCase));

            return bestArbitrage.Value;
        }

        public Arbitrage GetArbitrageFromActiveOrHistory(string conversionPath)
        {
            if (string.IsNullOrWhiteSpace(conversionPath))
                throw new ArgumentNullException(nameof(conversionPath));

            var result = _arbitrages.FirstOrDefault(x => conversionPath == x.Value.ConversionPath).Value;

            if (result == null)
                result = _arbitrageHistory.FirstOrDefault(x => conversionPath == x.Value.ConversionPath).Value;

            return result;
        }

        public IEnumerable<Arbitrage> GetArbitrageHistory(DateTime since, int take)
        {
            if (!_arbitrageHistory.Any())
                return new List<Arbitrage>();

            var result = new List<Arbitrage>();

            var arbitrages = _arbitrageHistory.Select(x => x.Value).ToList();
            var uniqueConversionPaths = arbitrages.Select(x => x.ConversionPath).Distinct().ToList();

            // Find only best arbitrage for path
            foreach (var conversionPath in uniqueConversionPaths)
            {
                var pathBestArbitrage = arbitrages.OrderByDescending(x => x.PnL).First(x => x.ConversionPath == conversionPath);
                result.Add(pathBestArbitrage);
            }

            return result
                .Where(x => x.EndedAt > since)
                .OrderByDescending(x => x.PnL)
                .Take(take)
                .ToList();
        }

        public Matrix GetMatrix(string assetPair, bool isPublic = false)
        {
            if (string.IsNullOrWhiteSpace(assetPair))
                return null;

            var result = new Matrix(assetPair);

            // Filter by asset pair
            var orderBooks = _orderBooks.Values.Where(x => x.AssetPair.Name.ToUpper().Trim() == assetPair.ToUpper().Trim()).ToList();
            
            // Filter by exchanges
            if (isPublic && _s.PublicMatrixExchanges.Any())
            {
                orderBooks = orderBooks.Where(x => _s.PublicMatrixExchanges.Keys.Contains(x.Source)).ToList();
            }

            // Order by exchange name
            orderBooks = orderBooks.OrderBy(x => x.Source).ToList();

            var uniqueExchanges = orderBooks.Select(x => x.Source).Distinct().ToList();

            // Raplace exchange names
            if (isPublic && _s.PublicMatrixExchanges.Any())
            {
                uniqueExchanges = uniqueExchanges.Select(x => x.Replace(x, _s.PublicMatrixExchanges[x])).ToList();
            }

            var matrixSide = uniqueExchanges.Count;
            for (var row = 0; row < matrixSide; row++)
            {
                var cellRow = new List<MatrixCell>();
                var orderBookRow = orderBooks[row];
                var isActual = (DateTime.UtcNow - orderBookRow.Timestamp).TotalSeconds < _s.ExpirationTimeInSeconds;

                result.Exchanges.Add(new Exchange(uniqueExchanges[row], isActual));
                result.Asks.Add(orderBookRow.BestAsk?.Price);
                
                for (var col = 0; col < matrixSide; col++)
                {
                    var orderBookCol = orderBooks[col];
                    
                    if (row == 0)
                        result.Bids.Add(orderBookCol.BestBid?.Price);

                    // The same exchanges
                    if (row == col)
                    {
                        cellRow.Add(null);
                        continue;
                    }

                    MatrixCell matrixCell;
                    if (orderBookRow.BestAsk == null || orderBookCol.BestBid == null)
                    {
                        matrixCell = new MatrixCell(null, null);
                        cellRow.Add(matrixCell);
                        continue;
                    }

                    var spread = (orderBookRow.BestAsk.Value.Price - orderBookCol.BestBid.Value.Price) / orderBookCol.BestBid.Value.Price * 100;
                    matrixCell = new MatrixCell(spread, null);
                    cellRow.Add(matrixCell);
                }

                // row ends
                result.Cells.Add(cellRow);
            }

            return result;
        }

        public ISettings GetSettings()
        {
            return _s;
        }

        public async void SetSettings(ISettings settings)
        {
            if (settings == null)
                throw new ArgumentNullException(nameof(settings));

            var restartNeeded = false;

            settings.ExpirationTimeInSeconds = settings.ExpirationTimeInSeconds < 0 ? 0 : settings.ExpirationTimeInSeconds;
            if (_s.ExpirationTimeInSeconds != settings.ExpirationTimeInSeconds)
            {
                _s.ExpirationTimeInSeconds = settings.ExpirationTimeInSeconds;
                restartNeeded = true;
            }

            settings.MinimumPnL = settings.MinimumPnL < 0 ? 0 : settings.MinimumPnL;
            if (_s.MinimumPnL != settings.MinimumPnL)
            {
                _s.MinimumPnL = settings.MinimumPnL;
                restartNeeded = true;
            }

            settings.MinimumVolume = settings.MinimumVolume < 0 ? 0 : settings.MinimumVolume;
            if (_s.MinimumVolume != settings.MinimumVolume)
            {
                _s.MinimumVolume = settings.MinimumVolume;
                restartNeeded = true;
            }

            settings.MinSpread = settings.MinSpread >= 0 || settings.MinSpread < -100 ? 0 : settings.MinSpread;
            if (_s.MinSpread != settings.MinSpread)
            {
                _s.MinSpread = settings.MinSpread;
                restartNeeded = true;
            }

            if (settings.IntermediateAssets != null && !_s.IntermediateAssets.SequenceEqual(settings.IntermediateAssets))
            {
                _s.IntermediateAssets = settings.IntermediateAssets.Where(x => !string.IsNullOrWhiteSpace(x)).Select(x => x.Trim()).ToList();
                restartNeeded = true;
            }

            if (settings.BaseAssets != null && !_s.BaseAssets.SequenceEqual(settings.BaseAssets))
            {
                _s.BaseAssets = settings.BaseAssets.Where(x => !string.IsNullOrWhiteSpace(x)).Select(x => x.Trim()).ToList();
                restartNeeded = true;
            }

            if (!string.IsNullOrWhiteSpace(settings.QuoteAsset) && _s.QuoteAsset != settings.QuoteAsset)
            {
                _s.QuoteAsset = settings.QuoteAsset.Trim();
                restartNeeded = true;
            }

            if (settings.Exchanges != null && !_s.Exchanges.SequenceEqual(settings.Exchanges))
            {
                _s.Exchanges = settings.Exchanges.Where(x => !string.IsNullOrWhiteSpace(x)).Select(x => x.Trim()).ToList();
                restartNeeded = true;
            }

            if (settings.PublicMatrixAssetPairs != null && !_s.PublicMatrixAssetPairs.SequenceEqual(settings.PublicMatrixAssetPairs))
            {
                _s.PublicMatrixAssetPairs = settings.PublicMatrixAssetPairs.Where(x => !string.IsNullOrWhiteSpace(x)).Select(x => x.Trim()).ToList();
            }

            if (settings.PublicMatrixExchanges != null && !_s.PublicMatrixExchanges.SequenceEqual(settings.PublicMatrixExchanges))
            {
                _s.PublicMatrixExchanges = settings.PublicMatrixExchanges;
            }

            await _settingsRepository.InsertOrReplaceAsync(_s);

            _restartNeeded = restartNeeded;
        }

        #endregion
    }
}
