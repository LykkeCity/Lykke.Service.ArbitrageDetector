﻿using Autofac;
using Common.Log;
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

            builder.RegisterType<OrderBookProcessor>()
                .As<IOrderBookProcessor>()
                .SingleInstance();

            builder.RegisterType<ArbitrageDetectorService>()
                .As<IArbitrageDetectorService>()
                .WithParameter("wantedCurrencies", _settings.CurrentValue.WantedCurrencies)
                .WithParameter("baseCurrency", _settings.CurrentValue.BaseCurrency)
                .WithParameter("executionDelay", _settings.CurrentValue.ArbitrageDetectorExecutionDelayInSeconds)
                .WithParameter("expirationTimeInSeconds", _settings.CurrentValue.ExpirationTimeInSeconds)
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
