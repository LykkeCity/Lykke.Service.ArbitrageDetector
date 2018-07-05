using System;
using System.Globalization;
using System.Threading.Tasks;
using AzureStorage;
using Lykke.Service.ArbitrageDetector.AzureRepositories.Models;
using Lykke.Service.ArbitrageDetector.Core.Domain.Interfaces;
using Lykke.Service.ArbitrageDetector.Core.Repositories;

namespace Lykke.Service.ArbitrageDetector.AzureRepositories.Repositories
{
    public class MatrixRepository : IMatrixRepository
    {
        private readonly INoSQLTableStorage<Matrix> _storage;

        public MatrixRepository(INoSQLTableStorage<Matrix> storage)
        {
            _storage = storage;
        }

        public async Task<IMatrix> GetAsync(string assetPair, DateTime dateTime)
        {
            return await _storage.GetDataAsync(assetPair, dateTime.ToString(CultureInfo.InvariantCulture));
        }

        public async Task InsertOrReplaceAsync(IMatrix matrix)
        {
            await _storage.InsertOrReplaceAsync(new Matrix(matrix));
        }

        public async Task<bool> DeleteAsync(string assetPair, DateTime dateTime)
        {
            return await _storage.DeleteIfExistAsync(assetPair, dateTime.ToString(CultureInfo.InvariantCulture));
        }
    }
}
