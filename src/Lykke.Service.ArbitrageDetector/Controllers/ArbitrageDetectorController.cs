using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using Lykke.Service.ArbitrageDetector.Aspects.Cache;
using Lykke.Service.ArbitrageDetector.Aspects.ExceptionHandling;
using Lykke.Service.ArbitrageDetector.Core.Services;
using Lykke.Service.ArbitrageDetector.Models;
using Microsoft.AspNetCore.Mvc;
using MoreLinq;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Lykke.Service.ArbitrageDetector.Controllers
{
    [Produces("application/json")]
    [ExceptionToBadRequest]
    [Cache(Duration = 1000)]
    public class ArbitrageDetectorController : Controller
    {
        private readonly IArbitrageDetectorService _arbitrageDetectorService;
        private readonly ILykkeArbitrageDetectorService _lykkeArbitrageDetectorService;

        public ArbitrageDetectorController(IArbitrageDetectorService arbitrageDetectorService, ILykkeArbitrageDetectorService lykkeArbitrageDetectorService)
        {
            _arbitrageDetectorService = arbitrageDetectorService;
            _lykkeArbitrageDetectorService = lykkeArbitrageDetectorService;
        }

        [HttpGet]
        [Route("orderBooks")]
        [SwaggerOperation("OrderBooks")]
        [ProducesResponseType(typeof(IEnumerable<OrderBook>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ErrorResponse), (int)HttpStatusCode.InternalServerError)]
        public virtual IActionResult OrderBooks(string exchange, string assetPair)
        {
            var result = _arbitrageDetectorService.GetOrderBooks(exchange, assetPair).Select(x => new OrderBook(x)).ToList();

            return Ok(result);
        }

        [HttpGet]
        [Route("newOrderBooks")]
        [SwaggerOperation("NewOrderBooks")]
        [ProducesResponseType(typeof(IEnumerable<OrderBookRow>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ErrorResponse), (int)HttpStatusCode.InternalServerError)]
        public virtual IActionResult NewOrderBooks(string exchange, string assetPair)
        {
            var result = _arbitrageDetectorService.GetOrderBooks(exchange, assetPair).Select(x => new OrderBookRow(x)).ToList();

            return Ok(result);
        }

        [HttpGet]
        [Route("crossRates")]
        [SwaggerOperation("CrossRates")]
        [ProducesResponseType(typeof(IEnumerable<CrossRateRow>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ErrorResponse), (int)HttpStatusCode.InternalServerError)]
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
        public IActionResult ArbitrageHistory(DateTime since, int take)
        {
            var result = _arbitrageDetectorService.GetArbitrageHistory(since, take).Select(x => new ArbitrageRow(x)).ToList();

            return Ok(result);
        }

        [HttpGet]
        [Route("matrix")]
        [SwaggerOperation("Matrix")]
        [ProducesResponseType(typeof(Matrix), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ErrorResponse), (int)HttpStatusCode.InternalServerError)]
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
        public IActionResult LykkeArbitrages()
        {
            var result = _lykkeArbitrageDetectorService.GetArbitrages()
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
    }
}
