using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using AutoMapper;
using Lykke.Service.ArbitrageDetector.Client.Models;
using Lykke.Service.ArbitrageDetector.Core.Services;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Lykke.Service.ArbitrageDetector.Controllers
{
    public class ArbitrageDetectorController : Controller
    {
        private readonly IOrderBooksService _orderBookService;
        private readonly IArbitrageDetectorService _arbitrageDetectorService;
        private readonly ILykkeArbitrageDetectorService _lykkeArbitrageDetectorService;
        private readonly IMatrixHistoryService _matrixHistoryService;
        private readonly ISettingsService _settingsService;

        public ArbitrageDetectorController(IOrderBooksService orderBookService,
            IArbitrageDetectorService arbitrageDetectorService, 
            ILykkeArbitrageDetectorService lykkeArbitrageDetectorService,
            IMatrixHistoryService matrixHistoryService,
            ISettingsService settingsService)
        {
            _orderBookService = orderBookService;
            _arbitrageDetectorService = arbitrageDetectorService;
            _lykkeArbitrageDetectorService = lykkeArbitrageDetectorService;
            _matrixHistoryService = matrixHistoryService;
            _settingsService = settingsService;
        }

        [HttpGet]
        [Route("orderBooks")]
        [SwaggerOperation("OrderBooks")]
        [ProducesResponseType(typeof(IEnumerable<OrderBookRow>), (int)HttpStatusCode.OK)]
        [ResponseCache(Duration = 1, VaryByQueryKeys = new[] { "*" })]
        public virtual IActionResult OrderBooks(string exchange, string assetPair)
        {
            var domain = _orderBookService.GetOrderBooks(exchange, assetPair);
            var model = Mapper.Map<IReadOnlyList<OrderBookRow>>(domain);

            return Ok(model);
        }

        [HttpGet]
        [Route("orderBook")]
        [SwaggerOperation("OrderBook")]
        [ProducesResponseType(typeof(OrderBook), (int)HttpStatusCode.OK)]
        [ResponseCache(Duration = 1, VaryByQueryKeys = new[] { "*" })]
        public virtual IActionResult OrderBook(string exchange, string assetPair)
        {
            var domain = _orderBookService.GetOrderBook(exchange, assetPair);
            var model = Mapper.Map<OrderBook>(domain);

            return Ok(model);
        }

        [HttpGet]
        [Route("synthOrderBooks")]
        [SwaggerOperation("SynthOrderBooks")]
        [ProducesResponseType(typeof(IEnumerable<SynthOrderBookRow>), (int)HttpStatusCode.OK)]
        [ResponseCache(Duration = 1)]
        public IActionResult SynthOrderBooks()
        {
            var domain = _arbitrageDetectorService.GetSynthOrderBooks().ToList();
            var model = Mapper.Map<IReadOnlyList<SynthOrderBookRow>>(domain);

            return Ok(model);
        }

        [HttpGet]
        [Route("arbitrages")]
        [SwaggerOperation("Arbitrages")]
        [ProducesResponseType(typeof(IEnumerable<ArbitrageRow>), (int)HttpStatusCode.OK)]
        [ResponseCache(Duration = 1)]
        public IActionResult Arbitrages()
        {
            var domain = _arbitrageDetectorService.GetArbitrages().ToList();
            var model = Mapper.Map<IReadOnlyList<ArbitrageRow>>(domain);

            return Ok(model);
        }

        [HttpGet]
        [Route("arbitrageFromHistory")]
        [SwaggerOperation("ArbitrageFromHistory")]
        [ProducesResponseType(typeof(Arbitrage), (int)HttpStatusCode.OK)]
        [ResponseCache(Duration = 1, VaryByQueryKeys = new[] { "*" })]
        public IActionResult ArbitrageFromHistory(string conversionPath)
        {
            var domain = _arbitrageDetectorService.GetArbitrageFromHistory(conversionPath);
            var model = Mapper.Map<Arbitrage>(domain);

            return Ok(model);
        }

        [HttpGet]
        [Route("arbitrageFromActiveOrHistory")]
        [SwaggerOperation("ArbitrageFromActiveOrHistory")]
        [ProducesResponseType(typeof(Arbitrage), (int)HttpStatusCode.OK)]
        [ResponseCache(Duration = 1, VaryByQueryKeys = new[] { "*" })]
        public IActionResult ArbitrageFromActiveOrHistory(string conversionPath)
        {
            var domain = _arbitrageDetectorService.GetArbitrageFromActiveOrHistory(conversionPath);
            var model = Mapper.Map<Arbitrage>(domain);

            return Ok(model);
        }

        [HttpGet]
        [Route("arbitrageHistory")]
        [SwaggerOperation("ArbitrageHistory")]
        [ProducesResponseType(typeof(IEnumerable<ArbitrageRow>), (int)HttpStatusCode.OK)]
        [ResponseCache(Duration = 1, VaryByQueryKeys = new[] { "*" })]
        public IActionResult ArbitrageHistory(DateTime since, int take)
        {
            var domain = _arbitrageDetectorService.GetArbitrageHistory(since, take).ToList();
            var model = Mapper.Map<IReadOnlyList<ArbitrageRow>>(domain);

            return Ok(model);
        }

        [HttpGet]
        [Route("matrix")]
        [SwaggerOperation("Matrix")]
        [ProducesResponseType(typeof(Matrix), (int)HttpStatusCode.OK)]
        [ResponseCache(Duration = 1, VaryByQueryKeys = new[] { "*" })]
        public IActionResult Matrix(string assetPair, bool depositFee = false, bool tradingFee = false)
        {
            if (string.IsNullOrWhiteSpace(assetPair))
                return NotFound();

            var domain = _arbitrageDetectorService.GetMatrix(assetPair, false, depositFee, tradingFee);
            var model = Mapper.Map<Matrix>(domain);

            return Ok(model);
        }

        [HttpGet]
        [Route("publicMatrix")]
        [SwaggerOperation("PublicMatrix")]
        [ProducesResponseType(typeof(Matrix), (int)HttpStatusCode.OK)]
        [ResponseCache(Duration = 1, VaryByQueryKeys = new [] { "*" })]
        public IActionResult PublicMatrix(string assetPair, bool depositFee = false, bool tradingFee = false)
        {
            if (string.IsNullOrWhiteSpace(assetPair))
                return NotFound();

            var domain = _arbitrageDetectorService.GetMatrix(assetPair, true, depositFee, tradingFee);
            var model = Mapper.Map<Matrix>(domain);

            return Ok(model);
        }

        [HttpGet]
        [Route("publicMatrixAssetPairs")]
        [SwaggerOperation("PublicMatrixAssetPairs")]
        [ProducesResponseType(typeof(IEnumerable<string>), (int)HttpStatusCode.OK)]
        [ResponseCache(Duration = 1)]
        public IActionResult PublicMatrixAssetPairs()
        {
            var settings = _settingsService.GetAsync().GetAwaiter().GetResult();
            var result = settings.PublicMatrixAssetPairs;

            return Ok(result);
        }

        [HttpGet]
        [Route("lykkeArbitrages")]
        [SwaggerOperation("LykkeArbitrages")]
        [ProducesResponseType(typeof(IEnumerable<LykkeArbitrageRow>), (int)HttpStatusCode.OK)]
        [ResponseCache(Duration = 1, VaryByQueryKeys = new[] { "*" })]
        public IActionResult LykkeArbitrages(string basePair, string crossPair, string target = "", string source = "", ArbitrageProperty property = default, decimal minValue = 0)
        {
            target = string.IsNullOrWhiteSpace(target) ? basePair : target;
            source = string.IsNullOrWhiteSpace(source) ? crossPair : source;

            var domain = _lykkeArbitrageDetectorService.GetArbitrages(target, source, (Core.Domain.ArbitrageProperty)property, minValue).ToList();
            var model = Mapper.Map<IReadOnlyList<LykkeArbitrageRow>>(domain);

            return Ok(model);
        }

        [HttpGet]
        [Route("matrixHistory/stamps")]
        [SwaggerOperation("MatrixHistoryStamps")]
        [ProducesResponseType(typeof(IEnumerable<DateTime>), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> MatrixHistoryStamps(string assetPair, DateTime date, bool lykkeArbitragesOnly)
        {
            var result = (await _matrixHistoryService.GetStampsAsync(assetPair, date, lykkeArbitragesOnly)).ToList();

            return Ok(result);
        }

        [HttpGet]
        [Route("matrixHistory/assetPairs")]
        [SwaggerOperation("MatrixHistoryAssetPairs")]
        [ProducesResponseType(typeof(IEnumerable<string>), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> MatrixHistoryAssetPairs(DateTime date, bool lykkeArbitragesOnly)
        {
            var result = (await _matrixHistoryService.GetAssetPairsAsync(date, lykkeArbitragesOnly)).ToList();

            return Ok(result);
        }

        [HttpGet]
        [Route("matrixHistory/matrix")]
        [SwaggerOperation("MatrixHistory")]
        [ProducesResponseType(typeof(Matrix), (int)HttpStatusCode.OK)]
        [ResponseCache(Duration = 60 * 60 * 4, VaryByQueryKeys = new[] { "*" })] // 4 hours
        public async Task<IActionResult> MatrixHistory(string assetPair, DateTime dateTime)
        {
            var domain = await _matrixHistoryService.GetAsync(assetPair, dateTime);
            var model = Mapper.Map<Matrix>(domain);

            return Ok(model);
        }

        [HttpGet]
        [Route("getSettings")]
        [SwaggerOperation("GetSettings")]
        [ProducesResponseType(typeof(Client.Models.Settings), (int)HttpStatusCode.OK)]
        public IActionResult GetSettings()
        {
            var domain = _settingsService.GetAsync().GetAwaiter().GetResult();
            var model = Mapper.Map<Client.Models.Settings>(domain);

            return Ok(model);
        }

        [HttpPost]
        [Route("setSettings")]
        [SwaggerOperation("SetSettings")]
        [ProducesResponseType(typeof(void), (int)HttpStatusCode.OK)]
        public IActionResult SetSettings([FromBody]Client.Models.Settings settings)
        {
            var domain = Mapper.Map<Core.Domain.Settings>(settings);
            _settingsService.SetAsync(domain).GetAwaiter().GetResult();

            return Ok();
        }
    }
}
