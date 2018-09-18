using Autofac;
using Common;
using Lykke.Service.ArbitrageDetector.Core.Handlers;
using Lykke.Service.ArbitrageDetector.Core.Services;

namespace Lykke.Service.ArbitrageDetector.Services
{
    public class AutofacModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            RegisterOrderBooks(builder);
        }

        private void RegisterOrderBooks(ContainerBuilder builder)
        {
            builder.RegisterType<SettingsService>()
                .As<ISettingsService>()
                .SingleInstance();

            builder.RegisterType<OrderBooksService>()
                .As<IOrderBooksService>()
                .As<IOrderBookHandler>()
                .As<IStartable>()
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
        }
    }
}
