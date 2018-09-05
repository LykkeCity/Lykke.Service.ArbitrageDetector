using System.Threading.Tasks;
using Autofac;
using Lykke.Sdk;
using Lykke.Service.ArbitrageDetector.Core.Services;
using Lykke.Service.ArbitrageDetector.RabbitSubscribers;

namespace Lykke.Service.ArbitrageDetector.Managers
{
    internal class StartupManager : IStartupManager
    {
        private readonly RabbitMessageSubscriber _rabbitMessageSubscriber;
        private readonly IArbitrageDetectorService _arbitrageDetectorService;
        private readonly ILykkeArbitrageDetectorService _lykkeArbitrageDetectorService;
        private readonly IMatrixHistoryService _matrixHistoryService;

        public StartupManager(RabbitMessageSubscriber rabbitMessageSubscriber,
            IArbitrageDetectorService arbitrageDetectorService,
            ILykkeArbitrageDetectorService lykkeArbitrageDetectorService,
            IMatrixHistoryService matrixHistoryService)
        {
            _rabbitMessageSubscriber = rabbitMessageSubscriber;
            _arbitrageDetectorService = arbitrageDetectorService;
            _lykkeArbitrageDetectorService = lykkeArbitrageDetectorService;
            _matrixHistoryService = matrixHistoryService;
        }

        public Task StartAsync()
        {
            _rabbitMessageSubscriber.Start();
            ((IStartable)_arbitrageDetectorService).Start();
            ((IStartable)_lykkeArbitrageDetectorService).Start();
            ((IStartable)_matrixHistoryService).Start();

            return Task.CompletedTask;
        }
    }
}
