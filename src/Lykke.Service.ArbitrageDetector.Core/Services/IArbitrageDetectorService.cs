using System.Collections.Generic;
using Lykke.Service.ArbitrageDetector.Core.Domain;

namespace Lykke.Service.ArbitrageDetector.Core.Services
{
    public interface IArbitrageDetectorService
    {
        void Process(OrderBook orderBook);

        IEnumerable<OrderBook> GetOrderBooks();

        IEnumerable<CrossRate> GetCrossRates();

        IEnumerable<Arbitrage> GetArbitrages();
    }
}
