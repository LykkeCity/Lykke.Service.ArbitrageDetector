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
using DataCrossRate = Lykke.Service.ArbitrageDetector.Models.CrossRate;
using DataArbitrage = Lykke.Service.ArbitrageDetector.Models.Arbitrage;

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
        public async Task<IActionResult> OrderBooks(string exchange, string instrument)
        {
            IEnumerable<DataOrderBook> result;

            try
            {
                result = _arbitrageDetectorService.GetOrderBooks(exchange, instrument).Select(x => new DataOrderBook(x)).ToList();
            }
            catch (Exception exception)
            {
                await _log.WriteErrorAsync(nameof(ArbitrageDetectorController), nameof(OrderBooks), nameof(OrderBooks), exception);

                return BadRequest(ErrorResponse.Create(exception.Message));
            }

            return Ok(result);
        }

        [HttpGet]
        [Route("crossRates")]
        [SwaggerOperation("CrossRates")]
        [ProducesResponseType(typeof(IEnumerable<DataCrossRate>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ErrorResponse), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> CrossRates()
        {
            IEnumerable<DataCrossRate> result;

            try
            {
                result = _arbitrageDetectorService.GetCrossRates().Select(x => new DataCrossRate(x)).ToList();
            }
            catch (Exception exception)
            {
                await _log.WriteErrorAsync(nameof(ArbitrageDetectorController), nameof(CrossRates), nameof(CrossRates), exception);

                return BadRequest(ErrorResponse.Create(exception.Message));
            }

            return Ok(result);
        }

        [HttpGet]
        [Route("arbitrages")]
        [SwaggerOperation("Arbitrages")]
        [ProducesResponseType(typeof(IEnumerable<DataArbitrage>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ErrorResponse), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> Arbitrages()
        {
            IEnumerable<DataArbitrage> result;

            try
            {
                result = _arbitrageDetectorService.GetArbitrages().Select(x => new DataArbitrage(x)).ToList();
            }
            catch (Exception exception)
            {
                await _log.WriteErrorAsync(nameof(ArbitrageDetectorController), nameof(Arbitrages), nameof(Arbitrages), exception);

                return BadRequest(ErrorResponse.Create(exception.Message));
            }

            return Ok(result);
        }

        [HttpGet]
        [Route("arbitrageHistory")]
        [SwaggerOperation("ArbitrageHistory")]
        [ProducesResponseType(typeof(IEnumerable<DataArbitrage>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ErrorResponse), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> ArbitrageHistory(DateTime since, int take)
        {
            IEnumerable<DataArbitrage> result;

            try
            {
                result = _arbitrageDetectorService.GetArbitrageHistory(since, take).Select(x => new DataArbitrage(x)).ToList();
            }
            catch (Exception exception)
            {
                await _log.WriteErrorAsync(nameof(ArbitrageDetectorController), nameof(ArbitrageHistory), nameof(ArbitrageHistory), exception);

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
                await _log.WriteErrorAsync(nameof(ArbitrageDetectorController), nameof(ArbitrageHistory), nameof(SetSettings), exception);

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
                await _log.WriteErrorAsync(nameof(ArbitrageDetectorController), nameof(ArbitrageHistory), nameof(SetSettings), exception);

                return BadRequest(ErrorResponse.Create(exception.Message));
            }

            return Ok();
        }
    }
}
