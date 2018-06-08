using System;
using Autofac;
using Autofac.Extras.DynamicProxy;
using AzureStorage.Tables;
using Common;
using Common.Log;
using Lykke.Service.ArbitrageDetector.Aspects.Cache;
using Lykke.Service.ArbitrageDetector.AzureRepositories;
using Lykke.Service.ArbitrageDetector.Controllers;
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
    public class ServicesModule : Module
    {
        private readonly IReloadingManager<AppSettings> _settings;
        private readonly ILog _log;

        public ServicesModule(IReloadingManager<AppSettings> settings, ILog log)
        {
            _settings = settings;
            _log = log;
        }

        protected override void Load(ContainerBuilder builder)
        {
            // Order Book Handlers

            builder.RegisterType<OrderBookParser>()
                .SingleInstance();

            builder.RegisterType<OrderBookValidator>()
                .SingleInstance();

            builder.RegisterType<OrderBookLykkeAssetsProvider>()
                .SingleInstance();

            // Services

            builder.RegisterType<ArbitrageDetectorService>()
                .As<IArbitrageDetectorService>()
                .As<IStartable>()
                .As<IStopable>()
                .AutoActivate()
                .SingleInstance();

            //builder.RegisterType<ArbitrageScreenerService>()
            //    .As<IStartable>()
            //    .As<IStopable>()
            //    .AutoActivate()
            //    .SingleInstance();

            builder.RegisterInstance(new AssetsService(new Uri(_settings.CurrentValue.AssetsServiceClient.ServiceUrl)))
                .As<IAssetsService>()
                .SingleInstance();

            builder.RegisterInstance(new RateCalculatorClient(_settings.CurrentValue.RateCalculatorServiceClient.ServiceUrl, _log))
                .As<IRateCalculatorClient>()
                .SingleInstance();

            // RabbitMessageSubscribers

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
        }
    }
}
