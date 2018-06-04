using Autofac;
using AzureStorage.Tables;
using Common;
using Common.Log;
using Lykke.Service.ArbitrageDetector.AzureRepositories;
using Lykke.Service.ArbitrageDetector.Core.Repositories;
using Lykke.Service.ArbitrageDetector.Core.Services;
using Lykke.Service.ArbitrageDetector.RabbitSubscribers;
using Lykke.Service.ArbitrageDetector.Services;
using Lykke.Service.ArbitrageDetector.Settings.ServiceSettings;
using Lykke.SettingsReader;

namespace Lykke.Service.ArbitrageDetector.Modules
{
    public class ServiceModule : Module
    {
        private readonly IReloadingManager<ArbitrageDetectorSettings> _settings;
        private readonly ILog _log;

        public ServiceModule(IReloadingManager<ArbitrageDetectorSettings> settings, ILog log)
        {
            _settings = settings;
            _log = log;
        }

        protected override void Load(ContainerBuilder builder)
        {
            // Common

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

            // Services and Handlers

            builder.RegisterType<OrderBookProcessor>()
                .As<IOrderBookProcessor>()
                .SingleInstance();

            builder.RegisterType<ArbitrageDetectorService>()
                .As<IArbitrageDetectorService>()
                .AutoActivate()
                .As<IStartable>()
                .As<IStopable>()
                .SingleInstance();

            foreach (var exchange in _settings.CurrentValue.RabbitMq.Exchanges)
            {
                builder.RegisterType<RabbitMessageSubscriber>()
                    .As<IStartable>()
                    .As<IStopable>()
                    .WithParameter("connectionString", _settings.CurrentValue.RabbitMq.ConnectionString)
                    .WithParameter("exchangeName", exchange)
                    .AutoActivate()
                    .SingleInstance();
            }

            //  Repositories

            var settingsRepository = new SettingsRepository(
                AzureTableStorage<AzureRepositories.Settings>.Create(
                    _settings.ConnectionString(x => x.Db.DataConnectionString),
                    nameof(AzureRepositories.Settings), _log));
            builder.RegisterInstance<ISettingsRepository>(settingsRepository).PropertiesAutowired();
        }
    }
}
