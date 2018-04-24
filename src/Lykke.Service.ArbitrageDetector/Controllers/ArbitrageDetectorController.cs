using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Common.Log;
using Lykke.Service.ArbitrageDetector.Core.Services;
using Lykke.Service.ArbitrageDetector.Models;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.SwaggerGen;
using DataOrderBook = Lykke.Service.ArbitrageDetector.Models.OrderBook;

namespace Lykke.Service.ArbitrageDetector.Controllers
{
    [Produces("application/json")]
    public class ArbitrageDetectorController : Controller
    {
        private readonly IArbitrageDetectorService _arbitrageDetectorService;
        private readonly ILog _log;

        public ArbitrageDetectorController(IArbitrageDetectorService arbitrageDetectorService, ILog log)
        {
            _arbitrageDetectorService = arbitrageDetectorService;
            _log = log;
        }

        [HttpGet]
        [Route("orderBooks")]
        [SwaggerOperation("OrderBooks")]
        [ProducesResponseType(typeof(IEnumerable<DataOrderBook>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ErrorResponse), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> OrderBooks(string exchange, string assetPair)
        {
            IEnumerable<DataOrderBook> result;

            try
            {
                result = _arbitrageDetectorService.GetOrderBooks(exchange, assetPair).Select(x => new DataOrderBook(x)).ToList();
            }
            catch (Exception exception)
            {
                await _log.WriteErrorAsync(GetType().Name, nameof(OrderBooks), exception);
                return BadRequest(ErrorResponse.Create(exception.Message));
            }

            return Ok(result);
        }

        [HttpGet]
        [Route("crossRates")]
        [SwaggerOperation("CrossRates")]
        [ProducesResponseType(typeof(IEnumerable<CrossRateRow>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ErrorResponse), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> CrossRates()
        {
            IEnumerable<CrossRateRow> result;

            try
            {
                result = _arbitrageDetectorService.GetCrossRates().Select(x => new CrossRateRow(x)).ToList();
            }
            catch (Exception exception)
            {
                await _log.WriteErrorAsync(GetType().Name, nameof(CrossRates), exception);

                return BadRequest(ErrorResponse.Create(exception.Message));
            }

            return Ok(result);
        }

        [HttpGet]
        [Route("arbitrages")]
        [SwaggerOperation("Arbitrages")]
        [ProducesResponseType(typeof(IEnumerable<ArbitrageRow>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ErrorResponse), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> Arbitrages()
        {
            IEnumerable<ArbitrageRow> result;

            try
            {
                result = _arbitrageDetectorService.GetArbitrages().Select(x => new ArbitrageRow(x)).ToList();
            }
            catch (Exception exception)
            {
                await _log.WriteErrorAsync(GetType().Name, nameof(Arbitrages), exception);

                return BadRequest(ErrorResponse.Create(exception.Message));
            }

            return Ok(result);
        }

        [HttpGet]
        [Route("arbitrageFromHistory")]
        [SwaggerOperation("ArbitrageFromHistory")]
        [ProducesResponseType(typeof(IEnumerable<Arbitrage>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ErrorResponse), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> ArbitrageFromHistory(string conversionPath)
        {
            Arbitrage result;

            try
            {
                var arbitrage = _arbitrageDetectorService.GetArbitrageFromHistory(conversionPath);
                result = arbitrage == null ? null : new Arbitrage(arbitrage);
            }
            catch (Exception exception)
            {
                await _log.WriteErrorAsync(GetType().Name, nameof(Arbitrage), exception);

                return BadRequest(ErrorResponse.Create(exception.Message));
            }

            return Ok(result);
        }

        [HttpGet]
        [Route("arbitrageFromActiveOrHistory")]
        [SwaggerOperation("ArbitrageFromActiveOrHistory")]
        [ProducesResponseType(typeof(IEnumerable<Arbitrage>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ErrorResponse), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> ArbitrageFromActiveOrHistory(string conversionPath)
        {
            Arbitrage result;

            try
            {
                var arbitrage = _arbitrageDetectorService.GetArbitrageFromActiveOrHistory(conversionPath);
                result = arbitrage == null ? null : new Arbitrage(arbitrage);
            }
            catch (Exception exception)
            {
                await _log.WriteErrorAsync(GetType().Name, nameof(Arbitrage), exception);

                return BadRequest(ErrorResponse.Create(exception.Message));
            }

            return Ok(result);
        }

        [HttpGet]
        [Route("arbitrageHistory")]
        [SwaggerOperation("ArbitrageHistory")]
        [ProducesResponseType(typeof(IEnumerable<ArbitrageRow>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ErrorResponse), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> ArbitrageHistory(DateTime since, int take)
        {
            IEnumerable<ArbitrageRow> result;

            try
            {
                result = _arbitrageDetectorService.GetArbitrageHistory(since, take).Select(x => new ArbitrageRow(x)).ToList();
            }
            catch (Exception exception)
            {
                await _log.WriteErrorAsync(GetType().Name, nameof(ArbitrageHistory), exception);

                return BadRequest(ErrorResponse.Create(exception.Message));
            }

            return Ok(result);
        }

        [HttpGet]
        [Route("getSettings")]
        [SwaggerOperation("GetSettings")]
        [ProducesResponseType(typeof(Models.Settings), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ErrorResponse), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetSettings()
        {
            Models.Settings result;

            try
            {
                var settings = _arbitrageDetectorService.GetSettings();
                result = new Models.Settings(settings);
            }
            catch (Exception exception)
            {
                await _log.WriteErrorAsync(GetType().Name, nameof(GetSettings), exception);

                return BadRequest(ErrorResponse.Create(exception.Message));
            }

            return Ok(result);
        }

        [HttpPost]
        [Route("setSettings")]
        [SwaggerOperation("SetSettings")]
        [ProducesResponseType(typeof(void), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ErrorResponse), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> SetSettings([FromBody]Models.Settings settings)
        {
            try
            {
                _arbitrageDetectorService.SetSettings(settings.ToModel());
            }
            catch (Exception exception)
            {
                await _log.WriteErrorAsync(GetType().Name, nameof(SetSettings), exception);

                return BadRequest(ErrorResponse.Create(exception.Message));
            }

            return Ok();
        }
    }
}
