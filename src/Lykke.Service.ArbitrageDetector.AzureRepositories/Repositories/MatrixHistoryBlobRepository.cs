using System;
using System.Threading.Tasks;
using AzureStorage;
using Common;
using Lykke.Service.ArbitrageDetector.AzureRepositories.Models;

namespace Lykke.Service.ArbitrageDetector.AzureRepositories.Repositories
{
    public class MatrixHistoryBlobRepository : BlobRepository
    {
        public MatrixHistoryBlobRepository(IBlobStorage storage) : base(storage, nameof(MatrixBlob))
        {
        }

        public Task SaveAsync(MatrixBlob matrix)
        {
            return SaveBlobAsync(MatrixEntity.GenerateBlobId(matrix.Matrix()), matrix.ToJson());
        }

        public async Task<MatrixBlob> GetAsync(string assetPair, DateTime dateTime)
        {
            var blobId = MatrixEntity.GenerateBlobId(assetPair, dateTime);
            if (!await BlobExistsAsync(blobId))
                return null;

            return (await GetBlobStringAsync(blobId)).DeserializeJson<MatrixBlob>();
        }

        public async Task DeleteIfExistsAsync(string assetPair, DateTime dateTime)
        {
            var blobId = MatrixEntity.GenerateBlobId(assetPair, dateTime);
            if (await BlobExistsAsync(blobId))
                await DeleteBlobAsync(blobId);
        }
    }
}
