using System.Collections.Generic;
using Lykke.Service.ArbitrageDetector.Core.Domain;

namespace Lykke.Service.ArbitrageDetector.Core.Services
{
    public interface IArbitrageDetectorService
    {
        void Process(OrderBook orderBook);


        IEnumerable<OrderBook> GetOrderBooks();

        IEnumerable<OrderBook> GetOrderBooksByExchange(string exchange);

        IEnumerable<OrderBook> GetOrderBooksByInstrument(string instrument);

        IEnumerable<OrderBook> GetOrderBooks(string exchange, string instrument);


        IEnumerable<CrossRate> GetCrossRates();


        IEnumerable<Arbitrage> GetArbitrages();
    }
}
