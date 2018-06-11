using Autofac;
using AzureStorage.Tables;
using Common.Log;
using Lykke.Service.ArbitrageDetector.AzureRepositories;
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
                AzureTableStorage<AzureRepositories.Settings>.Create(
                    _settings.ConnectionString(x => x.ArbitrageDetector.Db.DataConnectionString),
                    nameof(AzureRepositories.Settings), _log));
            builder.RegisterInstance<ISettingsRepository>(settingsRepository).PropertiesAutowired();
        }
    }
}
