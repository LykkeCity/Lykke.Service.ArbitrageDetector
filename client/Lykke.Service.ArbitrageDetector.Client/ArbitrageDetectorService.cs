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
    public class ArbitrageDetectorService : IArbitrageDetectorService, IDisposable
    {
        private readonly HttpClient _httpClient;
        private readonly IArbitrageDetectorApi _arbitrageDetectorApi;
        private readonly ApiRunner _runner;

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

        public async Task<IReadOnlyList<OrderBook>> OrderBooksAsync(string exchange, string instrument)
        {
            return await _runner.RunAsync(() => _arbitrageDetectorApi.OrderBooksAsync(exchange, instrument));
        }

        public async Task<IReadOnlyList<CrossRate>> CrossRatesAsync()
        {
            return await _runner.RunAsync(() => _arbitrageDetectorApi.CrossRatesAsync());
        }

        public async Task<IReadOnlyList<Arbitrage>> ArbitragesAsync()
        {
            return await _runner.RunAsync(() => _arbitrageDetectorApi.ArbitragesAsync());
        }

        public async Task<IReadOnlyList<ArbitrageHistory>> ArbitrageHistoryAsync(DateTime since)
        {
            return await _runner.RunAsync(() => _arbitrageDetectorApi.ArbitrageHistory(since));
        }

        public void Dispose()
        {
            _httpClient?.Dispose();
        }
    }
}
