using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Lykke.Service.ArbitrageDetector.Core.Services;
using Lykke.Service.ArbitrageDetector.Models;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Lykke.Service.ArbitrageDetector.Controllers
{
    public class ArbitrageDetectorController : Controller
    {
        private readonly IArbitrageDetectorService _arbitrageDetectorService;
        private readonly ILykkeArbitrageDetectorService _lykkeArbitrageDetectorService;
        private readonly IMatrixHistoryService _matrixHistoryService;

        public ArbitrageDetectorController(IArbitrageDetectorService arbitrageDetectorService, 
            ILykkeArbitrageDetectorService lykkeArbitrageDetectorService,
            IMatrixHistoryService matrixHistoryService)
        {
            _arbitrageDetectorService = arbitrageDetectorService;
            _lykkeArbitrageDetectorService = lykkeArbitrageDetectorService;
            _matrixHistoryService = matrixHistoryService;
        }

        [HttpGet("OrderBooks")]
        [SwaggerOperation("OrderBooks")]
        [ProducesResponseType(typeof(IEnumerable<OrderBookRow>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ErrorResponse), (int)HttpStatusCode.InternalServerError)]
        [ResponseCache(Duration = 1, VaryByQueryKeys = new[] { "*" })]
        public virtual IActionResult OrderBooks(string exchange, string assetPair)
        {
            var result = _arbitrageDetectorService.GetOrderBooks(exchange, assetPair).Select(x => new OrderBookRow(x)).ToList();

            return Ok(result);
        }

        [HttpGet("OrderBook")]
        [SwaggerOperation("OrderBook")]
        [ProducesResponseType(typeof(IEnumerable<OrderBook>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ErrorResponse), (int)HttpStatusCode.InternalServerError)]
        [ResponseCache(Duration = 1, VaryByQueryKeys = new[] { "*" })]
        public virtual IActionResult OrderBook(string exchange, string assetPair)
        {
            var result = _arbitrageDetectorService.GetOrderBook(exchange, assetPair);

            return Ok(result);
        }

        [HttpGet("SynthOrderBooks")]
        [SwaggerOperation("SynthOrderBooks")]
        [ProducesResponseType(typeof(IEnumerable<SynthOrderBookRow>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ErrorResponse), (int)HttpStatusCode.InternalServerError)]
        [ResponseCache(Duration = 1)]
        public IActionResult SynthOrderBooks()
        {
            var result = _arbitrageDetectorService.GetSynthOrderBooks().Select(x => new SynthOrderBookRow(x)).ToList();

            return Ok(result);
        }

        [HttpGet("CrossRates")]
        [SwaggerOperation("CrossRates")]
        [ProducesResponseType(typeof(IEnumerable<CrossRateRow>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ErrorResponse), (int)HttpStatusCode.InternalServerError)]
        [ResponseCache(Duration = 1)]
        [Obsolete]
        public IActionResult CrossRates()
        {
            var result = _arbitrageDetectorService.GetSynthOrderBooks().Select(x => new CrossRateRow(x)).ToList();

            return Ok(result);
        }

        [HttpGet("Arbitrages")]
        [SwaggerOperation("Arbitrages")]
        [ProducesResponseType(typeof(IEnumerable<ArbitrageRow>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ErrorResponse), (int)HttpStatusCode.InternalServerError)]
        [ResponseCache(Duration = 1)]
        public IActionResult Arbitrages()
        {
            var result = _arbitrageDetectorService.GetArbitrages().Select(x => new ArbitrageRow(x)).ToList();

            return Ok(result);
        }

        [HttpGet("ArbitrageFromHistory")]
        [SwaggerOperation("ArbitrageFromHistory")]
        [ProducesResponseType(typeof(IEnumerable<Arbitrage>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ErrorResponse), (int)HttpStatusCode.InternalServerError)]
        [ResponseCache(Duration = 1, VaryByQueryKeys = new[] { "*" })]
        public IActionResult ArbitrageFromHistory(string conversionPath)
        {
            var arbitrage = _arbitrageDetectorService.GetArbitrageFromHistory(conversionPath);
            var result = arbitrage == null ? null : new Arbitrage(arbitrage);

            return Ok(result);
        }

        [HttpGet("ArbitrageFromActiveOrHistory")]
        [SwaggerOperation("ArbitrageFromActiveOrHistory")]
        [ProducesResponseType(typeof(IEnumerable<Arbitrage>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ErrorResponse), (int)HttpStatusCode.InternalServerError)]
        [ResponseCache(Duration = 1, VaryByQueryKeys = new[] { "*" })]
        public IActionResult ArbitrageFromActiveOrHistory(string conversionPath)
        {
            var arbitrage = _arbitrageDetectorService.GetArbitrageFromActiveOrHistory(conversionPath);
            var result = arbitrage == null ? null : new Arbitrage(arbitrage);

            return Ok(result);
        }

        [HttpGet("ArbitrageHistory")]
        [SwaggerOperation("ArbitrageHistory")]
        [ProducesResponseType(typeof(IEnumerable<ArbitrageRow>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ErrorResponse), (int)HttpStatusCode.InternalServerError)]
        [ResponseCache(Duration = 1, VaryByQueryKeys = new[] { "*" })]
        public IActionResult ArbitrageHistory(DateTime since, int take)
        {
            var result = _arbitrageDetectorService.GetArbitrageHistory(since, take).Select(x => new ArbitrageRow(x)).ToList();

            return Ok(result);
        }

        [HttpGet("Matrix")]
        [SwaggerOperation("Matrix")]
        [ProducesResponseType(typeof(Matrix), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ErrorResponse), (int)HttpStatusCode.InternalServerError)]
        [ResponseCache(Duration = 1, VaryByQueryKeys = new[] { "*" })]
        public IActionResult Matrix(string assetPair, bool depositFee = false, bool tradingFee = false)
        {
            if (string.IsNullOrWhiteSpace(assetPair))
                return NotFound();

            var matrix = _arbitrageDetectorService.GetMatrix(assetPair, false, depositFee, tradingFee);
            var result = new Matrix(matrix);

            return Ok(result);
        }

        [HttpGet("PublicMatrix")]
        [SwaggerOperation("PublicMatrix")]
        [ProducesResponseType(typeof(Matrix), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ErrorResponse), (int)HttpStatusCode.InternalServerError)]
        [ResponseCache(Duration = 1, VaryByQueryKeys = new [] { "*" })]
        public IActionResult PublicMatrix(string assetPair, bool depositFee = false, bool tradingFee = false)
        {
            if (string.IsNullOrWhiteSpace(assetPair))
                return NotFound();

            var matrix = _arbitrageDetectorService.GetMatrix(assetPair, true, depositFee, tradingFee);
            var result = new Matrix(matrix);

            return Ok(result);
        }

        [HttpGet("PublicMatrixAssetPairs")]
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

        [HttpGet("LykkeArbitrages")]
        [SwaggerOperation("LykkeArbitrages")]
        [ProducesResponseType(typeof(IEnumerable<LykkeArbitrageRow>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ErrorResponse), (int)HttpStatusCode.InternalServerError)]
        [ResponseCache(Duration = 1, VaryByQueryKeys = new[] { "*" })]
        public IActionResult LykkeArbitrages(string basePair, string crossPair, string target = "", string source = "", ArbitrageProperty property = default, decimal minValue = 0)
        {
            target = string.IsNullOrWhiteSpace(target) ? basePair : target;
            source = string.IsNullOrWhiteSpace(source) ? crossPair : source;

            var result = _lykkeArbitrageDetectorService.GetArbitrages(target, source, (Core.Domain.ArbitrageProperty)property, minValue)
                .Select(x => new LykkeArbitrageRow(x))
                .ToList();

            return Ok(result);
        }

        [HttpGet("MatrixHistoryStamps")]
        [SwaggerOperation("MatrixHistoryStamps")]
        [ProducesResponseType(typeof(IEnumerable<DateTime>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ErrorResponse), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> MatrixHistoryStamps(string assetPair, DateTime date, bool lykkeArbitragesOnly)
        {
            var result = (await _matrixHistoryService.GetStampsAsync(assetPair, date, lykkeArbitragesOnly)).ToList();

            return Ok(result);
        }

        [HttpGet("MatrixHistoryAssetPairs")]
        [SwaggerOperation("MatrixHistoryAssetPairs")]
        [ProducesResponseType(typeof(IEnumerable<string>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ErrorResponse), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> MatrixHistoryAssetPairs(DateTime date, bool lykkeArbitragesOnly)
        {
            var result = (await _matrixHistoryService.GetAssetPairsAsync(date, lykkeArbitragesOnly)).ToList();

            return Ok(result);
        }

        [HttpGet("MatrixHistory")]
        [SwaggerOperation("MatrixHistory")]
        [ProducesResponseType(typeof(Matrix), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ErrorResponse), (int)HttpStatusCode.InternalServerError)]
        [ResponseCache(Duration = 60 * 60 * 4, VaryByQueryKeys = new[] { "*" })] // 4 hours
        public async Task<IActionResult> MatrixHistory(string assetPair, DateTime dateTime)
        {
            var result = await _matrixHistoryService.GetAsync(assetPair, dateTime);

            return Ok(result);
        }

        [HttpGet("GetSettings")]
        [SwaggerOperation("GetSettings")]
        [ProducesResponseType(typeof(Models.Settings), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ErrorResponse), (int)HttpStatusCode.InternalServerError)]
        public IActionResult GetSettings()
        {
            var settings = _arbitrageDetectorService.GetSettings();
            var result = new Models.Settings(settings);

            return Ok(result);
        }

        [HttpPost("SetSettings")]
        [SwaggerOperation("SetSettings")]
        [ProducesResponseType(typeof(void), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ErrorResponse), (int)HttpStatusCode.InternalServerError)]
        public IActionResult SetSettings([FromBody]Models.Settings settings)
        {
            var domainSettings = settings.ToDomain();
            _arbitrageDetectorService.SetSettings(domainSettings);

            return Ok();
        }
    }
}
