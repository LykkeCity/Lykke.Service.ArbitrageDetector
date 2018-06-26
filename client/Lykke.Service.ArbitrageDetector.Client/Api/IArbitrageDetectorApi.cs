using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Lykke.Service.ArbitrageDetector.Client.Models;
using Refit;

namespace Lykke.Service.ArbitrageDetector.Client.Api
{
    internal interface IArbitrageDetectorApi
    {
        [Get("/orderBooks")]
        Task<IEnumerable<OrderBookRow>> OrderBooksAsync(string exchange, string assetPair);

        [Get("/newOrderBooks")]
        Task<IEnumerable<OrderBookRow>> NewOrderBooksAsync(string exchange, string assetPair);

        [Get("/crossRates")]
        Task<IEnumerable<CrossRateRow>> CrossRatesAsync();

        [Get("/arbitrages")]
        Task<IEnumerable<ArbitrageRow>> ArbitragesAsync();

        [Get("/arbitrageFromHistory")]
        Task<Arbitrage> ArbitrageFromHistoryAsync(string conversionPath);

        [Get("/arbitrageFromActiveOrHistory")]
        Task<Arbitrage> ArbitrageFromActiveOrHistoryAsync(string conversionPath);

        [Get("/arbitrageHistory")]
        Task<IEnumerable<ArbitrageRow>> ArbitrageHistory(DateTime since, int take);

        [Get("/matrix")]
        Task<Matrix> Matrix(string assetPair);

        [Get("/publicMatrix")]
        Task<Matrix> PublicMatrix(string assetPair);

        [Get("/publicMatrixAssetPairs")]
        Task<IEnumerable<string>> PublicMatrixAssetPairs();

        [Get("/lykkeArbitrages")]
        Task<IEnumerable<LykkeArbitrageRow>> LykkeArbitrages(string basePair, string crossPair);

        [Get("/getSettings")]
        Task<Settings> GetSettings();

        [Post("/setSettings")]
        Task SetSettings(Settings settings);
    }
}
