﻿using System;
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

        [Get("/orderBook")]
        Task<OrderBook> OrderBookAsync(string exchange, string assetPair);

        [Get("/synthOrderBooks")]
        Task<IEnumerable<SynthOrderBookRow>> SynthOrderBooksAsync();

        [Get("/arbitrages")]
        Task<IEnumerable<ArbitrageRow>> ArbitragesAsync();

        [Get("/arbitrageFromHistory")]
        Task<Arbitrage> ArbitrageFromHistoryAsync(string conversionPath);

        [Get("/arbitrageFromActiveOrHistory")]
        Task<Arbitrage> ArbitrageFromActiveOrHistoryAsync(string conversionPath);

        [Get("/arbitrageHistory")]
        Task<IEnumerable<ArbitrageRow>> ArbitrageHistory(DateTime since, int take);

        [Get("/matrix")]
        Task<Matrix> Matrix(string assetPair, bool depositFee = false, bool tradingFee = false);

        [Get("/publicMatrix")]
        Task<Matrix> PublicMatrix(string assetPair, bool depositFee = false, bool tradingFee = false);

        [Get("/publicMatrixAssetPairs")]
        Task<IEnumerable<string>> PublicMatrixAssetPairs();

        [Get("/lykkeArbitrages")]
        Task<IEnumerable<LykkeArbitrageRow>> LykkeArbitrages(string basePair, string crossPair, string target = "", string source = "", ArbitrageProperty property = default, decimal minValue = 0);

        [Get("/matrixHistory/stamps")]
        Task<IEnumerable<DateTime>> MatrixHistoryStamps(string assetPair, DateTime date, bool lykkeArbitragesOnly);

        [Get("/matrixHistory/assetPairs")]
        Task<IEnumerable<string>> MatrixHistoryAssetPairs(DateTime date, bool lykkeArbitragesOnly);

        [Get("/matrixHistory/matrix")]
        Task<Matrix> MatrixHistory(string assetPair, DateTime dateTime);

        [Get("/getSettings")]
        Task<Settings> GetSettings();

        [Post("/setSettings")]
        Task SetSettings(Settings settings);
    }
}
