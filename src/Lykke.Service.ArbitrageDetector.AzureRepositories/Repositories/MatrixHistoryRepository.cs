using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AzureStorage;
using AzureStorage.Tables;
using Common.Log;
using Lykke.Service.ArbitrageDetector.AzureRepositories.Models;
using Lykke.Service.ArbitrageDetector.Core.Domain;
using Lykke.Service.ArbitrageDetector.Core.Repositories;
using Lykke.SettingsReader;
using Microsoft.WindowsAzure.Storage.Table;

namespace Lykke.Service.ArbitrageDetector.AzureRepositories.Repositories
{
    public class MatrixHistoryRepository : IMatrixHistoryRepository
    {
        private const string TableName = "MatrixHistory";
        private readonly MatrixHistoryBlobRepository _blobRepository;
        private readonly INoSQLTableStorage<MatrixEntity> _storage;

        public MatrixHistoryRepository(MatrixHistoryBlobRepository matrixHistoryBlobRepository, IReloadingManager<string> connectionString, ILog log)
        {
            _storage = AzureTableStorage<MatrixEntity>.Create(connectionString, TableName, log);
            _blobRepository = matrixHistoryBlobRepository ?? throw new ArgumentNullException(nameof(matrixHistoryBlobRepository));
        }

        public async Task InsertAsync(Matrix matrix)
        {
            var entity = new MatrixEntity(matrix);
            await _storage.InsertAsync(entity);

            var blob = new MatrixBlob(matrix);
            await _blobRepository.SaveAsync(blob);
        }

        public async Task<Matrix> GetAsync(string assetPair, DateTime dateTime)
        {
            var result = await _blobRepository.GetAsync(assetPair, dateTime);

            return result?.Matrix();
        }

        public async Task<IEnumerable<DateTime>> GetDateTimeStampsAsync(string assetPair, DateTime date, decimal? maxSpread, IReadOnlyCollection<string> exchanges)
        {
            if (string.IsNullOrWhiteSpace(assetPair)) { throw new ArgumentException(nameof(assetPair)); }
            date = date.Date;

            return await GetDateTimeStampsAsync(assetPair, date.Date, date.AddDays(1).Date, maxSpread, exchanges);
        }

        public async Task<IEnumerable<DateTime>> GetDateTimeStampsAsync(string assetPair, DateTime from, DateTime to, decimal? maxSpread, IReadOnlyCollection<string> exchanges)
        {
            var entities = await GetAsync(assetPair, from, to, maxSpread, exchanges);

            return entities.Select(x => x.DateTime);
        }

        public async Task<IEnumerable<string>> GetAssetPairsAsync(DateTime date, decimal? maxSpread, IReadOnlyCollection<string> exchanges)
        {
            date = date.Date;

            var entities = await GetAsync(date.Date, date.AddDays(1).Date, maxSpread, exchanges);

            return entities.Select(x => x.AssetPair).Distinct().OrderBy(x => x).ToList();
        }

        public async Task<bool> DeleteAsync(string assetPair, DateTime dateTime)
        {
            var pkey = MatrixEntity.GeneratePartitionKey(assetPair, dateTime);
            var rowkey = MatrixEntity.GenerateRowKey(dateTime);

            var result = await _storage.DeleteIfExistAsync(pkey, rowkey);
            await _blobRepository.DeleteIfExistsAsync(assetPair, dateTime);

            return result;
        }

        private async Task<IEnumerable<MatrixEntity>> GetAsync(string assetPair, DateTime from, DateTime to, decimal? maxSpread, IReadOnlyCollection<string> exchanges)
        {
            var filteredByAssetPairAndDates = await GetAsync(assetPair, from, to);

            return FilterByMaxSpreadForExchanges(filteredByAssetPairAndDates, maxSpread, exchanges);
        }

        private async Task<IEnumerable<MatrixEntity>> GetAsync(string assetPair, DateTime from, DateTime to)
        {
            if (string.IsNullOrWhiteSpace(assetPair)) { throw new ArgumentException(nameof(assetPair)); }

            var pKeyFrom = MatrixEntity.GeneratePartitionKey(assetPair, from);
            var pKeyTo = MatrixEntity.GeneratePartitionKey(assetPair, to);
            var rowKeyFrom = MatrixEntity.GenerateRowKey(from);
            var rowKeyTo = MatrixEntity.GenerateRowKey(to);

            var query = new TableQuery<MatrixEntity>();

            var pkeyCondFrom = TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.GreaterThanOrEqual, pKeyFrom);
            var pkeyCondTo = TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.LessThanOrEqual, pKeyTo);
            var pkeyFilter = TableQuery.CombineFilters(pkeyCondFrom, TableOperators.And, pkeyCondTo);

            var rowkeyCondFrom = TableQuery.GenerateFilterCondition("RowKey", QueryComparisons.GreaterThanOrEqual, rowKeyFrom);
            var rowkeyCondTo = TableQuery.GenerateFilterCondition("RowKey", QueryComparisons.LessThanOrEqual, rowKeyTo);
            var rowkeyFilter = TableQuery.CombineFilters(rowkeyCondFrom, TableOperators.And, rowkeyCondTo);

            query.FilterString = TableQuery.CombineFilters(pkeyFilter, TableOperators.And, rowkeyFilter);

            return await _storage.WhereAsync(query);
        }

        private async Task<IEnumerable<MatrixEntity>> GetAsync(DateTime from, DateTime to, decimal? maxSpread, IReadOnlyCollection<string> exchanges)
        {
            var filteredByDateRange = await GetAsync(from, to);

            return FilterByMaxSpreadForExchanges(filteredByDateRange, maxSpread, exchanges);
        }

        private async Task<IEnumerable<MatrixEntity>> GetAsync(DateTime from, DateTime to)
        {
            var rowKeyFrom = MatrixEntity.GenerateRowKey(from);
            var rowKeyTo = MatrixEntity.GenerateRowKey(to);

            var query = new TableQuery<MatrixEntity>();

            var rowkeyCondFrom = TableQuery.GenerateFilterCondition("RowKey", QueryComparisons.GreaterThanOrEqual, rowKeyFrom);
            var rowkeyCondTo = TableQuery.GenerateFilterCondition("RowKey", QueryComparisons.LessThanOrEqual, rowKeyTo);
            var rowkeyFilter = TableQuery.CombineFilters(rowkeyCondFrom, TableOperators.And, rowkeyCondTo);

            query.FilterString = rowkeyFilter;

            return await _storage.WhereAsync(query);
        }

        private IEnumerable<MatrixEntity> FilterByMaxSpreadForExchanges(IEnumerable<MatrixEntity> input, decimal? maxSpread, IReadOnlyCollection<string> exchanges)
        {
            if (!maxSpread.HasValue || exchanges == null || !exchanges.Any())
                return input;

            // Filter by maxSpread for selected exchanges
            var result = (from matrix in input
                          from exchangeMinSpread in matrix.OurExchangesMinSpreads
                          from exchange in exchanges
                          where exchangeMinSpread.Key.Equals(exchange, StringComparison.OrdinalIgnoreCase)
                          where exchangeMinSpread.Value.HasValue && exchangeMinSpread.Value.Value < maxSpread.Value
                          select matrix).ToList();

            return result;
        }
    }
}
