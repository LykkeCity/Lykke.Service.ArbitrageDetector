using System;
using System.Threading.Tasks;
using Lykke.Service.ArbitrageDetector.Core.Domain.Interfaces;

namespace Lykke.Service.ArbitrageDetector.Core.Repositories
{
    public interface IMatrixRepository
    {
        Task<IMatrix> GetAsync(string assetPair, DateTime dateTime);

        Task InsertOrReplaceAsync(IMatrix matrix);

        Task<bool> DeleteAsync(string assetPair, DateTime dateTime);
    }
}
