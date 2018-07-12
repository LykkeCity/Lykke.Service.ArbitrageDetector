using Autofac;
using AzureStorage.Blob;
using AzureStorage.Tables;
using Common.Log;
using Lykke.Service.ArbitrageDetector.AzureRepositories.Models;
using Lykke.Service.ArbitrageDetector.AzureRepositories.Repositories;
using Lykke.Service.ArbitrageDetector.Core.Repositories;
using Lykke.Service.ArbitrageDetector.Settings;
using Lykke.SettingsReader;

namespace Lykke.Service.ArbitrageDetector.Modules
{
    public class RepositoriesModule : Module
    {
        private readonly IReloadingManager<AppSettings> _settings;
        private readonly ILog _log;

        public RepositoriesModule(IReloadingManager<AppSettings> settings, ILog log)
        {
            _settings = settings;
            _log = log;
        }

        protected override void Load(ContainerBuilder builder)
        {
            var connectionString = _settings.ConnectionString(x => x.ArbitrageDetector.Db.DataConnectionString);

            // Blob

            builder.RegisterInstance(AzureBlobStorage.Create(connectionString));
            builder.RegisterType<MatrixHistoryBlobRepository>();

            // Table

            builder.RegisterInstance(AzureTableStorage<AzureRepositories.Models.Settings>.Create(connectionString, nameof(AzureRepositories.Models.Settings), _log));
            builder.RegisterType<SettingsRepository>().As<ISettingsRepository>();

            builder.RegisterInstance(AzureTableStorage<MatrixEntity>.Create(connectionString, nameof(MatrixEntity), _log));
            builder.RegisterType<MatrixHistoryRepository>().As<IMatrixHistoryRepository>();
        }
    }
}
