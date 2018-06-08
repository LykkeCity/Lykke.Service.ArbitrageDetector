using Autofac;
using Lykke.Service.ArbitrageDetector.Aspects.Cache;
using Lykke.Service.ArbitrageDetector.Aspects.ExceptionHandling;

namespace Lykke.Service.ArbitrageDetector.Modules
{
    public class AspectsModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            // Cache

            builder.RegisterType<MemoryCacheProvider>()
                .As<ICacheProvider>()
                .SingleInstance();

            builder.RegisterType<CacheInterceptor>()
                .SingleInstance();

            // ExceptionLogging

            builder.RegisterType<ExceptionToBadRequestInterceptor>()
                .SingleInstance();
        }
    }
}
