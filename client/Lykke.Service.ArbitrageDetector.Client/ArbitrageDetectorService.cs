﻿using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Lykke.Service.ArbitrageDetector.Client.Api;
using Lykke.Service.ArbitrageDetector.Client.Models;
using Microsoft.Extensions.PlatformAbstractions;
using Refit;

namespace Lykke.Service.ArbitrageDetector.Client
{
    /// <summary>
    /// Contains methods for work with arbitrage detector service.
    /// </summary>
    public sealed class ArbitrageDetectorService : IArbitrageDetectorService, IDisposable
    {
        private readonly HttpClient _httpClient;
        private readonly IArbitrageDetectorApi _arbitrageDetectorApi;
        private readonly ApiRunner _runner;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="url"></param>
        public ArbitrageDetectorService(string url) : this(new ArbitrageDetectorServiceClientSettings(url))
        {
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="settings"></param>
        public ArbitrageDetectorService(ArbitrageDetectorServiceClientSettings settings)
        {
            if (settings == null)
                throw new ArgumentNullException(nameof(settings));

            if (string.IsNullOrEmpty(settings.ServiceUrl))
                throw new ArgumentException("Service URL Required");

            _httpClient = new HttpClient
            { 
                BaseAddress = new Uri(settings.ServiceUrl),
                DefaultRequestHeaders =
                {
                    {
                        "User-Agent",
                        $"{PlatformServices.Default.Application.ApplicationName}/{PlatformServices.Default.Application.ApplicationVersion}"
                    }
                }
            };

            _arbitrageDetectorApi = RestService.For<IArbitrageDetectorApi>(_httpClient);

            _runner = new ApiRunner();
        }

        /// <inheritdoc />
        public async Task<IEnumerable<OrderBookRow>> OrderBooksAsync(string exchange, string assetPair)
        {
            return await _runner.RunAsync(() => _arbitrageDetectorApi.OrderBooksAsync(exchange, assetPair));
        }

        /// <inheritdoc />
        public async Task<OrderBook> OrderBookAsync(string exchange, string assetPair)
        {
            return await _runner.RunAsync(() => _arbitrageDetectorApi.OrderBookAsync(exchange, assetPair));
        }

        /// <inheritdoc />
        public async Task<IEnumerable<SynthOrderBookRow>> SynthOrderBooksAsync()
        {
            return await _runner.RunAsync(() => _arbitrageDetectorApi.SynthOrderBooksAsync());
        }

        /// <inheritdoc />
        public async Task<IEnumerable<ArbitrageRow>> ArbitragesAsync()
        {
            return await _runner.RunAsync(() => _arbitrageDetectorApi.ArbitragesAsync());
        }

        /// <inheritdoc />
        public async Task<Arbitrage> ArbitrageFromHistoryAsync(string conversionPath)
        {
            return await _runner.RunAsync(() => _arbitrageDetectorApi.ArbitrageFromHistoryAsync(conversionPath));
        }

        /// <inheritdoc />
        public async Task<Arbitrage> ArbitrageFromActiveOrHistoryAsync(string conversionPath)
        {
            return await _runner.RunAsync(() => _arbitrageDetectorApi.ArbitrageFromActiveOrHistoryAsync(conversionPath));
        }

        /// <inheritdoc />
        public async Task<IEnumerable<ArbitrageRow>> ArbitrageHistoryAsync(DateTime since, int take)
        {
            return await _runner.RunAsync(() => _arbitrageDetectorApi.ArbitrageHistory(since, take));
        }

        /// <inheritdoc />
        public async Task<Matrix> MatrixAsync(string assetPair, bool depositFee = false, bool tradingFee = false)
        {
            return await _runner.RunAsync(() => _arbitrageDetectorApi.Matrix(assetPair, depositFee, tradingFee));
        }

        /// <inheritdoc />
        public async Task<Matrix> PublicMatrixAsync(string assetPair, bool depositFee = false, bool tradingFee = false)
        {
            return await _runner.RunAsync(() => _arbitrageDetectorApi.PublicMatrix(assetPair, depositFee, tradingFee));
        }

        /// <inheritdoc />
        public async Task<IEnumerable<string>> PublicMatrixAssetPairsAsync()
        {
            return await _runner.RunAsync(() => _arbitrageDetectorApi.PublicMatrixAssetPairs());
        }

        /// <inheritdoc />
        public async Task<IEnumerable<LykkeArbitrageRow>> LykkeArbitragesAsync(string basePair, string crossPair, string target = "", string source = "", ArbitrageProperty property = default, decimal minValue = 0)
        {
            return await _runner.RunAsync(() => _arbitrageDetectorApi.LykkeArbitrages(basePair, crossPair, target, source, property, minValue));
        }

        /// <inheritdoc />
        public async Task<IEnumerable<DateTime>> MatrixHistoryStamps(string assetPair, DateTime date, bool lykkeArbitragesOnly)
        {
            return await _runner.RunAsync(() => _arbitrageDetectorApi.MatrixHistoryStamps(assetPair, date, lykkeArbitragesOnly));
        }

        /// <inheritdoc />
        public async Task<IEnumerable<string>> MatrixHistoryAssetPairs(DateTime date, bool lykkeArbitragesOnly)
        {
            return await _runner.RunAsync(() => _arbitrageDetectorApi.MatrixHistoryAssetPairs(date, lykkeArbitragesOnly));
        }

        /// <inheritdoc />
        public async Task<Matrix> MatrixHistory(string assetPair, DateTime dateTime)
        {
            return await _runner.RunAsync(() => _arbitrageDetectorApi.MatrixHistory(assetPair, dateTime));
        }

        /// <inheritdoc />
        public async Task<Settings> GetSettingsAsync()
        {
            return await _runner.RunAsync(() => _arbitrageDetectorApi.GetSettings());
        }

        /// <inheritdoc />
        public async Task SetSettingsAsync(Settings settings)
        {
            await _runner.RunAsync(() => _arbitrageDetectorApi.SetSettings(settings));
        }

        /// <inheritdoc />
        public void Dispose()
        {
            _httpClient?.Dispose();
        }
    }
}
