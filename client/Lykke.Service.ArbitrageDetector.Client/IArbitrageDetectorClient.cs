using System.Collections.Generic;
using System.Threading.Tasks;
using Lykke.Service.ArbitrageDetector.Client.Models;

namespace Lykke.Service.ArbitrageDetector.Client
{
    public interface IArbitrageDetectorClient
    {
        Task<IEnumerable<OrderBookModel>> GetOrderBooks();

        Task<IEnumerable<CrossRateModel>> GetCrossRates();

        Task<IEnumerable<ArbitrageModel>> GetArbitrages();
    }
}
