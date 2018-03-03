using System.Collections.Generic;
using Lykke.Service.ArbitrageDetector.Core.Domain;

namespace Lykke.Service.ArbitrageDetector.Core.Services
{
    public interface IArbitrageDetectorService
    {
        void Process(OrderBook orderBook);

        IEnumerable<Arbitrage> GetArbitrages();

        IEnumerable<string> GetArbitragesStrings();

        IDictionary<ExchangeAssetPair, OrderBook> GetOrderBooks();

        IDictionary<ExchangeAssetPair, CrossRate> GetCrossRates();
    }
}
