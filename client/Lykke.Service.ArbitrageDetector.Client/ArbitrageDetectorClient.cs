using System;
using Common.Log;

namespace Lykke.Service.ArbitrageDetector.Client
{
    public class ArbitrageDetectorClient : IArbitrageDetectorClient, IDisposable
    {
        private readonly ILog _log;

        public ArbitrageDetectorClient(string serviceUrl, ILog log)
        {
            _log = log;
        }

        public void Dispose()
        {
            //if (_service == null)
            //    return;
            //_service.Dispose();
            //_service = null;
        }
    }
}
