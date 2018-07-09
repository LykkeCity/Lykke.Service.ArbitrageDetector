﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Lykke.Service.ArbitrageDetector.Core.Domain.Interfaces;

namespace Lykke.Service.ArbitrageDetector.Core.Repositories
{
    public interface IMatrixRepository
    {
        Task<IMatrix> GetAsync(string assetPair, DateTime dateTime);

        Task<IEnumerable<IMatrix>> GetByAssetPairAndDateAsync(string assetPair, DateTime date);

        Task<IEnumerable<IMatrix>> GetDateTimesOnlyByAssetPairAndDateAsync(string assetPair, DateTime date);

        Task InsertAsync(IMatrix matrix);

        Task<bool> DeleteAsync(string assetPair, DateTime dateTime);
    }
}
