using Lykke.Service.ArbitrageDetector.Core.Domain;

namespace Lykke.Service.ArbitrageDetector.Core.Services
{
    public interface IArbitrageCalculator
    {
        void Process(OrderBook orderBook);
    }
}
