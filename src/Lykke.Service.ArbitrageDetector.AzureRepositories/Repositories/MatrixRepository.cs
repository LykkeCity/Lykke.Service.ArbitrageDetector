using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AzureStorage;
using Lykke.Service.ArbitrageDetector.AzureRepositories.Models;
using Lykke.Service.ArbitrageDetector.Core.Domain;
using Lykke.Service.ArbitrageDetector.Core.Repositories;

namespace Lykke.Service.ArbitrageDetector.AzureRepositories.Repositories
{
    public class MatrixRepository : IMatrixRepository
    {
        private readonly IMatrixBlobRepository _blobRepository;
        private readonly INoSQLTableStorage<MatrixReference> _storage;

        public MatrixRepository(IMatrixBlobRepository blobRepository, INoSQLTableStorage<MatrixReference> storage)
        {
            _blobRepository = blobRepository ?? throw new ArgumentNullException(nameof(blobRepository));
            _storage = storage ?? throw new ArgumentNullException(nameof(storage));
        }

        public async Task<Matrix> GetAsync(string assetPair, DateTime dateTime)
        {
            return await _blobRepository.GetAsync(assetPair, dateTime);
        }

        public async Task<IEnumerable<Matrix>> GetByAssetPairAndDateAsync(string assetPair, DateTime date)
        {
            throw new NotImplementedException();
        }

        public async Task<IEnumerable<Matrix>> GetDateTimesOnlyByAssetPairAndDateAsync(string assetPair, DateTime date)
        {
            throw new NotImplementedException();
        }

        public async Task InsertAsync(Matrix matrix)
        {
            await _storage.InsertAsync(new MatrixReference(matrix));
            await _blobRepository.SaveAsync(matrix);
        }

        public async Task<bool> DeleteAsync(string assetPair, DateTime dateTime)
        {
            return await _storage.DeleteIfExistAsync(assetPair, dateTime.Ticks.ToString());
        }
    }
}
