using Autofac;
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
            var settingsRepository = new SettingsRepository(
                AzureTableStorage<AzureRepositories.Models.Settings>.Create(
                    _settings.ConnectionString(x => x.ArbitrageDetector.Db.DataConnectionString),
                    nameof(AzureRepositories.Models.Settings), _log));
            builder.RegisterInstance<ISettingsRepository>(settingsRepository).PropertiesAutowired();

            var matrixRepository = new MatrixRepository(
                AzureTableStorage<Matrix>.Create(
                    _settings.ConnectionString(x => x.ArbitrageDetector.Db.DataConnectionString),
                    nameof(Matrix), _log));
            builder.RegisterInstance<IMatrixRepository>(matrixRepository).PropertiesAutowired();
        }
    }
}
