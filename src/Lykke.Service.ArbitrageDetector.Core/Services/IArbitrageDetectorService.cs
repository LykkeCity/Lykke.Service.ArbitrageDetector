using System;
using System.Collections.Generic;
using Lykke.Service.ArbitrageDetector.Core.Domain;

namespace Lykke.Service.ArbitrageDetector.Core.Services
{
    public interface IArbitrageDetectorService
    {
        void Process(OrderBook orderBook);

        // Order Books

        IEnumerable<OrderBook> GetOrderBooks();

        IEnumerable<OrderBook> GetOrderBooks(string exchange, string instrument);

        // Cross Rates

        IEnumerable<CrossRate> GetCrossRates();

        // Arbitrages

        IEnumerable<Arbitrage> GetArbitrages();

        Arbitrage GetArbitrageFromHistory(string conversionPath);

        Arbitrage GetArbitrageFromActiveOrHistory(string conversionPath);

        IEnumerable<Arbitrage> GetArbitrageHistory(DateTime since, int take);

        // Matrix

        Matrix GetMatrix(string assetPair, bool withSuffixOnly = false);

        // For lykke.com

        Matrix GetPublicMatrix(string assetPair);

        IEnumerable<string> GetPublicMatrixAssetPairs();

        void SetPublicMatrixAssetPairs(IEnumerable<string> assetPairs);


        Settings GetSettings();

        void SetSettings(Settings settings);
    }
}
