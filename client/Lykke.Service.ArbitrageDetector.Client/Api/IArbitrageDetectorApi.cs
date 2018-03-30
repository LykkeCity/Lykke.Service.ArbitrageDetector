using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Lykke.Service.ArbitrageDetector.Client.Models;
using Refit;

namespace Lykke.Service.ArbitrageDetector.Client.Api
{
    internal interface IArbitrageDetectorApi
    {
        [Get("/orderBooks")]
        Task<IEnumerable<OrderBook>> OrderBooksAsync(string exchange, string instrument);

        [Get("/crossRates")]
        Task<IEnumerable<CrossRate>> CrossRatesAsync();

        [Get("/arbitrages")]
        Task<IEnumerable<Arbitrage>> ArbitragesAsync();

        [Get("/arbitrageHistory")]
        Task<IEnumerable<Arbitrage>> ArbitrageHistory(DateTime since, int take);

        [Get("/getSettings")]
        Task<Settings> GetSettings();

        [Post("/setSettings")]
        Task SetSettings(Settings settings);
    }
}
