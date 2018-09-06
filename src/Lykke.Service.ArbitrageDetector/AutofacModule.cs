using System;
using Autofac;
using Common;
using JetBrains.Annotations;
using Lykke.Job.OrderBooksCacheProvider.Client;
using Lykke.Sdk;
using Lykke.Service.ArbitrageDetector.Managers;
using Lykke.Service.ArbitrageDetector.RabbitMq.Subscribers;
using Lykke.Service.Assets.Client;
using Lykke.Service.ArbitrageDetector.Settings;
using Lykke.SettingsReader;

namespace Lykke.Service.ArbitrageDetector
{
    [UsedImplicitly]
    public class AutofacModule : Module
    {
        private readonly IReloadingManager<AppSettings> _settings;

        public AutofacModule(IReloadingManager<AppSettings> settings)
        {
            _settings = settings;
        }

        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterModule(new AzureRepositories.AutofacModule(
                _settings.Nested(o => o.ArbitrageDetector.Db.DataConnectionString)));

            builder.RegisterModule(new Services.AutofacModule());

            builder.RegisterType<StartupManager>()
                .As<IStartupManager>();

            builder.RegisterType<ShutdownManager>()
                .As<IShutdownManager>();

            RegisterRabbit(builder);

            RegisterClients(builder);
        }

        private void RegisterRabbit(ContainerBuilder builder)
        {
            foreach (var exchange in _settings.CurrentValue.ArbitrageDetector.RabbitMq.Exchanges)
            {
                builder.RegisterType<OrderBooksSubscriber>()
                    .AsSelf()
                    .As<IStartable>()
                    .As<IStopable>()
                    .WithParameter("connectionString", _settings.CurrentValue.ArbitrageDetector.RabbitMq.ConnectionString)
                    .WithParameter("exchangeName", exchange)
                    .AutoActivate()
                    .SingleInstance();
            }
        }

        private void RegisterClients(ContainerBuilder builder)
        {
            builder.RegisterInstance(new AssetsService(new Uri(_settings.CurrentValue.AssetsServiceClient.ServiceUrl)))
                .As<IAssetsService>()
                .SingleInstance();

            builder.RegisterInstance(new OrderBookProviderClient(_settings.CurrentValue.OrderBooksCacheProviderClient.ServiceUrl))
                .As<IOrderBookProviderClient>()
                .SingleInstance();
        }
    }
}
