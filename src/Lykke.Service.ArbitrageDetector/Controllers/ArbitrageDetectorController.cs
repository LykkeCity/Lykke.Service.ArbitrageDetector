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
    public class ArbitrageDetectorController : BaseController
    {
        private readonly IArbitrageDetectorService _arbitrageDetectorService;
        private readonly ILog _log;

        public ArbitrageDetectorController(IArbitrageDetectorService arbitrageDetectorService, ILog log)
        {
            _arbitrageDetectorService = arbitrageDetectorService;
            _log = log;
        }

        [HttpGet("orderBooks")]
        [SwaggerOperation("GetOrderBooks")]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ErrorResponse), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetOrderBoooks()
        {
            IDictionary<ExchangeAssetPair, OrderBook> result;

            try
            {
                result = _arbitrageDetectorService.GetOrderBooks();
            }
            catch (Exception exception)
            {
                await _log.WriteErrorAsync(nameof(ArbitrageDetectorController), nameof(GetOrderBoooks), "", exception);

                return BadRequest(ErrorResponse.Create(exception.Message));
            }

            return Ok(result);
        }

        [HttpGet("crossRates")]
        [SwaggerOperation("GetCrossRates")]
        [ProducesResponseType(typeof(IDictionary<ExchangeAssetPair, CrossRate>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ErrorResponse), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetCrossRates()
        {
            IDictionary<ExchangeAssetPair, CrossRate> result;

            try
            {
                result = _arbitrageDetectorService.GetCrossRates();
            }
            catch (Exception exception)
            {
                await _log.WriteErrorAsync(nameof(ArbitrageDetectorController), nameof(GetCrossRates), "", exception);

                return BadRequest(ErrorResponse.Create(exception.Message));
            }

            return Ok(result);
        }

        [HttpGet("arbitrages")]
        [SwaggerOperation("GetArbitrages")]
        [ProducesResponseType(typeof(IEnumerable<string>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ErrorResponse), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetArbitrages()
        {
            IEnumerable<string> result;

            try
            {
                result = _arbitrageDetectorService.GetArbitragesStrings();
            }
            catch (Exception exception)
            {
                await _log.WriteErrorAsync(nameof(ArbitrageDetectorController), nameof(GetArbitrages), "", exception);

                return BadRequest(ErrorResponse.Create(exception.Message));
            }

            return Ok(result);
        }
    }
}
