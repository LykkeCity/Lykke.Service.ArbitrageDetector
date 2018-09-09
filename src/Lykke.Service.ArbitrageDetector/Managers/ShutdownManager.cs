using System.Threading.Tasks;
using Common;
using Lykke.Sdk;

namespace Lykke.Service.ArbitrageDetector.Managers
{
    internal class ShutdownManager : IShutdownManager
    {
        private readonly IStopable[] _stopables;

        public ShutdownManager(IStopable[] stopables)
        {
            _stopables = stopables;
        }

        public Task StopAsync()
        {
            foreach (var stopable in _stopables)
                stopable.Stop();

            return Task.CompletedTask;
        }
    }
}
