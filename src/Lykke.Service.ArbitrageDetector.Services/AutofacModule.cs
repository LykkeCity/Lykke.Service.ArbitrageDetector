using Autofac;
using Common;
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
        }
    }
}
