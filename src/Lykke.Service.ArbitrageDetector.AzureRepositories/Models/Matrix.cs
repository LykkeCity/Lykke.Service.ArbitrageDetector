using System;
using System.Collections.Generic;
using System.Globalization;
using Lykke.AzureStorage.Tables;
using Lykke.AzureStorage.Tables.Entity.Annotation;
using Lykke.Service.ArbitrageDetector.Core.Domain.Interfaces;

namespace Lykke.Service.ArbitrageDetector.AzureRepositories.Models
{
    public class Matrix : AzureTableEntity, IMatrix
    {
        public string AssetPair
        {
            get => PartitionKey;
            set => PartitionKey = value;
        }

        [JsonValueSerializer]
        public IList<IExchange> Exchanges { get; set; }

        [JsonValueSerializer]
        public IList<decimal?> Asks { get; set; }

        [JsonValueSerializer]
        public IList<decimal?> Bids { get; set; }

        [JsonValueSerializer]
        public IList<IList<IMatrixCell>> Cells { get; set; }

        public Matrix()
        {
        }

        public Matrix(IMatrix domain)
        {
            var dateTime = DateTime.UtcNow;

            AssetPair = domain.AssetPair; // PartitionKey
            RowKey = dateTime.ToString(CultureInfo.InvariantCulture);
            Timestamp = dateTime;

            Exchanges = domain.Exchanges;
            Asks = domain.Asks;
            Bids = domain.Bids;
            Cells = domain.Cells;
        }
    }
}
