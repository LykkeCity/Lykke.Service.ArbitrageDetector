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

        public async Task<IReadOnlyList<OrderBook>> GetOrderBooksAsync()
        {
            return await _runner.RunAsync(() => _arbitrageDetectorApi.GetOrderBooksAsync());
        }

        public async Task<IReadOnlyList<OrderBook>> GetOrderBooksByExchangeAsync(string exchange)
        {
            return await _runner.RunAsync(() => _arbitrageDetectorApi.GetOrderBooksByExchangeAsync(exchange));
        }

        public async Task<IReadOnlyList<OrderBook>> GetOrderBooksByInstrumentAsync(string instrument)
        {
            return await _runner.RunAsync(() => _arbitrageDetectorApi.GetOrderBooksByInstrumentAsync(instrument));
        }

        public async Task<IReadOnlyList<OrderBook>> GetOrderBooksAsync(string exchange, string instrument)
        {
            return await _runner.RunAsync(() => _arbitrageDetectorApi.GetOrderBooksAsync(exchange, instrument));
        }

        public async Task<IReadOnlyList<CrossRate>> GetCrossRatesAsync()
        {
            return await _runner.RunAsync(() => _arbitrageDetectorApi.GetCrossRatesAsync());
        }

        public async Task<IReadOnlyList<Arbitrage>> GetArbitragesAsync()
        {
            return await _runner.RunAsync(() => _arbitrageDetectorApi.GetArbitragesAsync());
        }

        public void Dispose()
        {
            _httpClient?.Dispose();
        }
    }
}
