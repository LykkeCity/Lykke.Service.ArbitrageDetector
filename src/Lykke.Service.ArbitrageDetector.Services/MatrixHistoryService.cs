using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Common;
using Common.Log;
using Lykke.Service.ArbitrageDetector.Core.Domain;
using Lykke.Service.ArbitrageDetector.Core.Repositories;
using Lykke.Service.ArbitrageDetector.Core.Services;

namespace Lykke.Service.ArbitrageDetector.Services
{
    public class MatrixHistoryService : TimerPeriod, IMatrixHistoryService
    {
        private static readonly TimeSpan DefaultInterval = new TimeSpan(0, 0, 5, 0);
        private readonly IMatrixHistoryRepository _matrixHistoryRepository;
        private readonly IArbitrageDetectorService _arbitrageDetectorService;

        public MatrixHistoryService(TimeSpan interval, ILog log, IMatrixHistoryRepository matrixHistoryRepository, IArbitrageDetectorService arbitrageDetectorService)
            // TODO: must be changed after Common.TimerPeriod change
            : base((int)interval.TotalMilliseconds == 0 ? (int)DefaultInterval.TotalMilliseconds : (int)interval.TotalMilliseconds, log)
        {
            _matrixHistoryRepository = matrixHistoryRepository;
            _arbitrageDetectorService = arbitrageDetectorService;
        }

        public override async Task Execute()
        {
            InitializeSettingsFirstTime();
            await SaveMatrixToDatabase();
        }

        private void InitializeSettingsFirstTime()
        {
            var settings = _arbitrageDetectorService.GetSettings();

            // First time settings initialization
            if (settings.MatrixSnapshotAssetPairs == null)
            {
                settings.MatrixSnapshotAssetPairs = new List<string>();
                settings.MatrixSnapshotInterval = new TimeSpan(0, 0, 5, 0);
                _arbitrageDetectorService.SetSettings(settings);
            }
        }

        private async Task SaveMatrixToDatabase()
        {
            var settings = _arbitrageDetectorService.GetSettings();

            var assetPairs = settings.MatrixSnapshotAssetPairs;

            foreach (var assetPair in assetPairs)
            {
                var matrix = _arbitrageDetectorService.GetMatrix(assetPair);
                await _matrixHistoryRepository.InsertAsync(matrix);
            }
        }

        #region IMatrixHistoryService

        public Task<IEnumerable<DateTime>> GetDateTimeStampsAsync(string assetPair, DateTime date)
        {
            return _matrixHistoryRepository.GetDateTimeStampsAsync(assetPair, date);
        }

        public Task<IEnumerable<string>> GetAssetPairsAsync(DateTime date)
        {
            return _matrixHistoryRepository.GetAssetPairsAsync(date);
        }

        public Task<Matrix> GetAsync(string assetPair, DateTime date)
        {
            return _matrixHistoryRepository.GetAsync(assetPair, date);
        }

        #endregion
    }
}
