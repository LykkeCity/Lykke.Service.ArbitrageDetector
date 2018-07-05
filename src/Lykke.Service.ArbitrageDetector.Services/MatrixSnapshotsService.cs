using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Common;
using Common.Log;
using Lykke.Service.ArbitrageDetector.Core.Repositories;
using Lykke.Service.ArbitrageDetector.Core.Services;

namespace Lykke.Service.ArbitrageDetector.Services
{
    public class MatrixSnapshotsService : TimerPeriod
    {
        private readonly IMatrixRepository _matrixRepository;
        private readonly IArbitrageDetectorService _arbitrageDetectorService;

        public MatrixSnapshotsService(ILog log, IMatrixRepository matrixRepository, IArbitrageDetectorService arbitrageDetectorService) : base(10*1000, log)
        {
            _matrixRepository = matrixRepository;
            _arbitrageDetectorService = arbitrageDetectorService;
        }

        public override async Task Execute()
        {
            var settings = _arbitrageDetectorService.GetSettings();

            if (settings.MatrixSnapshotAssetPairs == null || !settings.MatrixSnapshotAssetPairs.Any())
            {
                settings.MatrixSnapshotAssetPairs = new List<string> { "BTCUSD" };
                _arbitrageDetectorService.SetSettings(settings);
            }

            var assetPairs = settings.MatrixSnapshotAssetPairs;

            foreach (var assetPair in assetPairs)
            {
                var matrix = _arbitrageDetectorService.GetMatrix(assetPair);
                await _matrixRepository.InsertOrReplaceAsync(matrix);
            }
        }
    }
}
