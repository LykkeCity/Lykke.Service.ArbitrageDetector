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
    public class MatrixSnapshotsService : TimerPeriod, IMatrixSnapshotsService
    {
        private static readonly TimeSpan DefaultInterval = new TimeSpan(0, 0, 5, 0);
        private readonly IMatrixRepository _matrixRepository;
        private readonly IArbitrageDetectorService _arbitrageDetectorService;

        public MatrixSnapshotsService(TimeSpan interval, ILog log, IMatrixRepository matrixRepository, IArbitrageDetectorService arbitrageDetectorService)
            // TODO: must be changed after Common.TimerPeriod change
            : base((int)interval.TotalMilliseconds == 0 ? (int)DefaultInterval.TotalMilliseconds : (int)interval.TotalMilliseconds, log)
        {
            _matrixRepository = matrixRepository;
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
                await _matrixRepository.InsertAsync(matrix);
            }
        }

        #region IMatrixSnapshotsService

        public Task<Matrix> GetAsync(string assetPair, DateTime date)
        {
            return _matrixRepository.GetAsync(assetPair, date);
        }

        public Task<IEnumerable<DateTime>> GetDateTimeStampsAsync(string assetPair, DateTime date)
        {
            return _matrixRepository.GetDateTimeStampsAsync(assetPair, date);
        }

        public Task<IEnumerable<DateTime>> GetDateTimeStampsAsync(string assetPair, DateTime from, DateTime to)
        {
            return _matrixRepository.GetDateTimeStampsAsync(assetPair, from, to);
        }

        #endregion
    }
}
