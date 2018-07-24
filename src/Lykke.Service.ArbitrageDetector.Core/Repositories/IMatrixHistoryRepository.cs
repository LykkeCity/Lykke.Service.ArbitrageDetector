using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Lykke.Service.ArbitrageDetector.Core.Domain;

namespace Lykke.Service.ArbitrageDetector.Core.Repositories
{
    public interface IMatrixHistoryRepository
    {
        Task<Matrix> GetAsync(string assetPair, DateTime dateTime);

        Task<IEnumerable<DateTime>> GetDateTimeStampsAsync(string assetPair, DateTime date, decimal? maxSpread, IReadOnlyCollection<string> exchanges);

        Task<IEnumerable<DateTime>> GetDateTimeStampsAsync(string assetPair, DateTime from, DateTime to, decimal? maxSpread, IReadOnlyCollection<string> exchanges);

        Task<IEnumerable<string>> GetAssetPairsAsync(DateTime date, decimal? maxSpread, IReadOnlyCollection<string> exchanges);

        Task InsertAsync(Matrix matrix);

        Task<bool> DeleteAsync(string assetPair, DateTime dateTime);
    }
}
