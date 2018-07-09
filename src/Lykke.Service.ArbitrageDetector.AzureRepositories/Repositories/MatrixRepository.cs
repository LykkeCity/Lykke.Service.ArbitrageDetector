using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AzureStorage;
using Lykke.Service.ArbitrageDetector.AzureRepositories.Models;
using Lykke.Service.ArbitrageDetector.Core.Domain.Interfaces;
using Lykke.Service.ArbitrageDetector.Core.Repositories;
using Microsoft.WindowsAzure.Storage.Table;

namespace Lykke.Service.ArbitrageDetector.AzureRepositories.Repositories
{
    public class MatrixRepository : IMatrixRepository
    {
        private readonly INoSQLTableStorage<Matrix> _storage;

        public MatrixRepository(INoSQLTableStorage<Matrix> storage)
        {
            _storage = storage;
        }

        public async Task<IMatrix> GetAsync(string assetPair, DateTime dateTime)
        {
            return await _storage.GetDataAsync(assetPair, dateTime.Ticks.ToString());
        }

        public async Task<IEnumerable<IMatrix>> GetByAssetPairAndDateAsync(string assetPair, DateTime date)
        {
            var query = new TableQuery<Matrix>().Where(
                    TableQuery.CombineFilters(
                        TableQuery.GenerateFilterConditionForDate(nameof(Matrix.DateTime), QueryComparisons.GreaterThanOrEqual, date.Date),
                        TableOperators.And,
                        TableQuery.GenerateFilterConditionForDate(nameof(Matrix.DateTime), QueryComparisons.LessThan, date.AddDays(1).Date))
                    );
            //query.Where(TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, assetPair));

            return await _storage.WhereAsync(query);

            //return await _storage.WhereAsync(assetPair, date.Date, date.AddDays(1).Date, ToIntervalOption.ExcludeTo);

            //return await _storage.GetDataAsync(assetPair, x => x.DateTime > date.Date);
        }

        public async Task<IEnumerable<IMatrix>> GetDateTimesOnlyByAssetPairAndDateAsync(string assetPair, DateTime date)
        {
            var query = new TableQuery<Matrix>();
            query.Select(new [] { nameof(Matrix.DateTime) });

            return await _storage.WhereAsync(query, x => x.AssetPair == assetPair && x.DateTime.Date == date.Date);
        }

        public async Task InsertAsync(IMatrix matrix)
        {
            await _storage.InsertAsync(new Matrix(matrix));
        }

        public async Task<bool> DeleteAsync(string assetPair, DateTime dateTime)
        {
            return await _storage.DeleteIfExistAsync(assetPair, dateTime.Ticks.ToString());
        }
    }
}
