using System;
using System.Collections.Generic;
using System.Linq;
using Common;
using Lykke.AzureStorage.Tables;
using Lykke.AzureStorage.Tables.Entity.Annotation;
using Lykke.Service.ArbitrageDetector.Core.Domain;

namespace Lykke.Service.ArbitrageDetector.AzureRepositories.Models
{
    public class MatrixEntity : AzureTableEntity
    {
        public const string LykkeExchangeMustContain = "LYKKE";
        public string AssetPair => PartitionKey.Split(" - ")[0];

        /// <summary>
        /// Timestamp for current matrix.
        /// </summary>
        public DateTime DateTime => DateTime.Parse(RowKey);

        /// <summary>
        /// Our exchanges (like "lykke" or "lykke(e)") with minimum spread.
        /// </summary>
        [JsonValueSerializer]
        public Dictionary<string, decimal?> OurExchangesMinSpreads { get; set; } = new Dictionary<string, decimal?>();


        public MatrixEntity()
        {
        }

        public MatrixEntity(Matrix matrix)
        {
            PartitionKey = GeneratePartitionKey(matrix.AssetPair, matrix.DateTime);
            RowKey = GenerateRowKey(matrix.DateTime);

            var lykkeExchanges = matrix.Exchanges.Where(x => x.Name.ToUpper().Contains(LykkeExchangeMustContain)).ToList();
            foreach (var lykkeExchange in lykkeExchanges)
            {
                var minSpread = matrix.GetLowestSpread(lykkeExchange.Name);
                OurExchangesMinSpreads.Add(lykkeExchange.Name, minSpread);
            }
        }

        public static string GeneratePartitionKey(string assetPair, DateTime date)
        {
            return $"{assetPair} - {date.ToIsoDate()}";
        }

        public static string GenerateRowKey(DateTime dateTime)
        {
            return $"{dateTime.ToIsoDateTime()}";
        }

        public static string GenerateBlobId(Matrix domain)
        {
            return GenerateBlobId(domain.AssetPair, domain.DateTime);
        }

        public static string GenerateBlobId(string assetPair, DateTime dateTime)
        {
            return $"{assetPair} - {dateTime.ToIsoDateTime()}";
        }
    }
}
