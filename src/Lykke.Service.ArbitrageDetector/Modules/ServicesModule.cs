using System;
using Autofac;
using AzureStorage.Tables;
using Common;
using Common.Log;
using Lykke.Service.ArbitrageDetector.AzureRepositories.Models;
using Lykke.Service.ArbitrageDetector.AzureRepositories.Repositories;
using Lykke.Service.ArbitrageDetector.Core.Services;
using Lykke.Service.ArbitrageDetector.OrderBookHandlers;
using Lykke.Service.ArbitrageDetector.RabbitSubscribers;
using Lykke.Service.ArbitrageDetector.Services;
using Lykke.Service.ArbitrageDetector.Settings;
using Lykke.Service.RateCalculator.Client;
using Lykke.SettingsReader;
using ILykkeAssetsService = Lykke.Service.Assets.Client.IAssetsService;
using LykkeAssetsService = Lykke.Service.Assets.Client.AssetsService;
using IAssetsService = Lykke.Service.ArbitrageDetector.Core.Services.IAssetsService;
using AssetsService = Lykke.Service.ArbitrageDetector.Services.AssetsService;

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
            // TODO: must be moved into particular service after Common.TimerPeriod change
            var settingsRepository = new SettingsRepository(
                AzureTableStorage<AzureRepositories.Models.Settings>.Create(
                    _settings.ConnectionString(x => x.ArbitrageDetector.Db.DataConnectionString), nameof(AzureRepositories.Models.Settings), _log));
            var dbSetings = settingsRepository.GetAsync().GetAwaiter().GetResult();

            // Order Book Handlers

            builder.RegisterType<OrderBookParser>()
                .SingleInstance();

            builder.RegisterType<OrderBookValidator>()
                .SingleInstance();

            // Services

            builder.RegisterType<AssetsService>()
                .As<IAssetsService>()
                .SingleInstance();

            builder.RegisterType<ArbitrageDetectorService>()
                .As<IArbitrageDetectorService>()
                .As<IStartable>()
                .As<IStopable>()
                .SingleInstance();

            //builder.RegisterType<ArbitrageScreenerService>()
            //    .As<IStartable>()
            //    .As<IStopable>()
            //    .AutoActivate()
            //    .SingleInstance();

            builder.RegisterType<LykkeArbitrageDetectorService>()
                .As<ILykkeArbitrageDetectorService>()
                .As<IStartable>()
                .As<IStopable>()
                .SingleInstance();

            builder.RegisterInstance(new LykkeAssetsService(new Uri(_settings.CurrentValue.AssetsServiceClient.ServiceUrl)))
                .As<ILykkeAssetsService>()
                .SingleInstance();

            builder.RegisterInstance(new RateCalculatorClient(_settings.CurrentValue.RateCalculatorServiceClient.ServiceUrl, _log))
                .As<IRateCalculatorClient>()
                .SingleInstance();

            builder.RegisterType<MatrixHistoryService>()
                .As<IMatrixHistoryService>()
                .WithParameter("interval", dbSetings.MatrixHistoryInterval)
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
