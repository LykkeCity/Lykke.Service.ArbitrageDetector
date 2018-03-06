using System.Collections.Generic;
using System.Threading.Tasks;
using Lykke.Service.ArbitrageDetector.Client.Models;

namespace Lykke.Service.ArbitrageDetector.Client
{
    /// <summary>
    /// HTTP client for arbitrage detector service.
    /// </summary>
    public interface IArbitrageDetectorClient
    {
        /// <summary>
        /// Returns a collection of OrderBook entities.
        /// </summary>
        /// <returns>A collection of OrderBook entities.</returns>
        Task<IReadOnlyList<OrderBook>> GetOrderBooksAsync();

        /// <summary>
        /// Returns a collection of CrossRate entities.
        /// </summary>
        /// <returns>A collection of CrossRate entities.</returns>
        Task<IReadOnlyList<CrossRate>> GetCrossRatesAsync();

        /// <summary>
        /// Returns a collection of Arbitrage entities.
        /// </summary>
        /// <returns>A collection of Arbitrage entities.</returns>
        Task<IReadOnlyList<Arbitrage>> GetArbitragesAsync();
    }
}
