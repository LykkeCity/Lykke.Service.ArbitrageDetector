using System.Threading.Tasks;
using Common;
using Lykke.Sdk;
using Lykke.Service.ArbitrageDetector.Core.Services;
using Lykke.Service.ArbitrageDetector.RabbitMq.Subscribers;

namespace Lykke.Service.ArbitrageDetector.Managers
{
    internal class ShutdownManager : IShutdownManager
    {
        private readonly OrderBooksSubscriber[] _orderBooksSubscribers;
        private readonly IArbitrageDetectorService _arbitrageDetectorService;
        private readonly ILykkeArbitrageDetectorService _lykkeArbitrageDetectorService;
        private readonly IMatrixHistoryService _matrixHistoryService;

        public ShutdownManager(OrderBooksSubscriber[] orderBooksSubscribers,
            IArbitrageDetectorService arbitrageDetectorService,
            ILykkeArbitrageDetectorService lykkeArbitrageDetectorService,
            IMatrixHistoryService matrixHistoryService)
        {
            _orderBooksSubscribers = orderBooksSubscribers;
            _arbitrageDetectorService = arbitrageDetectorService;
            _lykkeArbitrageDetectorService = lykkeArbitrageDetectorService;
            _matrixHistoryService = matrixHistoryService;
        }

        public Task StopAsync()
        {
            foreach (var rabbitMessageSubscriber in _orderBooksSubscribers)
                rabbitMessageSubscriber.Stop();

            ((IStopable)_arbitrageDetectorService).Stop();
            ((IStopable)_lykkeArbitrageDetectorService).Stop();
            ((IStopable)_matrixHistoryService).Stop();

            return Task.CompletedTask;
        }
    }
}
