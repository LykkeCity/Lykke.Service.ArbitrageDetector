using System;
using System.Linq;
using Autofac;
using Common;
using Common.Log;
using Lykke.Job.OrderBooksCacheProvider.Client;
using Lykke.Service.ArbitrageDetector.Core.Services;
using Lykke.Service.ArbitrageDetector.OrderBookHandlers;
using Lykke.Service.ArbitrageDetector.RabbitSubscribers;
using Lykke.Service.ArbitrageDetector.Services;
using Lykke.Service.ArbitrageDetector.Settings;
using Lykke.Service.RateCalculator.Client;
using Lykke.SettingsReader;
using ILykkeAssetsService = Lykke.Service.Assets.Client.IAssetsService;
using LykkeAssetsService = Lykke.Service.Assets.Client.AssetsService;

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
            // OutProc Services

            builder.RegisterInstance(new LykkeAssetsService(new Uri(_settings.CurrentValue.AssetsServiceClient.ServiceUrl)))
                .As<ILykkeAssetsService>()
                .SingleInstance();

            builder.RegisterInstance(new OrderBookProviderClient(_settings.CurrentValue.OrderBooksCacheProviderClient.ServiceUrl))
                .As<IOrderBookProviderClient>()
                .SingleInstance();

            // Order Book Handlers

            builder.RegisterType<OrderBookParser>()
                .SingleInstance();

            builder.RegisterType<OrderBookValidator>()
                .SingleInstance();

            // InProc Services

            builder.RegisterType<LykkeExchangeService>()
                .As<ILykkeExchangeService>()
                .As<IStartable>()
                .PropertiesAutowired(PropertyWiringOptions.AllowCircularDependencies)
                .SingleInstance();

            builder.RegisterType<ArbitrageDetectorService>()
                .As<IArbitrageDetectorService>()
                .As<IStartable>()
                .As<IStopable>()
                .SingleInstance();

            builder.RegisterType<LykkeArbitrageDetectorService>()
                .As<ILykkeArbitrageDetectorService>()
                .As<IStartable>()
                .As<IStopable>()
                .SingleInstance();

            builder.RegisterType<MatrixHistoryService>()
                .As<IMatrixHistoryService>()
                .As<IStartable>()
                .As<IStopable>()
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
