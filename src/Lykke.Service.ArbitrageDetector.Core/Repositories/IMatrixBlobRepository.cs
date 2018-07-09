using System;
using System.Threading.Tasks;
using Lykke.Service.ArbitrageDetector.Core.Domain;

namespace Lykke.Service.ArbitrageDetector.Core.Repositories
{
    public interface IMatrixBlobRepository
    {
        Task SaveAsync(Matrix matrix);

        Task<Matrix> GetAsync(string assetPair, DateTime dateTime);

        Task DeleteIfExistsAsync(string assetPair, DateTime dateTime);
    }
}
