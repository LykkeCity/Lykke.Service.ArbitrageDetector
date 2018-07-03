using Autofac;
using Common.Log;
using Lykke.Service.ArbitrageDetector.Core.Services.Infrastructure;
using Lykke.Service.ArbitrageDetector.Services;
using Lykke.Service.ArbitrageDetector.Services.Infrastructure;

namespace Lykke.Service.ArbitrageDetector.Modules
{
    public class CommonModule : Module
    {
        private readonly ILog _log;

        public CommonModule(ILog log)
        {
            _log = log;
        }

        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterInstance(_log)
                .As<ILog>()
                .SingleInstance();

            builder.RegisterType<HealthService>()
                .As<IHealthService>()
                .SingleInstance();

            builder.RegisterType<StartupManager>()
                .As<IStartupManager>();

            builder.RegisterType<ShutdownManager>()
                .As<IShutdownManager>();
        }
    }
}
