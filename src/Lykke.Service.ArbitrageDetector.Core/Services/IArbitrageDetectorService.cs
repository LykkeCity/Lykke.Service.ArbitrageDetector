using System;
using System.Collections.Generic;
using Lykke.Service.ArbitrageDetector.Core.Domain;

namespace Lykke.Service.ArbitrageDetector.Core.Services
{
    public interface IArbitrageDetectorService
    {
        void Process(OrderBook orderBook);

        IEnumerable<OrderBook> GetOrderBooks();

        IEnumerable<OrderBook> GetOrderBooks(string exchange, string instrument);

        IEnumerable<CrossRate> GetCrossRates();

        IEnumerable<Arbitrage> GetArbitrages();

        IEnumerable<Arbitrage> GetArbitrageHistory(DateTime since, int take);
    }
}
