using System;
using System.Collections.Generic;
using Lykke.Service.ArbitrageDetector.Core.Domain;

namespace Lykke.Service.ArbitrageDetector.Core.Services
{
    public interface IArbitrageDetectorService
    {
        void Process(OrderBook orderBook);

        // Order Books

        IEnumerable<OrderBook> GetOrderBooks(string exchange, string assetPair);

        OrderBook GetOrderBook(string exchange, string assetPair);

        // Cross Rates

        IEnumerable<SynthOrderBook> GetSynthOrderBooks();

        // Arbitrages

        IEnumerable<Arbitrage> GetArbitrages();

        Arbitrage GetArbitrageFromHistory(string conversionPath);

        Arbitrage GetArbitrageFromActiveOrHistory(string conversionPath);

        IEnumerable<Arbitrage> GetArbitrageHistory(DateTime since, int take);

        // Matrix

        Matrix GetMatrix(string assetPair, bool isPublic = false);

        // Settings

        ISettings GetSettings();

        void SetSettings(ISettings settings);
    }
}
