using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Lykke.Service.ArbitrageDetector.Core.Domain;

namespace Lykke.Service.ArbitrageDetector.Core.Services
{
    public interface IMatrixSnapshotsService
    {
        Task<Matrix> GetAsync(string assetPair, DateTime date);

        Task<IEnumerable<DateTime>> GetDateTimeStampsAsync(string assetPair, DateTime date);

        Task<IEnumerable<DateTime>> GetDateTimeStampsAsync(string assetPair, DateTime from, DateTime to);

        Task<IEnumerable<string>> GetAssetPairsAsync(DateTime date);
    }
}
