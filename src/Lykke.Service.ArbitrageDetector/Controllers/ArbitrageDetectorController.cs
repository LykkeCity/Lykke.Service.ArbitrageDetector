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
using DataCrossRate = Lykke.Service.ArbitrageDetector.Core.DataModel.CrossRate;
using DataArbitrage = Lykke.Service.ArbitrageDetector.Core.DataModel.Arbitrage;

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
        [ProducesResponseType(typeof(IEnumerable<DataCrossRate>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ErrorResponse), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> CrossRates()
        {
            IEnumerable<DataCrossRate> result;

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
        [ProducesResponseType(typeof(IEnumerable<DataArbitrage>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ErrorResponse), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> Arbitrages()
        {
            IEnumerable<DataArbitrage> result;

            try
            {
                result = _arbitrageDetectorService.GetArbitragesData();
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
        [ProducesResponseType(typeof(IEnumerable<DataArbitrage>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ErrorResponse), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> ArbitrageHistory(DateTime since, int take)
        {
            IEnumerable<DataArbitrage> result;

            try
            {
                result = _arbitrageDetectorService.GetArbitrageHistory(since, take);
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
