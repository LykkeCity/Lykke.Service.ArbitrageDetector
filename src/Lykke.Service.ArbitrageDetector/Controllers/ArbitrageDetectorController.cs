using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Lykke.Service.ArbitrageDetector.Aspects.ExceptionHandling;
using Lykke.Service.ArbitrageDetector.Core.Services;
using Lykke.Service.ArbitrageDetector.Models;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Lykke.Service.ArbitrageDetector.Controllers
{
    [Produces("application/json")]
    [ExceptionToBadRequest]
    public class ArbitrageDetectorController : Controller
    {
        private readonly IArbitrageDetectorService _arbitrageDetectorService;
        private readonly ILykkeArbitrageDetectorService _lykkeArbitrageDetectorService;
        private readonly IMatrixSnapshotsService _matrixSnapshotsService;

        public ArbitrageDetectorController(IArbitrageDetectorService arbitrageDetectorService, 
            ILykkeArbitrageDetectorService lykkeArbitrageDetectorService,
            IMatrixSnapshotsService matrixSnapshotsService)
        {
            _arbitrageDetectorService = arbitrageDetectorService;
            _lykkeArbitrageDetectorService = lykkeArbitrageDetectorService;
            _matrixSnapshotsService = matrixSnapshotsService;
        }

        [HttpGet]
        [Route("orderBooks")]
        [SwaggerOperation("OrderBooks")]
        [ProducesResponseType(typeof(IEnumerable<OrderBookRow>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ErrorResponse), (int)HttpStatusCode.InternalServerError)]
        [ResponseCache(Duration = 1, VaryByQueryKeys = new[] { "exchange", "assetPair" })]
        public virtual IActionResult OrderBooks(string exchange, string assetPair)
        {
            var result = _arbitrageDetectorService.GetOrderBooks(exchange, assetPair).Select(x => new OrderBookRow(x)).ToList();

            return Ok(result);
        }

        [HttpGet]
        [Route("crossRates")]
        [SwaggerOperation("CrossRates")]
        [ProducesResponseType(typeof(IEnumerable<CrossRateRow>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ErrorResponse), (int)HttpStatusCode.InternalServerError)]
        [ResponseCache(Duration = 1)]
        public IActionResult CrossRates()
        {
            var result = _arbitrageDetectorService.GetCrossRates().Select(x => new CrossRateRow(x)).ToList();

            return Ok(result);
        }

        [HttpGet]
        [Route("arbitrages")]
        [SwaggerOperation("Arbitrages")]
        [ProducesResponseType(typeof(IEnumerable<ArbitrageRow>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ErrorResponse), (int)HttpStatusCode.InternalServerError)]
        [ResponseCache(Duration = 1)]
        public IActionResult Arbitrages()
        {
            var result = _arbitrageDetectorService.GetArbitrages().Select(x => new ArbitrageRow(x)).ToList();

            return Ok(result);
        }

        [HttpGet]
        [Route("arbitrageFromHistory")]
        [SwaggerOperation("ArbitrageFromHistory")]
        [ProducesResponseType(typeof(IEnumerable<Arbitrage>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ErrorResponse), (int)HttpStatusCode.InternalServerError)]
        [ResponseCache(Duration = 1, VaryByQueryKeys = new[] { "conversionPath" })]
        public IActionResult ArbitrageFromHistory(string conversionPath)
        {
            var arbitrage = _arbitrageDetectorService.GetArbitrageFromHistory(conversionPath);
            var result = arbitrage == null ? null : new Arbitrage(arbitrage);

            return Ok(result);
        }

        [HttpGet]
        [Route("arbitrageFromActiveOrHistory")]
        [SwaggerOperation("ArbitrageFromActiveOrHistory")]
        [ProducesResponseType(typeof(IEnumerable<Arbitrage>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ErrorResponse), (int)HttpStatusCode.InternalServerError)]
        [ResponseCache(Duration = 1, VaryByQueryKeys = new[] { "conversionPath" })]
        public IActionResult ArbitrageFromActiveOrHistory(string conversionPath)
        {
            var arbitrage = _arbitrageDetectorService.GetArbitrageFromActiveOrHistory(conversionPath);
            var result = arbitrage == null ? null : new Arbitrage(arbitrage);

            return Ok(result);
        }

        [HttpGet]
        [Route("arbitrageHistory")]
        [SwaggerOperation("ArbitrageHistory")]
        [ProducesResponseType(typeof(IEnumerable<ArbitrageRow>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ErrorResponse), (int)HttpStatusCode.InternalServerError)]
        [ResponseCache(Duration = 1, VaryByQueryKeys = new[] { "since", "take" })]
        public IActionResult ArbitrageHistory(DateTime since, int take)
        {
            var result = _arbitrageDetectorService.GetArbitrageHistory(since, take).Select(x => new ArbitrageRow(x)).ToList();

            return Ok(result);
        }

        [HttpGet]
        [Route("matrix")]
        [SwaggerOperation("MatrixEntity")]
        [ProducesResponseType(typeof(Matrix), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ErrorResponse), (int)HttpStatusCode.InternalServerError)]
        [ResponseCache(Duration = 1, VaryByQueryKeys = new[] { "assetPair" })]
        public IActionResult Matrix(string assetPair)
        {
            var matrix = _arbitrageDetectorService.GetMatrix(assetPair);
            var result = new Matrix(matrix);

            return Ok(result);
        }

        [HttpGet]
        [Route("publicMatrix")]
        [SwaggerOperation("PublicMatrix")]
        [ProducesResponseType(typeof(Matrix), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ErrorResponse), (int)HttpStatusCode.InternalServerError)]
        [ResponseCache(Duration = 1, VaryByQueryKeys = new [] { "assetPair" })]
        public IActionResult PublicMatrix(string assetPair)
        {
            var matrix = _arbitrageDetectorService.GetMatrix(assetPair, true);
            var result = new Matrix(matrix);

            return Ok(result);
        }

        [HttpGet]
        [Route("publicMatrixAssetPairs")]
        [SwaggerOperation("PublicMatrixAssetPairs")]
        [ProducesResponseType(typeof(IEnumerable<string>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ErrorResponse), (int)HttpStatusCode.InternalServerError)]
        [ResponseCache(Duration = 1)]
        public IActionResult PublicMatrixAssetPairs()
        {
            var settings = _arbitrageDetectorService.GetSettings();
            var result = settings.PublicMatrixAssetPairs;

            return Ok(result);
        }

        [HttpGet]
        [Route("lykkeArbitrages")]
        [SwaggerOperation("LykkeArbitrages")]
        [ProducesResponseType(typeof(IEnumerable<LykkeArbitrageRow>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ErrorResponse), (int)HttpStatusCode.InternalServerError)]
        [ResponseCache(Duration = 1, VaryByQueryKeys = new[] { "basePair", "crossPair" })]
        public IActionResult LykkeArbitrages(string basePair, string crossPair)
        {
            var result = _lykkeArbitrageDetectorService.GetArbitrages(basePair, crossPair)
                .Select(x => new LykkeArbitrageRow(x))
                .OrderBy(x => x.BaseAssetPair.Name)
                .ToList();

            return Ok(result);
        }

        [HttpGet]
        [Route("getSettings")]
        [SwaggerOperation("GetSettings")]
        [ProducesResponseType(typeof(Models.Settings), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ErrorResponse), (int)HttpStatusCode.InternalServerError)]
        public IActionResult GetSettings()
        {
            var settings = _arbitrageDetectorService.GetSettings();
            var result = new Models.Settings(settings);

            return Ok(result);
        }

        [HttpPost]
        [Route("setSettings")]
        [SwaggerOperation("SetSettings")]
        [ProducesResponseType(typeof(void), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ErrorResponse), (int)HttpStatusCode.InternalServerError)]
        public IActionResult SetSettings([FromBody]Models.Settings settings)
        {
            _arbitrageDetectorService.SetSettings(settings.ToModel());

            return Ok();
        }

        [HttpGet]
        [Route("matrixSnapshotStampsByDate")]
        [SwaggerOperation("MatrixSnapshotStampsByDate")]
        [ProducesResponseType(typeof(IEnumerable<DateTime>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ErrorResponse), (int)HttpStatusCode.InternalServerError)]
        [ResponseCache(Duration = 60*60*1, VaryByQueryKeys = new[] { "*" })] // 1 hour
        public async Task<IActionResult> MatrixSnapshotStampsByDate(string assetPair, DateTime date)
        {
            var result = (await _matrixSnapshotsService.GetDateTimeStampsAsync(assetPair, date)).ToList();

            return Ok(result);
        }

        [HttpGet]
        [Route("matrixSnapshotStampsByDateTimeRange")]
        [SwaggerOperation("MatrixSnapshotStampsByDateTimeRange")]
        [ProducesResponseType(typeof(IEnumerable<DateTime>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ErrorResponse), (int)HttpStatusCode.InternalServerError)]
        [ResponseCache(Duration = 60 * 60 * 1, VaryByQueryKeys = new[] { "*" })] // 1 hour
        public async Task<IActionResult> MatrixSnapshotStampsByDateTimeRange(string assetPair, DateTime from, DateTime to)
        {
            var result = (await _matrixSnapshotsService.GetDateTimeStampsAsync(assetPair, from, to)).ToList();

            return Ok(result);
        }

        [HttpGet]
        [Route("matrixSnapshotAssetPairs")]
        [SwaggerOperation("MatrixSnapshotAssetPairs")]
        [ProducesResponseType(typeof(IEnumerable<string>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ErrorResponse), (int)HttpStatusCode.InternalServerError)]
        [ResponseCache(Duration = 60 * 60 * 1, VaryByQueryKeys = new[] { "*" })] // 1 hour
        public async Task<IActionResult> MatrixSnapshotAssetPairs(DateTime date)
        {
            var result = (await _matrixSnapshotsService.GetAssetPairsAsync(date)).ToList();

            return Ok(result);
        }

        [HttpGet]
        [Route("matrixSnapshot")]
        [SwaggerOperation("MatrixSnapshot")]
        [ProducesResponseType(typeof(Matrix), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ErrorResponse), (int)HttpStatusCode.InternalServerError)]
        [ResponseCache(Duration = 60 * 60 * 1, VaryByQueryKeys = new[] { "*" })] // 1 hour
        public async Task<IActionResult> MatrixSnapshot(string assetPair, DateTime dateTime)
        {
            var result = await _matrixSnapshotsService.GetAsync(assetPair, dateTime);

            return Ok(result);
        }
    }
}
