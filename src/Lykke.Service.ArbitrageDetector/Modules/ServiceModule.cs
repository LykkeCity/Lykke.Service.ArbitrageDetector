using System;
using Autofac;
using AzureStorage.Tables;
using Common;
using Common.Log;
using Lykke.Service.ArbitrageDetector.AzureRepositories;
using Lykke.Service.ArbitrageDetector.Core.Repositories;
using Lykke.Service.ArbitrageDetector.Core.Services;
using Lykke.Service.ArbitrageDetector.RabbitSubscribers;
using Lykke.Service.ArbitrageDetector.RabbitSubscribers.OrderBookHandlers;
using Lykke.Service.ArbitrageDetector.Services;
using Lykke.Service.ArbitrageDetector.Settings;
using Lykke.Service.Assets.Client;
using Lykke.Service.RateCalculator.Client;
using Lykke.SettingsReader;

namespace Lykke.Service.ArbitrageDetector.Modules
{
    public class ServiceModule : Module
    {
        private readonly IReloadingManager<AppSettings> _settings;
        private readonly ILog _log;

        public ServiceModule(IReloadingManager<AppSettings> settings, ILog log)
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

            builder.RegisterType<OrderBookParser>()
                .As<OrderBookParser>()
                .SingleInstance();

            builder.RegisterType<OrderBookValidator>()
                .As<OrderBookValidator>()
                .SingleInstance();

            builder.RegisterType<OrderBookLykkeAssetsProvider>()
                .As<OrderBookLykkeAssetsProvider>()
                .SingleInstance();

            // Services and Handlers

            builder.RegisterType<ArbitrageDetectorService>()
                .As<IArbitrageDetectorService>()
                .AutoActivate()
                .As<IStartable>()
                .As<IStopable>()
                .AutoActivate()
                .SingleInstance();

            //builder.RegisterType<ArbitrageScreenerService>()
            //    .As<ArbitrageScreenerService>()
            //    .As<IStartable>()
            //    .As<IStopable>()
            //    .AutoActivate()
            //    .SingleInstance();

            foreach (var exchange in _settings.CurrentValue.ArbitrageDetector.RabbitMq.Exchanges)
            {
                builder.RegisterType<RabbitMessageSubscriber>()
                    .As<IStartable>()
                    .As<IStopable>()
                    .WithParameter("connectionString", _settings.CurrentValue.ArbitrageDetector.RabbitMq.ConnectionString)
                    .WithParameter("exchangeName", exchange)
                    .AutoActivate()
                    .SingleInstance();
            }

            builder.RegisterInstance(new AssetsService(new Uri(_settings.CurrentValue.AssetsServiceClient.ServiceUrl)))
                .As<IAssetsService>()
                .SingleInstance();

            builder.RegisterInstance(new RateCalculatorClient(_settings.CurrentValue.RateCalculatorServiceClient.ServiceUrl, _log))
                .As<IRateCalculatorClient>()
                .SingleInstance();

            //  Repositories

            var settingsRepository = new SettingsRepository(
                AzureTableStorage<AzureRepositories.Settings>.Create(
                    _settings.ConnectionString(x => x.ArbitrageDetector.Db.DataConnectionString),
                    nameof(AzureRepositories.Settings), _log));
            builder.RegisterInstance<ISettingsRepository>(settingsRepository).PropertiesAutowired();
        }
    }
}
