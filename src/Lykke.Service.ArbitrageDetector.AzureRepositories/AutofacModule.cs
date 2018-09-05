using Autofac;
using AzureStorage.Blob;
using AzureStorage.Tables;
using JetBrains.Annotations;
using Lykke.Common.Log;
using Lykke.Service.ArbitrageDetector.AzureRepositories.Repositories;
using Lykke.Service.ArbitrageDetector.Core.Repositories;
using Lykke.SettingsReader;

namespace Lykke.Service.ArbitrageDetector.AzureRepositories
{
    [UsedImplicitly]
    public class AutofacModule : Module
    {
        private readonly IReloadingManager<string> _connectionString;

        public AutofacModule(IReloadingManager<string> connectionString)
        {
            _connectionString = connectionString;
        }

        protected override void Load(ContainerBuilder builder)
        {
            // Blob

            builder.RegisterInstance(AzureBlobStorage.Create(_connectionString));
            builder.RegisterType<MatrixHistoryBlobRepository>();

            // Table

            builder.Register(container => AzureTableStorage<Models.Settings>.Create(_connectionString,
                nameof(Models.Settings), container.Resolve<ILogFactory>()));
            builder.RegisterType<SettingsRepository>().As<ISettingsRepository>();

            builder.RegisterType<MatrixHistoryRepository>()
                .As<IMatrixHistoryRepository>()
                .WithParameter("connectionString", _connectionString);
        }
    }
}
