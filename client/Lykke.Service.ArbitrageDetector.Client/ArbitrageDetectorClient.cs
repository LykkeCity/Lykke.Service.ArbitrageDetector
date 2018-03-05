using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Lykke.Service.ArbitrageDetector.Client.AutorestClient;
using Lykke.Service.ArbitrageDetector.Client.AutorestClient.Models;

namespace Lykke.Service.ArbitrageDetector.Client
{
    /// <summary>
    /// Contains methods for work with arbitrage detector service.
    /// </summary>
    public class ArbitrageDetectorClient : IArbitrageDetectorClient, IDisposable
    {
        private ArbitrageDetectorAPI _service;

        /// <summary>
        /// Initializes a new instance of the <see cref="ArbitrageDetectorClient"/> class.
        /// </summary>
        /// <param name="serviceUrl">The arbitrage detector service url.</param>
        public ArbitrageDetectorClient(string serviceUrl)
        {
            _service = new ArbitrageDetectorAPI(new Uri(serviceUrl));
        }
        
        public void Dispose()
        {
            if (_service == null)
                return;
            _service.Dispose();
            _service = null;
        }

        public async Task<IEnumerable<OrderBook>> GetOrderBooks()
        {
            var result = await _service.GetOrderBooksAsync();

            return null;
            //return result;
        }

        public async Task<IEnumerable<CrossRate>> GetCrossRates()
        {
            return null;
        }

        public async Task<IEnumerable<Arbitrage>> GetArbitrages()
        {
            return null;
        }
    }
}
