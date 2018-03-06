using System.Collections.Generic;
using System.Threading.Tasks;
using Lykke.Service.ArbitrageDetector.Client.Models;
using Refit;

namespace Lykke.Service.ArbitrageDetector.Client.Api
{
    internal interface IArbitrageDetectorApi
    {
        [Get("/api/orderBooks")]
        Task<IReadOnlyList<OrderBook>> GetOrderBooksAsync();

        [Get("/api/crossRates")]
        Task<IReadOnlyList<CrossRate>> GetCrossRatesAsync();

        [Get("/api/arbitrages")]
        Task<IReadOnlyList<Arbitrage>> GetArbitragesAsync();
    }
}
