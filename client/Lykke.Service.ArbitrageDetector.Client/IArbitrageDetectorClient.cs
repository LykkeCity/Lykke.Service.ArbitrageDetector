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
        /// Returns a collection of OrderBook entities by exchange name.
        /// </summary>
        /// <param name="exchange">A name of an exchange.</param>
        /// <returns>A collection of OrderBook entities.</returns>
        Task<IReadOnlyList<OrderBook>> GetOrderBooksByExchangeAsync(string exchange);

        /// <summary>
        /// Returns a collection of OrderBook entities by instrument name.
        /// </summary>
        /// <param name="instrument">A name of an instrument.</param>
        /// <returns>A collection of OrderBook entities.</returns>
        Task<IReadOnlyList<OrderBook>> GetOrderBooksByInstrumentAsync(string instrument);

        /// <summary>
        /// Returns a collection of OrderBook entities by exchange and instrument.
        /// </summary>
        /// <param name="exchange">A name of an exchange.</param>
        /// <param name="instrument">A name of an instrument</param>
        /// <returns>A collection of OrderBook entities.</returns>
        Task<IReadOnlyList<OrderBook>> GetOrderBooksAsync(string exchange, string instrument);

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
