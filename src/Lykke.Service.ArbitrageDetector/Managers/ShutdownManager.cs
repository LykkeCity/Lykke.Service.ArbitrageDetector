using System.Threading.Tasks;
using Common;
using Lykke.Sdk;
using Lykke.Service.ArbitrageDetector.Core.Services;
using Lykke.Service.ArbitrageDetector.RabbitSubscribers;

namespace Lykke.Service.ArbitrageDetector.Managers
{
    internal class ShutdownManager : IShutdownManager
    {
        private readonly RabbitMessageSubscriber[] _rabbitMessageSubscribers;
        private readonly IArbitrageDetectorService _arbitrageDetectorService;
        private readonly ILykkeArbitrageDetectorService _lykkeArbitrageDetectorService;
        private readonly IMatrixHistoryService _matrixHistoryService;

        public ShutdownManager(RabbitMessageSubscriber[] rabbitMessageSubscribers,
            IArbitrageDetectorService arbitrageDetectorService,
            ILykkeArbitrageDetectorService lykkeArbitrageDetectorService,
            IMatrixHistoryService matrixHistoryService)
        {
            _rabbitMessageSubscribers = rabbitMessageSubscribers;
            _arbitrageDetectorService = arbitrageDetectorService;
            _lykkeArbitrageDetectorService = lykkeArbitrageDetectorService;
            _matrixHistoryService = matrixHistoryService;
        }

        public Task StopAsync()
        {
            foreach (var rabbitMessageSubscriber in _rabbitMessageSubscribers)
                rabbitMessageSubscriber.Stop();

            ((IStopable)_arbitrageDetectorService).Stop();
            ((IStopable)_lykkeArbitrageDetectorService).Stop();
            ((IStopable)_matrixHistoryService).Stop();

            return Task.CompletedTask;
        }
    }
}
