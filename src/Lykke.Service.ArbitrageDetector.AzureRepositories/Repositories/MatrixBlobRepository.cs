using System;
using System.Threading.Tasks;
using AzureStorage;
using Common;
using Lykke.Service.ArbitrageDetector.AzureRepositories.Models;
using Lykke.Service.ArbitrageDetector.Core.Domain;
using Lykke.Service.ArbitrageDetector.Core.Repositories;

namespace Lykke.Service.ArbitrageDetector.AzureRepositories.Repositories
{
    public class MatrixBlobRepository : BaseBlobRepository, IMatrixBlobRepository
    {
        public MatrixBlobRepository(IBlobStorage storage) : base(storage, nameof(Matrix))
        {
        }

        public Task SaveAsync(Matrix matrix)
        {
            return SaveBlobAsync(MatrixReference.GenerateBlobId(matrix), matrix.ToJson());
        }

        public Task<Matrix> GetAsync(string assetPair, DateTime dateTime)
        {
            var blobId = MatrixReference.GenerateBlobId(assetPair, dateTime);
            return Task.Run(() =>  GetBlobStringAsync(blobId).GetAwaiter().GetResult().DeserializeJson<Matrix>());
        }

        public async Task DeleteIfExistsAsync(string assetPair, DateTime dateTime)
        {
            var blobId = MatrixReference.GenerateBlobId(assetPair, dateTime);
            if (await BlobExistsAsync(blobId))
                await DeleteBlobAsync(blobId);
        }
    }
}
