using System;
using System.Collections.Generic;
using Lykke.Service.ArbitrageDetector.Core.Domain;
using DataCrossRate = Lykke.Service.ArbitrageDetector.Core.DataModel.CrossRate;
using DataArbitrage = Lykke.Service.ArbitrageDetector.Core.DataModel.Arbitrage;

namespace Lykke.Service.ArbitrageDetector.Core.Services
{
    public interface IArbitrageDetectorService
    {
        void Process(OrderBook orderBook);

        IEnumerable<OrderBook> GetOrderBooks();

        IEnumerable<OrderBook> GetOrderBooks(string exchange, string instrument);

        IEnumerable<DataCrossRate> GetCrossRates();

        IEnumerable<Arbitrage> GetArbitrages();

        IEnumerable<DataArbitrage> GetArbitragesData();

        IEnumerable<DataArbitrage> GetArbitrageHistory(DateTime since, int take);
    }
}
