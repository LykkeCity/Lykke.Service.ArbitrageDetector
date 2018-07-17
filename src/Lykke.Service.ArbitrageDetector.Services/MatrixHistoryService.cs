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

            InitSettings();
        }

        private void InitSettings()
        {
            var settings = _arbitrageDetectorService.GetSettings();

            // First time matrix history settings initialization

            var isDirty = false;

            if (settings.MatrixHistoryAssetPairs == null)
            {
                settings.MatrixHistoryAssetPairs = new List<string>();
                isDirty = true;
            }

            if (settings.MatrixHistoryInterval == default)
            {
                settings.MatrixHistoryInterval = new TimeSpan(0, 0, 5, 0);
                isDirty = true;
            }

            if (string.IsNullOrWhiteSpace(settings.MatrixHistoryLykkeName))
            {
                settings.MatrixHistoryLykkeName = "lykke";
                isDirty = true;
            }

            if (isDirty)
                _arbitrageDetectorService.SetSettings(settings);
        }

        public override async Task Execute()
        {
            await SaveMatrixToDatabase();
        }

        private async Task SaveMatrixToDatabase()
        {
            var settings = _arbitrageDetectorService.GetSettings();

            var assetPairs = settings.MatrixHistoryAssetPairs;

            foreach (var assetPair in assetPairs)
            {
                var matrix = _arbitrageDetectorService.GetMatrix(assetPair);
                await _matrixHistoryRepository.InsertAsync(matrix);
            }
        }

        #region IMatrixHistoryService

        public Task<IEnumerable<DateTime>> GetStampsAsync(string assetPair, DateTime date, bool lykkeArbitragesOnly)
        {
            GetSignificantSpreadAndLykkeName(lykkeArbitragesOnly, out var matrixSignificantSpread, out var lykkeName);

            return _matrixHistoryRepository.GetDateTimeStampsAsync(assetPair, date, matrixSignificantSpread, lykkeName);
        }

        public Task<IEnumerable<string>> GetAssetPairsAsync(DateTime date, bool lykkeArbitragesOnly)
        {
            GetSignificantSpreadAndLykkeName(lykkeArbitragesOnly, out var matrixSignificantSpread, out var lykkeName);

            return _matrixHistoryRepository.GetAssetPairsAsync(date, matrixSignificantSpread, lykkeName);
        }

        public Task<Matrix> GetAsync(string assetPair, DateTime date)
        {
            return _matrixHistoryRepository.GetAsync(assetPair, date);
        }

        private void GetSignificantSpreadAndLykkeName(bool lykkeArbitragesOnly, out decimal? matrixSignificantSpread, out IReadOnlyCollection<string> lykkeName)
        {
            matrixSignificantSpread = null;
            lykkeName = null;
            if (lykkeArbitragesOnly)
            {
                var settings = _arbitrageDetectorService.GetSettings();
                matrixSignificantSpread = settings.MatrixSignificantSpread;
                lykkeName = new[] { settings.MatrixHistoryLykkeName };
            }
        }

        #endregion
    }
}
