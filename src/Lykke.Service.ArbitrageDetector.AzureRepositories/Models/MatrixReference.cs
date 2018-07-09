using System;
using Common;
using Lykke.AzureStorage.Tables;
using Lykke.Service.ArbitrageDetector.Core.Domain;

namespace Lykke.Service.ArbitrageDetector.AzureRepositories.Models
{
    public class MatrixReference : AzureTableEntity
    {
        public MatrixReference()
        {
        }

        public MatrixReference(Matrix matrix)
        {
            PartitionKey = GeneratePartitionKey(matrix.AssetPair, matrix.DateTime);
            RowKey = GenerateRowKey(matrix.DateTime);
        }

        public static string GeneratePartitionKey(string assetPair, DateTime date)
        {
            return $"{assetPair} {date.ToIsoDate()}";
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
            return $"{assetPair} {dateTime.ToIsoDateTime()}";
        }
    }
}
