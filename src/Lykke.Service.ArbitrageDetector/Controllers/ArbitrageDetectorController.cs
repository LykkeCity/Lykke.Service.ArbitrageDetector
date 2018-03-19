using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Common.Log;
using Lykke.Service.ArbitrageDetector.Core.Domain;
using Lykke.Service.ArbitrageDetector.Core.Services;
using Lykke.Service.ArbitrageDetector.Models;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.SwaggerGen;

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
        [ProducesResponseType(typeof(IEnumerable<CrossRate>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ErrorResponse), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> OrderBooks(string exchange, string instrument)
        {
            IEnumerable<OrderBook> result;

            try
            {
                result = _arbitrageDetectorService.GetOrderBooks(exchange, instrument);
            }
            catch (Exception exception)
            {
                await _log.WriteErrorAsync(nameof(ArbitrageDetectorController), nameof(OrderBooks), "", exception);

                return BadRequest(ErrorResponse.Create(exception.Message));
            }

            return Ok(result);
        }

        [HttpGet]
        [Route("crossRates")]
        [SwaggerOperation("CrossRates")]
        [ProducesResponseType(typeof(IEnumerable<CrossRate>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ErrorResponse), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> CrossRates()
        {
            IEnumerable<CrossRate> result;

            try
            {
                result = _arbitrageDetectorService.GetCrossRates();
            }
            catch (Exception exception)
            {
                await _log.WriteErrorAsync(nameof(ArbitrageDetectorController), nameof(CrossRates), "", exception);

                return BadRequest(ErrorResponse.Create(exception.Message));
            }

            return Ok(result);
        }

        [HttpGet]
        [Route("arbitrages")]
        [SwaggerOperation("Arbitrages")]
        [ProducesResponseType(typeof(IEnumerable<Arbitrage>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ErrorResponse), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> Arbitrages()
        {
            IEnumerable<Arbitrage> result;

            try
            {
                result = _arbitrageDetectorService.GetArbitrages();
            }
            catch (Exception exception)
            {
                await _log.WriteErrorAsync(nameof(ArbitrageDetectorController), nameof(Arbitrages), "", exception);

                return BadRequest(ErrorResponse.Create(exception.Message));
            }

            return Ok(result);
        }

        [HttpGet]
        [Route("arbitrageHistory")]
        [SwaggerOperation("ArbitrageHistory")]
        [ProducesResponseType(typeof(IEnumerable<ArbitrageHistory>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ErrorResponse), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> ArbitrageHistory(DateTime since)
        {
            IEnumerable<ArbitrageHistory> result;

            try
            {
                result = _arbitrageDetectorService.GetArbitrageHistory(since);
            }
            catch (Exception exception)
            {
                await _log.WriteErrorAsync(nameof(ArbitrageDetectorController), nameof(Arbitrages), "", exception);

                return BadRequest(ErrorResponse.Create(exception.Message));
            }

            return Ok(result);
        }
    }
}
