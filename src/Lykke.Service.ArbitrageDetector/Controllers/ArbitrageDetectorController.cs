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
        [SwaggerOperation("GetOrderBooks")]
        [ProducesResponseType(typeof(IEnumerable<OrderBook>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ErrorResponse), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetOrderBooks()
        {
            IEnumerable<OrderBook> result;

            try
            {
                result = _arbitrageDetectorService.GetOrderBooks();
            }
            catch (Exception exception)
            {
                await _log.WriteErrorAsync(nameof(ArbitrageDetectorController), nameof(GetOrderBooks), "", exception);

                return BadRequest(ErrorResponse.Create(exception.Message));
            }

            return Ok(result);
        }

        [HttpGet]
        [Route("orderBooks/exchange/{exchange}")]
        [SwaggerOperation("GetOrderBooksByExchange")]
        [ProducesResponseType(typeof(IEnumerable<CrossRate>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ErrorResponse), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetOrderBooksByExchange(string exchange)
        {
            IEnumerable<OrderBook> result;

            try
            {
                result = _arbitrageDetectorService.GetOrderBooksByExchange(exchange);
            }
            catch (Exception exception)
            {
                await _log.WriteErrorAsync(nameof(ArbitrageDetectorController), nameof(GetOrderBooksByExchange), "", exception);

                return BadRequest(ErrorResponse.Create(exception.Message));
            }

            return Ok(result);
        }

        [HttpGet]
        [Route("orderBooks/instrument/{instrument}")]
        [SwaggerOperation("GetOrderBooksByInstrument")]
        [ProducesResponseType(typeof(IEnumerable<CrossRate>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ErrorResponse), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetOrderBooksByInstrument(string instrument)
        {
            IEnumerable<OrderBook> result;

            try
            {
                result = _arbitrageDetectorService.GetOrderBooksByInstrument(instrument);
            }
            catch (Exception exception)
            {
                await _log.WriteErrorAsync(nameof(ArbitrageDetectorController), nameof(GetOrderBooksByExchange), "", exception);

                return BadRequest(ErrorResponse.Create(exception.Message));
            }

            return Ok(result);
        }

        [HttpGet]
        [Route("orderBooks/exchange/{exchange}/instrument/{instrument}")]
        [SwaggerOperation("GetOrderBooks")]
        [ProducesResponseType(typeof(IEnumerable<CrossRate>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ErrorResponse), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetOrderBooks(string exchange, string instrument)
        {
            IEnumerable<OrderBook> result;

            try
            {
                result = _arbitrageDetectorService.GetOrderBooks(exchange, instrument);
            }
            catch (Exception exception)
            {
                await _log.WriteErrorAsync(nameof(ArbitrageDetectorController), nameof(GetOrderBooksByExchange), "", exception);

                return BadRequest(ErrorResponse.Create(exception.Message));
            }

            return Ok(result);
        }

        [HttpGet]
        [Route("crossRates")]
        [SwaggerOperation("GetCrossRates")]
        [ProducesResponseType(typeof(IEnumerable<CrossRate>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ErrorResponse), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetCrossRates()
        {
            IEnumerable<CrossRate> result;

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

        [HttpGet]
        [Route("arbitrages")]
        [SwaggerOperation("GetArbitrages")]
        [ProducesResponseType(typeof(IEnumerable<Arbitrage>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ErrorResponse), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetArbitrages()
        {
            IEnumerable<Arbitrage> result;

            try
            {
                result = _arbitrageDetectorService.GetArbitrages();
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
