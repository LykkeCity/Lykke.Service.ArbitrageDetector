using Autofac;
using Autofac.Extras.DynamicProxy;
using Lykke.Service.ArbitrageDetector.Aspects.Cache;
using Lykke.Service.ArbitrageDetector.Aspects.ExceptionHandling;
using Lykke.Service.ArbitrageDetector.Controllers;

namespace Lykke.Service.ArbitrageDetector.Modules
{
    public class ControllersModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<ArbitrageDetectorController>()
                .EnableClassInterceptors()
                //.InterceptedBy(typeof(CacheInterceptor))
                .InterceptedBy(typeof(ExceptionToBadRequestInterceptor))
                .InstancePerLifetimeScope();
        }
    }
}
