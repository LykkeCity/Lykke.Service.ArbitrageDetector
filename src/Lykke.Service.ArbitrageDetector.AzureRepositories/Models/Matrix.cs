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

        public IList<decimal?> Asks { get; set; }

        public IList<decimal?> Bids { get; set; }

        [JsonValueSerializer]
        public IList<IList<IMatrixCell>> Cells { get; set; }

        public Matrix()
        {
        }

        public Matrix(IMatrix domain)
        {
            AssetPair = domain.AssetPair; // PartitionKey
            RowKey = DateTime.UtcNow.ToString(CultureInfo.InvariantCulture);

            Exchanges = domain.Exchanges;
            Asks = domain.Asks;
            Bids = domain.Bids;
            Cells = domain.Cells;
        }
    }
}
