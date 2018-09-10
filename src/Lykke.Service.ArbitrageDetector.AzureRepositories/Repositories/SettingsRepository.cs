using System.Threading.Tasks;
using AutoMapper;
using AzureStorage;
using Lykke.Service.ArbitrageDetector.AzureRepositories.Models;
using Lykke.Service.ArbitrageDetector.Core.Repositories;
using DomainSettings = Lykke.Service.ArbitrageDetector.Core.Domain.Settings;

namespace Lykke.Service.ArbitrageDetector.AzureRepositories.Repositories
{
    public class SettingsRepository : ISettingsRepository
    {
        private readonly INoSQLTableStorage<Settings> _storage;

        public SettingsRepository(INoSQLTableStorage<Settings> storage)
        {
            _storage = storage;
        }

        public async Task<DomainSettings> GetAsync()
        {
            var settings = await _storage.GetDataAsync("", "");
            var domain = Mapper.Map<DomainSettings>(settings);

            return domain;
        }

        public async Task InsertOrReplaceAsync(DomainSettings settings)
        {
            var model = Mapper.Map<Settings>(settings);
            model.PartitionKey = "";
            model.RowKey = "";

            await _storage.InsertOrReplaceAsync(model);
        }

        public async Task<bool> DeleteAsync()
        {
            return await _storage.DeleteIfExistAsync("", "");
        }
    }
}
