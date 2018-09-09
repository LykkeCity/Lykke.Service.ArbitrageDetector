using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Autofac;
using Common;
using Common.Log;
using Lykke.Common.Log;
using Lykke.Service.ArbitrageDetector.Core.Utils;
using Lykke.Service.ArbitrageDetector.Core.Domain;
using Lykke.Service.ArbitrageDetector.Core.Services;
using Lykke.Service.ArbitrageDetector.Services.Models;
using MoreLinq;

namespace Lykke.Service.ArbitrageDetector.Services
{
    public class ArbitrageDetectorService : IArbitrageDetectorService, IStartable, IStopable
    {
        private static readonly TimeSpan DefaultInterval = new TimeSpan(0, 0, 0, 2);
        private readonly ConcurrentDictionary<AssetPairSource, OrderBook> _orderBooks;
        private readonly ConcurrentDictionary<AssetPairSource, SynthOrderBook> _synthOrderBooks;
        private readonly ConcurrentDictionary<string, Arbitrage> _arbitrages;
        private readonly ConcurrentDictionary<string, Arbitrage> _arbitrageHistory;
        private bool _restartNeeded;
        private readonly TimerTrigger _trigger;
        private readonly ISettingsService _settingsService;
        private readonly ILog _log;

        public ArbitrageDetectorService(ISettingsService settingsService, ILogFactory logFactory)
        {
            _orderBooks = new ConcurrentDictionary<AssetPairSource, OrderBook>();
            _synthOrderBooks = new ConcurrentDictionary<AssetPairSource, SynthOrderBook>();
            _arbitrages = new ConcurrentDictionary<string, Arbitrage>();
            _arbitrageHistory = new ConcurrentDictionary<string, Arbitrage>();

            _settingsService = settingsService;
            _log = logFactory.CreateLog(this);

            _trigger = new TimerTrigger(nameof(ArbitrageDetectorService), DefaultInterval, logFactory, Execute);
        }


        public void Process(OrderBook orderBook)
        {
            var key = new AssetPairSource(orderBook.Source, orderBook.AssetPair);
            _orderBooks[key] = orderBook;
        }

        public async Task Execute(ITimerTrigger timer, TimerTriggeredHandlerArgs args, CancellationToken cancellationtoken)
        {
            try
            {
                await Execute();
            }
            catch (Exception ex)
            {
                _log.Error(ex);
            }
        }

        public async Task Execute()
        {
            await CalculateSynthOrderBooksAsync();
            await RefreshArbitragesAsync();

            RestartIfNeeded();
        }

        public Task<IEnumerable<SynthOrderBook>> CalculateSynthOrderBooksAsync()
        {
            var watch = Stopwatch.StartNew();

            var newActualSynthOrderBooks = new List<SynthOrderBook>();
            var orderBooks = GetWantedActualOrderBooks().Values.ToList();
            var settings = Settings();

            foreach (var @base in settings.BaseAssets)
            {
                var target = new AssetPair(@base, settings.QuoteAsset, 8, 8);
                var newActualSynthsFromAll = SynthOrderBook.GetSynthsFromAll(target, orderBooks, orderBooks);

                newActualSynthOrderBooks.AddRange(newActualSynthsFromAll);
            }

            foreach (var newSynthOrderBook in newActualSynthOrderBooks)
                _synthOrderBooks[new AssetPairSource(newSynthOrderBook.Source, newSynthOrderBook.AssetPair)] = newSynthOrderBook;

            watch.Stop();
            if (watch.ElapsedMilliseconds > 500)
                _log.Info($"{watch.ElapsedMilliseconds} ms, {_synthOrderBooks.Count} synthetic order books, {orderBooks.Count} order books.");

            return Task.FromResult(_synthOrderBooks.Select(x => x.Value));
        }

        private (IList<SynthOrderBookLine> bids, IList<SynthOrderBookLine> asks)? CalculateSynthOrderBookLines(IList<SynthOrderBook> synthOrderBooks)
        {
            // If no asks or bids then return empty list
            if (!synthOrderBooks.SelectMany(x => x.Bids).Any() || !synthOrderBooks.SelectMany(x => x.Asks).Any())
                return null;

            var settings = Settings();

            // 1. Calculate minAsk and maxBid
            var maxBid = synthOrderBooks.SelectMany(x => x.Bids).Max(x => x.Price);
            var minAsk = synthOrderBooks.SelectMany(x => x.Asks).Min(x => x.Price);

            // No arbitrages
            if (minAsk >= maxBid)
                return null;

            // 2. Collect only arbitrages lines
            var bids = new List<SynthOrderBookLine>();
            var asks = new List<SynthOrderBookLine>();
            foreach (var synthOrderBook in synthOrderBooks)
            {
                bids.AddRange(
                    synthOrderBook.Bids
                        .Where(x => x.Price > minAsk && (settings.MinimumVolume == 0 || x.Volume >= settings.MinimumVolume))
                        .Select(x => new SynthOrderBookLine(synthOrderBook, x)));

                asks.AddRange(
                    synthOrderBook.Asks
                        .Where(x => x.Price < maxBid && (settings.MinimumVolume == 0 || x.Volume >= settings.MinimumVolume))
                        .Select(x => new SynthOrderBookLine(synthOrderBook, x)));
            }

            // 3. Order by Price
            bids = bids.OrderByDescending(x => x.Price).ToList();
            asks = asks.OrderByDescending(x => x.Price).ToList();

            return (bids, asks);
        }

        public Task<Dictionary<string, Arbitrage>> CalculateArbitrages()
        {
            var newArbitrages = new Dictionary<string, Arbitrage>();
            var actualSynthOrderBooks = GetActualSynthOrderBooks();
            var settings = Settings();

            // For each asset pair
            var uniqueAssetPairs = actualSynthOrderBooks.Select(x => x.AssetPair).Distinct().ToList();
            foreach (var assetPair in uniqueAssetPairs)
            {
                var watch = Stopwatch.StartNew();

                var assetPairSynthOrderBooks = actualSynthOrderBooks.Where(x => x.AssetPair.Equals(assetPair)).ToList();

                // For each synthetic order book make a line for every ask and every bid
                var bidsAndAsks = CalculateSynthOrderBookLines(assetPairSynthOrderBooks);
                var bidsAndAsksMs = watch.ElapsedMilliseconds;

                if (!bidsAndAsks.HasValue)
                    return Task.FromResult(newArbitrages);

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

                        // Filtering by spread
                        var spread = Arbitrage.GetSpread(bidPrice, askPrice);
                        if (settings.MinSpread < 0 && spread < settings.MinSpread)
                            continue;

                        var bidVolume = bid.Volume;
                        var askVolume = ask.Volume;
                        var volume = askVolume < bidVolume ? askVolume : bidVolume;
                        // Filtering by PnL
                        var pnL = Arbitrage.GetPnL(bidPrice, askPrice, volume);
                        if (settings.MinimumPnL > 0 && pnL < settings.MinimumPnL)
                            continue;

                        var key = Arbitrage.FormatConversionPath(bid.SynthOrderBook.ConversionPath, ask.SynthOrderBook.ConversionPath);
                        if (newArbitrages.TryGetValue(key, out var existed))
                        {
                            var newpnL = Arbitrage.GetPnL(bidPrice, askPrice, volume);
                            // Best PnL
                            if (newpnL <= existed.PnL)
                                continue;

                            var arbitrage = new Arbitrage(assetPair, bid.SynthOrderBook, new VolumePrice(bid.Price, bid.Volume), ask.SynthOrderBook, new VolumePrice(ask.Price, ask.Volume));
                            newArbitrages[key] = arbitrage;
                        }
                        else
                        {
                            var arbitrage = new Arbitrage(assetPair, bid.SynthOrderBook, new VolumePrice(bid.Price, bid.Volume), ask.SynthOrderBook, new VolumePrice(ask.Price, ask.Volume));
                            newArbitrages.Add(key, arbitrage);
                        }
                    }
                }

                watch.Stop();
                if (watch.ElapsedMilliseconds > 1000)
                    _log.Info($"{watch.ElapsedMilliseconds} ms, {newArbitrages.Count} arbitrages, {actualSynthOrderBooks.Count} actual synthetic order books, {bidsAndAsksMs} ms for bids and asks, {bids.Count} bids, {asks.Count} asks, {totalItarations} iterations, {possibleArbitrages} possible arbitrages.");
            }

            return Task.FromResult(newArbitrages);
        }

        public async Task RefreshArbitragesAsync()
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
                    _arbitrages[newArbitrage.Key] = newArbitrage.Value;
                }
                // Existed
                else
                {
                    if (newArbitrage.Value.PnL > oldArbitrage.PnL)
                    {
                        removed++;
                        MoveFromActualToHistory(oldArbitrage);

                        _arbitrages[newArbitrage.Key] = newArbitrage.Value;
                    }
                }
            }

            var beforeCleaning = _arbitrageHistory.Count;
            CleanHistory();

            watch.Stop();
            if (watch.ElapsedMilliseconds - calculateArbitragesMs > 100)
                _log.Info($"{watch.ElapsedMilliseconds} ms, new {newArbitrages.Count} arbitrages, {removed} removed, {added} added, {beforeCleaning - _arbitrageHistory.Count} cleaned, {_arbitrages.Count} active, {_arbitrageHistory.Count} in history.");
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
                    .Take(Settings().HistoryMaxSize)
                    .ForEach(x => remained[x.Key] = x.Value);
            }

            _arbitrageHistory.Clear();
            _arbitrageHistory.AddRange(remained);
        }


        private Dictionary<AssetPairSource, OrderBook> GetWantedActualOrderBooks()
        {
            var result = new Dictionary<AssetPairSource, OrderBook>();
            var settings = Settings();

            foreach (var keyValue in _orderBooks)
            {
                // Filter by exchanges
                if (settings.Exchanges.Any() && !settings.Exchanges.Contains(keyValue.Key.Exchange))
                    continue;

                // Filter by base, quote and intermediate assets
                var assetPair = keyValue.Key.AssetPair;
                var passed = !settings.IntermediateAssets.Any()
                  || settings.IntermediateAssets.Contains(assetPair.Base)
                  || settings.IntermediateAssets.Contains(assetPair.Quote);
                if (!passed)
                    continue;

                // Filter by expiration time
                if (DateTime.UtcNow - keyValue.Value.Timestamp < new TimeSpan(0, 0, 0, settings.ExpirationTimeInSeconds))
                {
                    result.Add(keyValue.Key, keyValue.Value);
                }
            }

            return result;
        }

        private IList<SynthOrderBook> GetActualSynthOrderBooks()
        {
            var result = new List<SynthOrderBook>();

            foreach (var synthOrderBook in _synthOrderBooks)
            {
                if (DateTime.UtcNow - synthOrderBook.Value.Timestamp < new TimeSpan(0, 0, 0, Settings().ExpirationTimeInSeconds))
                {
                    result.Add(synthOrderBook.Value);
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
            _arbitrageHistory[key] = arbitrage;
        }

        private void RestartIfNeeded()
        {
            if (_restartNeeded)
            {
                _restartNeeded = false;

                _synthOrderBooks.Clear();
                _arbitrages.Clear();
                _arbitrageHistory.Clear();

                _log.Info("Restarted softly.");
            }
        }

        private decimal? GetPriceWithAccuracy(decimal? price, AssetPair assetPair)
        {
            if (!price.HasValue)
                return null;

            return Math.Round(price.Value, assetPair.Accuracy == 0 ? 6 : assetPair.Accuracy);
        }

        private decimal? GetVolumeWithAccuracy(decimal? volume, AssetPair assetPair)
        {
            if (!volume.HasValue)
                return null;

            return Math.Round(volume.Value, assetPair.InvertedAccuracy == 0 ? 6 : assetPair.InvertedAccuracy);
        }

        private Settings Settings()
        {
            return _settingsService.GetAsync().GetAwaiter().GetResult();
        }


        #region IArbitrageDetectorService

        public IEnumerable<OrderBook> GetOrderBooks(string exchange, string assetPair)
        {
            if (!_orderBooks.Any())
                return new List<OrderBook>();

            var result = _orderBooks.Select(x => x.Value).ToList();

            if (!string.IsNullOrWhiteSpace(exchange))
                result = result.Where(x => x.Source.ToUpper().Trim().Contains(exchange.ToUpper().Trim())).ToList();

            if (!string.IsNullOrWhiteSpace(assetPair))
                result = result.Where(x => x.AssetPair.Name.ToUpper().Trim().Contains(assetPair.ToUpper().Trim())).ToList();

            return result.OrderByDescending(x => x.AssetPair.Name).ToList();
        }

        public OrderBook GetOrderBook(string exchange, string assetPair)
        {
            if (string.IsNullOrWhiteSpace(exchange))
                throw new ArgumentException($"{nameof(exchange)} must be set.");

            if (string.IsNullOrWhiteSpace(assetPair))
                throw new ArgumentException($"{nameof(assetPair)} must be set.");

            if (!_orderBooks.Any())
                return null;

            var allOrderBooks = _orderBooks.Values;
            
            var result = allOrderBooks.SingleOrDefault(x => x.Source.Equals(exchange, StringComparison.OrdinalIgnoreCase)
                                                  && x.AssetPair.Name.Equals(assetPair, StringComparison.OrdinalIgnoreCase));

            return result;
        }

        public IEnumerable<SynthOrderBook> GetSynthOrderBooks()
        {
            if (!_synthOrderBooks.Any())
                return new List<SynthOrderBook>();

            var result = _synthOrderBooks.Select(x => x.Value)
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

        public Matrix GetMatrix(string assetPair, bool isPublic = false, bool depositFee = false, bool tradingFee = false)
        {
            if (string.IsNullOrWhiteSpace(assetPair))
                return null;

            var result = new Matrix(assetPair);
            var s = Settings();

            // Filter by asset pair
            var orderBooks = _orderBooks.Values.Where(x => 
                string.Equals(x.AssetPair.Name, assetPair.Replace("/", ""), StringComparison.OrdinalIgnoreCase)).ToList();

            // Filter by exchanges
            if (isPublic && s.PublicMatrixExchanges.Any())
                orderBooks = orderBooks.Where(x => s.PublicMatrixExchanges.Keys.Contains(x.Source)).ToList();

            // Order by exchange name
            orderBooks = orderBooks.OrderBy(x => x.Source).ToList();

            // Fees
            var exchangesFees = new List<ExchangeFees>();
            foreach (var orderBook in orderBooks)
            {
                var exchangeFees = s.ExchangesFees.SingleOrDefault(x => x.ExchangeName.Equals(orderBook.Source, StringComparison.OrdinalIgnoreCase))
                                   ?? new ExchangeFees { ExchangeName = orderBook.Source }; // deposit and trading fees = 0 by default
                exchangesFees.Add(exchangeFees);
            }

            // Order books with fees
            var useFees = depositFee || tradingFee;
            var orderBooksWithFees = useFees ? new List<OrderBook>() : null;
            if (useFees)
            {
                // Put fees into prices
                foreach (var orderBook in orderBooks)
                {
                    var exchangeFees = exchangesFees.Single(x => x.ExchangeName == orderBook.Source);
                    var totalFee = (depositFee ? exchangeFees.DepositFee : 0) + (tradingFee ? exchangeFees.TradingFee : 0);
                    var orderBookWithFees = orderBook.DeepClone(totalFee);
                    orderBooksWithFees.Add(orderBookWithFees);
                }
                orderBooks = orderBooksWithFees;
            }

            var exchangesNames = orderBooks.Select(x => x.Source).ToList();

            // Raplace exchange names
            if (isPublic && s.PublicMatrixExchanges.Any())
                exchangesNames = exchangesNames.Select(x => x.Replace(x, s.PublicMatrixExchanges[x])).ToList();

            var matrixSide = exchangesNames.Count;
            for (var row = 0; row < matrixSide; row++)
            {
                var orderBookRow = orderBooks[row];
                var cellsRow = new List<MatrixCell>();
                var isActual = (DateTime.UtcNow - orderBookRow.Timestamp).TotalSeconds < s.ExpirationTimeInSeconds;
                var assetPairObj = orderBookRow.AssetPair;

                // Add ask and exchange
                var exchangeName = orderBookRow.Source;
                var exchangeFees = exchangesFees.Single(x => x.ExchangeName == exchangeName);
                result.Exchanges.Add(new Exchange(exchangeName, isActual, exchangeFees));
                result.Asks.Add(GetPriceWithAccuracy(orderBookRow.BestAsk?.Price, assetPairObj));

                for (var col = 0; col < matrixSide; col++)
                {
                    var orderBookCol = orderBooks[col];

                    // Add bid
                    if (row == 0)
                        result.Bids.Add(GetPriceWithAccuracy(orderBookCol.BestBid?.Price, assetPairObj));

                    // If the same exchanges than cell = null
                    MatrixCell cell;
                    if (row == col)
                    {
                        cellsRow.Add(null);
                        continue;
                    }

                    // If current cell doesn't have prices on one or both sides.
                    if (orderBookCol.BestBid == null || orderBookRow.BestAsk == null)
                    {
                        cell = new MatrixCell(null, null);
                        cellsRow.Add(cell);
                        continue;
                    }

                    var spread = Arbitrage.GetSpread(orderBookCol.BestBid.Value.Price, orderBookRow.BestAsk.Value.Price);
                    spread = Math.Round(spread, 2);
                    decimal? volume = null;
                    if (spread < 0)
                    {
                        volume = Arbitrage.GetArbitrageVolumePnL(orderBookCol.Bids, orderBookRow.Asks)?.Volume;
                        volume = GetVolumeWithAccuracy(volume, assetPairObj);
                    }

                    cell = new MatrixCell(spread, volume);
                    cellsRow.Add(cell);
                }

                // row ends
                result.Cells.Add(cellsRow);
            }

            result.DateTime = DateTime.UtcNow;

            return result;
        }

        #endregion

        #region IStartable, IStopable

        public void Start()
        {
            _trigger.Start();
        }

        public void Stop()
        {
            _trigger.Stop();
        }

        public void Dispose()
        {
            _trigger?.Dispose();
        }

        #endregion
    }
}
