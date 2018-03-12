using System;

namespace Lykke.Service.ArbitrageDetector.Core.Domain
{
    public struct ExchangeConversionPath
    {
        public string Exchange { get; }
        
        public string ConversionPath { get; }

        public ExchangeConversionPath(string exchange, string conversionPath)
        {
            Exchange = string.IsNullOrEmpty(exchange) ? throw new ArgumentNullException(nameof(exchange)) : exchange;
            ConversionPath = string.IsNullOrEmpty(conversionPath) ? throw new ArgumentNullException(nameof(conversionPath)) : conversionPath;
        }
    }
}
