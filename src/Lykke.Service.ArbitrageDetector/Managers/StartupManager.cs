using System.Threading.Tasks;
using Autofac;
using Lykke.Sdk;

namespace Lykke.Service.ArbitrageDetector.Managers
{
    internal class StartupManager : IStartupManager
    {
        private readonly IStartable[] _startables;

        public StartupManager(IStartable[] startables)
        {
            _startables = startables;
        }

        public Task StartAsync()
        {
            foreach (var startable in _startables)
            {
                startable.Start();
            }

            return Task.CompletedTask;
        }
    }
}
