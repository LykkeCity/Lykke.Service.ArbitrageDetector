using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Autofac;
using Common;
using Common.Log;
using Lykke.Common.Log;
using Lykke.Service.ArbitrageDetector.Core.Domain;
using Lykke.Service.ArbitrageDetector.Core.Repositories;
using Lykke.Service.ArbitrageDetector.Core.Services;

namespace Lykke.Service.ArbitrageDetector.Services
{
    public class MatrixHistoryService : IMatrixHistoryService, IStartable, IStopable
    {
        private readonly TimerTrigger _trigger;
        private readonly IArbitrageDetectorService _arbitrageDetectorService;
        private readonly ISettingsService _settingsService;
        private readonly IMatrixHistoryRepository _matrixHistoryRepository;
        private readonly ILog _log;

        public MatrixHistoryService(IArbitrageDetectorService arbitrageDetectorService, ISettingsService settingsService, IMatrixHistoryRepository matrixHistoryRepository, ILogFactory logFactory)
        {
            _arbitrageDetectorService = arbitrageDetectorService;
            _settingsService = settingsService;
            _matrixHistoryRepository = matrixHistoryRepository;
            _log = logFactory.CreateLog(this);

            var settings = settingsService.GetAsync().GetAwaiter().GetResult();
            _trigger = new TimerTrigger(nameof(MatrixHistoryService), settings.MatrixHistoryInterval, logFactory, Execute);
        }

        public async Task Execute(ITimerTrigger timer, TimerTriggeredHandlerArgs args, CancellationToken cancellationtoken)
        {
            try
            {
                await Execute();
            }
            catch (Exception ex)
            {
                _log.Error(ex);
            }
        }

        public async Task Execute()
        {
            await SaveMatrixToDatabase();
        }

        private async Task SaveMatrixToDatabase()
        {
            var settings = await _settingsService.GetAsync();

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
                var settings = _settingsService.GetAsync().GetAwaiter().GetResult();
                matrixSignificantSpread = settings.MatrixSignificantSpread;
                lykkeName = new[] { settings.MatrixHistoryLykkeName };
            }
        }

        #endregion

        #region IStartable, IStopable

        public void Start()
        {
            _trigger.Start();
        }
        public void Stop()
        {
            _trigger.Stop();
        }

        public void Dispose()
        {
            _trigger?.Dispose();
        }

        #endregion
    }
}
