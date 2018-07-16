using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Lykke.Service.ArbitrageDetector.Core.Domain;

namespace Lykke.Service.ArbitrageDetector.Core.Services
{
    public interface IMatrixHistoryService
    {
        Task<IEnumerable<DateTime>> GetStampsAsync(string assetPair, DateTime date, bool arbitragesOnly);

        Task<IEnumerable<string>> GetAssetPairsAsync(DateTime date, bool arbitragesOnly);

        Task<Matrix> GetAsync(string assetPair, DateTime date);
    }
}
