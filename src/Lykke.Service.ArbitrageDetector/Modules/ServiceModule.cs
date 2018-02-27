using Autofac;
using Common.Log;
using Lykke.Service.ArbitrageDetector.Core.Services;
using Lykke.Service.ArbitrageDetector.RabbitSubscribers;
using Lykke.Service.ArbitrageDetector.Settings.ServiceSettings;
using Lykke.Service.ArbitrageDetector.Services;
using Lykke.Service.ArbitrageDetector.Settings;
using Lykke.SettingsReader;

namespace Lykke.Service.ArbitrageDetector.Modules
{
    public class ServiceModule : Module
    {
        private readonly IReloadingManager<ArbitrageDetectorSettings> _settings;
        private readonly ILog _log;
        private readonly IConsole _console;

        public ServiceModule(IReloadingManager<ArbitrageDetectorSettings> settings, ILog log, IConsole console)
        {
            _settings = settings;
            _log = log;
            _console = console;
        }

        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterInstance(_log)
                .As<ILog>()
                .SingleInstance();

            builder.RegisterInstance(_console)
                .As<IConsole>()
                .SingleInstance();

            builder.RegisterType<HealthService>()
                .As<IHealthService>()
                .SingleInstance();

            builder.RegisterType<StartupManager>()
                .As<IStartupManager>();

            builder.RegisterType<ShutdownManager>()
                .As<IShutdownManager>();

            builder.RegisterType<ArbitrageCalculator>()
                .As<IArbitrageCalculator>()
                .WithParameter("wantedCurrencies", _settings.CurrentValue.WantedCurrencies)
                .WithParameter("executionDelay", _settings.CurrentValue.ArbitrageDetectorExecutionDelayInSeconds)
                .As<IStartable>()
                .AutoActivate()
                .SingleInstance();

            builder.RegisterType<OrderBookProcessor>()
                .As<IOrderBookProcessor>()
                .As<IStartable>()
                .AutoActivate()
                .SingleInstance();

            builder.RegisterType<RabbitMessageSubscriber>()
                .As<IStartable>()
                .AutoActivate()
                .SingleInstance()
                .WithParameter("connectionString", _settings.CurrentValue.RabbitMq.ConnectionString)
                .WithParameter("exchangeName", _settings.CurrentValue.RabbitMq.Exchange);
        }
    }
}
