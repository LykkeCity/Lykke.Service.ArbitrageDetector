using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Lykke.Service.ArbitrageDetector.Client.Models;

namespace Lykke.Service.ArbitrageDetector.Client
{
    /// <summary>
    /// HTTP client for arbitrage detector service.
    /// </summary>
    public interface IArbitrageDetectorService
    {
        /// <summary>
        /// Returns a collection of OrderBook entities by exchange and instrument.
        /// </summary>
        /// <param name="exchange">Name of an exchange.</param>
        /// <param name="assetPair">Name of an instrument</param>
        /// <returns>A collection of OrderBook entities.</returns>
        Task<IEnumerable<OrderBookRow>> OrderBooksAsync(string exchange, string assetPair);

        /// <summary>
        /// Returns a collection of OrderBook entities by exchange and instrument.
        /// </summary>
        /// <param name="exchange">Name of an exchange.</param>
        /// <param name="assetPair">Name of an instrument</param>
        /// <returns>A collection of OrderBook entities.</returns>
        Task<IEnumerable<OrderBookRow>> NewOrderBooksAsync(string exchange, string assetPair);

        /// <summary>
        /// Returns a collection of CrossRate entities.
        /// </summary>
        /// <returns>A collection of CrossRate entities.</returns>
        Task<IEnumerable<CrossRateRow>> CrossRatesAsync();

        /// <summary> 
        /// Returns a collection of Arbitrage entities.
        /// </summary>
        /// <returns>A collection of Arbitrage entities.</returns>
        Task<IEnumerable<ArbitrageRow>> ArbitragesAsync();

        /// <summary>
        /// Returns arbitrage from history.
        /// </summary>
        /// <param name="conversionPath"></param>
        /// <returns></returns>
        Task<Arbitrage> ArbitrageFromHistoryAsync(string conversionPath);

        /// <summary>
        /// Returns an arbitrage from active arbitrages.
        /// If not exists then returns the best from the history.
        /// </summary>
        /// <param name="conversionPath"></param>
        /// <returns></returns>
        Task<Arbitrage> ArbitrageFromActiveOrHistoryAsync(string conversionPath);

        /// <summary>
        /// Returns a collection of ArbitrageHistory entities.
        /// </summary>
        /// <param name="since"></param>
        /// <param name="take"></param>
        /// <returns>A collection of Arbitrage entities.</returns>
        Task<IEnumerable<ArbitrageRow>> ArbitrageHistoryAsync(DateTime since, int take);

        /// <summary>
        /// Returns arbitrage matrix.
        /// </summary>
        /// <returns>Arbitrage matrix.</returns>
        Task<Matrix> MatrixAsync(string assetPair);

        /// <summary>
        /// Returns public arbitrages matrix.
        /// </summary>
        /// <returns>Arbitrage matrix.</returns>
        Task<Matrix> PublicMatrixAsync(string assetPair);

        /// <summary>
        /// Returns public arbitrages matrix asset pairs.
        /// </summary>
        /// <returns>Arbitrage matrix.</returns>
        Task<IEnumerable<string>> PublicMatrixAssetPairsAsync();

        /// <summary>
        /// Returns a collection of LykkeArbitrageRow entities.
        /// </summary>
        /// <returns>A collection of Lykke Arbitrage entities.</returns>
        Task<IEnumerable<LykkeArbitrageRow>> LykkeArbitragesAsync(string basePair, string crossPair);

        /// <summary>
        /// Get matrix datetime stamps by date.
        /// </summary>
        /// <param name="assetPair"></param>
        /// <param name="date"></param>
        /// <returns></returns>
        Task<IEnumerable<DateTime>> MatrixHistoryStamps(string assetPair, DateTime date);

        /// <summary>
        /// Get available asset pairs by date.
        /// </summary>
        /// <param name="date"></param>
        /// <returns></returns>
        Task<IEnumerable<string>> MatrixHistoryAssetPairs(DateTime date);

        /// <summary>
        /// Get matrix snapshot.
        /// </summary>
        /// <param name="assetPair"></param>
        /// <param name="dateTime"></param>
        /// <returns></returns>
        Task<Matrix> MatrixHistory(string assetPair, DateTime dateTime);

        /// <summary>
        /// Get settings.
        /// </summary>
        Task<Settings> GetSettingsAsync();

        /// <summary>
        /// Set new settings.
        /// </summary>
        /// <param name="settings"></param>
        Task SetSettingsAsync(Settings settings);
    }
}
