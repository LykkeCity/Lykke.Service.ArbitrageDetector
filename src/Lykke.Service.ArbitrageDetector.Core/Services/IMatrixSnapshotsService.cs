using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Lykke.Service.ArbitrageDetector.Core.Domain;

namespace Lykke.Service.ArbitrageDetector.Core.Services
{
    public interface IMatrixSnapshotsService
    {
        Task<IEnumerable<Matrix>> GetByAssetPairAndDateAsync(string assetPair, DateTime date);

        Task<IEnumerable<Matrix>> GetDateTimesOnlyByAssetPairAndDateAsync(string assetPair, DateTime date);
    }
}
