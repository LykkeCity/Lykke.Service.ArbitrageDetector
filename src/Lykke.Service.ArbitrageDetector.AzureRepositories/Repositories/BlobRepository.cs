using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using AzureStorage;

namespace Lykke.Service.ArbitrageDetector.AzureRepositories.Repositories
{
    public abstract class BlobRepository
    {
        private string _container;
        private readonly IBlobStorage _storage;

        protected BlobRepository(IBlobStorage storage, string container)
        {
            _container = container;
            _storage = storage;

            storage.CreateContainerIfNotExistsAsync(_container).GetAwaiter().GetResult();
        }

        protected async Task SaveBlobAsync(string blobKey, byte[] blobData)
        {
            //TODO: additional request can be bad for performance, need to change AzureBlobStorage -> createIfNotExists
            if (await BlobExistsAsync(blobKey))
                throw new InvalidOperationException($"Blob is already existed, id: {blobKey}");

            await _storage.SaveBlobAsync(_container, blobKey, blobData);
        }

        protected async Task SaveBlobAsync(string blobKey, string blobString)
        {
            //TODO: additional request can be bad for performance, need to change AzureBlobStorage -> createIfNotExists
            if (await BlobExistsAsync(blobKey))
                throw new InvalidOperationException($"Blob is already existed, id: {blobKey}");

            await _storage.SaveBlobAsync(_container, blobKey, Encoding.UTF8.GetBytes(blobString));
        }

        protected async Task<bool> BlobExistsAsync(string blobKey)
        {
            return await _storage.HasBlobAsync(_container, blobKey);
        }

        protected async Task<byte[]> GetBlobAsync(string blobKey)
        {
            var stream = await _storage.GetAsync(_container, blobKey);
            using (var ms = new MemoryStream())
            {
                stream.CopyTo(ms);
                return ms.ToArray();
            }
        }

        protected async Task<string> GetBlobStringAsync(string blobKey)
        {
            return await _storage.GetAsTextAsync(_container, blobKey);
        }

        protected async Task DeleteBlobAsync(string blobKey)
        {
            await _storage.DelBlobAsync(_container, blobKey);
        }
    }
}
