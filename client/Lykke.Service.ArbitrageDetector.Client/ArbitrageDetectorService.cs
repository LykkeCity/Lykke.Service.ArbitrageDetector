using System;
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

        public ArbitrageDetectorService(string url) : this(new ArbitrageDetectorServiceClientSettings(url))
        {
        }

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

        public async Task<IEnumerable<OrderBook>> OrderBooksAsync(string exchange, string assetPair)
        {
            return await _runner.RunAsync(() => _arbitrageDetectorApi.OrderBooksAsync(exchange, assetPair));
        }

        public async Task<IEnumerable<CrossRateRow>> CrossRatesAsync()
        {
            return await _runner.RunAsync(() => _arbitrageDetectorApi.CrossRatesAsync());
        }

        public async Task<IEnumerable<ArbitrageRow>> ArbitragesAsync()
        {
            return await _runner.RunAsync(() => _arbitrageDetectorApi.ArbitragesAsync());
        }

        public async Task<Arbitrage> ArbitrageFromHistoryAsync(string conversionPath)
        {
            return await _runner.RunAsync(() => _arbitrageDetectorApi.ArbitrageFromHistoryAsync(conversionPath));
        }

        public async Task<Arbitrage> ArbitrageFromActiveOrHistoryAsync(string conversionPath)
        {
            return await _runner.RunAsync(() => _arbitrageDetectorApi.ArbitrageFromActiveOrHistoryAsync(conversionPath));
        }

        public async Task<IEnumerable<ArbitrageRow>> ArbitrageHistoryAsync(DateTime since, int take)
        {
            return await _runner.RunAsync(() => _arbitrageDetectorApi.ArbitrageHistory(since, take));
        }

        public async Task<Settings> GetSettingsAsync()
        {
            return await _runner.RunAsync(() => _arbitrageDetectorApi.GetSettings());
        }

        public async Task SetSettingsAsync(Settings settings)
        {
            await _runner.RunAsync(() => _arbitrageDetectorApi.SetSettings(settings));
        }

        public void Dispose()
        {
            _httpClient?.Dispose();
        }
    }
}
