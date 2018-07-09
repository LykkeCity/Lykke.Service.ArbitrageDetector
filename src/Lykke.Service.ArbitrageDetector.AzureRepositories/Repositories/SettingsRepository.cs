﻿using System.Threading.Tasks;
using AzureStorage;
using Lykke.Service.ArbitrageDetector.AzureRepositories.Models;
using Lykke.Service.ArbitrageDetector.Core.Domain.Interfaces;
using Lykke.Service.ArbitrageDetector.Core.Repositories;

namespace Lykke.Service.ArbitrageDetector.AzureRepositories.Repositories
{
    public class SettingsRepository : ISettingsRepository
    {
        private readonly INoSQLTableStorage<Settings> _storage;

        public SettingsRepository(INoSQLTableStorage<Settings> storage)
        {
            _storage = storage;
        }

        public async Task<ISettings> GetAsync()
        {
            return await _storage.GetDataAsync("", "");
        }

        public async Task InsertOrReplaceAsync(ISettings settings)
        {
            await _storage.InsertOrReplaceAsync(new Settings(settings));
        }

        public async Task<bool> DeleteAsync()
        {
            return await _storage.DeleteIfExistAsync("", "");
        }
    }
}