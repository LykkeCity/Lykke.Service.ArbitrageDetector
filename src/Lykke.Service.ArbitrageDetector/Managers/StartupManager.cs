using System.Threading.Tasks;
using Autofac;
using Lykke.Sdk;
using Lykke.Service.ArbitrageDetector.Core.Services;
using Lykke.Service.ArbitrageDetector.RabbitMq.Subscribers;

namespace Lykke.Service.ArbitrageDetector.Managers
{
    internal class StartupManager : IStartupManager
    {
        private readonly OrderBooksSubscriber _orderBooksSubscriber;
        private readonly IArbitrageDetectorService _arbitrageDetectorService;
        private readonly ILykkeArbitrageDetectorService _lykkeArbitrageDetectorService;
        private readonly IMatrixHistoryService _matrixHistoryService;

        public StartupManager(OrderBooksSubscriber orderBooksSubscriber,
            IArbitrageDetectorService arbitrageDetectorService,
            ILykkeArbitrageDetectorService lykkeArbitrageDetectorService,
            IMatrixHistoryService matrixHistoryService)
        {
            _orderBooksSubscriber = orderBooksSubscriber;
            _arbitrageDetectorService = arbitrageDetectorService;
            _lykkeArbitrageDetectorService = lykkeArbitrageDetectorService;
            _matrixHistoryService = matrixHistoryService;
        }

        public Task StartAsync()
        {
            _orderBooksSubscriber.Start();
            ((IStartable)_arbitrageDetectorService).Start();
            ((IStartable)_lykkeArbitrageDetectorService).Start();
            ((IStartable)_matrixHistoryService).Start();

            return Task.CompletedTask;
        }
    }
}
