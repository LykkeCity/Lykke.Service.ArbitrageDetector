using System.Collections.Generic;
using System.Threading.Tasks;
using Lykke.Service.ArbitrageDetector.Client.AutorestClient.Models;


namespace Lykke.Service.ArbitrageDetector.Client
{
    public interface IArbitrageDetectorClient
    {
        Task<IEnumerable<OrderBook>> GetOrderBooks();

        Task<IEnumerable<CrossRate>> GetCrossRates();

        Task<IEnumerable<Arbitrage>> GetArbitrages();
    }
}
