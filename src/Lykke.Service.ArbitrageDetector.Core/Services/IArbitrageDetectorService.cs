using System;
using System.Collections.Generic;
using Lykke.Service.ArbitrageDetector.Core.Domain;

namespace Lykke.Service.ArbitrageDetector.Core.Services
{
    public interface IArbitrageDetectorService
    {
        // Cross Rates

        IEnumerable<SynthOrderBook> GetSynthOrderBooks();

        // Arbitrages

        IEnumerable<Arbitrage> GetArbitrages();

        Arbitrage GetArbitrageFromHistory(string conversionPath);

        Arbitrage GetArbitrageFromActiveOrHistory(string conversionPath);

        IEnumerable<Arbitrage> GetArbitrageHistory(DateTime since, int take);

        // Matrix

        Matrix GetMatrix(string assetPair, bool isPublic = false, bool depositFee = false, bool tradingFee = false);
    }
}
