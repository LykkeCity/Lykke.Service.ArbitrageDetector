using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Autofac;
using Common;
using Common.Log;
using Lykke.Service.ArbitrageDetector.Core.Domain;
using Lykke.Service.ArbitrageDetector.Core.Repositories;
using Lykke.Service.ArbitrageDetector.Core.Services;
using Lykke.Service.ArbitrageDetector.Core.Services.Infrastructure;

namespace Lykke.Service.ArbitrageDetector.Services
{
    public class MatrixHistoryService : IMatrixHistoryService, IStartable, IStopable
    {
        private static readonly TimeSpan DefaultInterval = new TimeSpan(0, 0, 5, 0);
        private readonly TimerTrigger _trigger;
        private readonly IMatrixHistoryRepository _matrixHistoryRepository;
        private readonly IArbitrageDetectorService _arbitrageDetectorService;
        private readonly ILog _log;

        public MatrixHistoryService(ILog log, IShutdownManager shutdownManager, IMatrixHistoryRepository matrixHistoryRepository, IArbitrageDetectorService arbitrageDetectorService)
        {
            shutdownManager?.Register(this);

            _matrixHistoryRepository = matrixHistoryRepository;
            _arbitrageDetectorService = arbitrageDetectorService;
            _log = log ?? throw new ArgumentNullException(nameof(log));

            InitSettings();

            var settings = _arbitrageDetectorService.GetSettings();
            _trigger = new TimerTrigger(nameof(MatrixHistoryService), settings.MatrixHistoryInterval, log, Execute);
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
                settings.MatrixHistoryInterval = DefaultInterval;
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

        public async Task Execute(ITimerTrigger timer, TimerTriggeredHandlerArgs args, CancellationToken cancellationtoken)
        {
            try
            {
                await Execute();
            }
            catch (Exception ex)
            {
                await _log.WriteErrorAsync(GetType().Name, nameof(Execute), ex);
            }
        }

        public async Task Execute()
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
