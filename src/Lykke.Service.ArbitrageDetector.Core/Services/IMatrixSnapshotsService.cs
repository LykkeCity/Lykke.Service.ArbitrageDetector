using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Lykke.Service.ArbitrageDetector.Core.Domain.Interfaces;

namespace Lykke.Service.ArbitrageDetector.Core.Services
{
    public interface IMatrixSnapshotsService
    {
        Task<IEnumerable<IMatrix>> GetByAssetPairAndDateAsync(string assetPair, DateTime date);

        Task<IEnumerable<IMatrix>> GetDateTimesOnlyByAssetPairAndDateAsync(string assetPair, DateTime date);
    }
}
