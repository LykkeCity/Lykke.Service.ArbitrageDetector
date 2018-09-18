using System.Threading.Tasks;
using Lykke.Service.ArbitrageDetector.Core.Domain;

namespace Lykke.Service.ArbitrageDetector.Core.Handlers
{
    public interface IOrderBookHandler
    {
        Task HandleAsync(OrderBook orderBook);
    }
}
