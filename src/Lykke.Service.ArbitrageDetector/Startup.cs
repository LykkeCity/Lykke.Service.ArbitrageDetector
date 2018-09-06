using JetBrains.Annotations;
using Lykke.Sdk;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using System;
using Lykke.Common.Api.Contract.Responses;
using Lykke.Service.ArbitrageDetector.Settings;
using AutoMapper;

namespace Lykke.Service.ArbitrageDetector
{
    [UsedImplicitly]
    public class Startup
    {
        private readonly LykkeSwaggerOptions _swaggerOptions = new LykkeSwaggerOptions
        {
            ApiTitle = "ArbitrageDetector API",
            ApiVersion = "v1"
        };

        [UsedImplicitly]
        public IServiceProvider ConfigureServices(IServiceCollection services)
        {
            services.AddResponseCaching();

            return services.BuildServiceProvider<AppSettings>(options =>
            {
                options.SwaggerOptions = _swaggerOptions;

                options.Logs = logs =>
                {
                    logs.AzureTableName = "ArbitrageDetectorLog";
                    logs.AzureTableConnectionStringResolver = settings =>
                        settings.ArbitrageDetector.Db.LogsConnectionString;
                };

                Mapper.Initialize(cfg =>
                {
                    cfg.AddProfiles(typeof(AutoMapperProfile));
                });
                Mapper.AssertConfigurationIsValid();
            });
        }

        [UsedImplicitly]
        public void Configure(IApplicationBuilder app)
        {
            app.UseLykkeConfiguration(options =>
            {
                app.UseResponseCaching();

                options.SwaggerOptions = _swaggerOptions;
                options.DefaultErrorHandler = exception => ErrorResponse.Create(exception.Message);
            });
        }
    }
}
