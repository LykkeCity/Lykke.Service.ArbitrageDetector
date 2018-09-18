using System;
using System.Collections.Generic;

namespace Lykke.Service.ArbitrageDetector.AzureRepositories.Models
{
    public sealed class MatrixBlob
    {
        public string AssetPair { get; set; }

        public IList<Exchange> Exchanges { get; set; } = new List<Exchange>();

        public IList<decimal?> Asks { get; set; } = new List<decimal?>();

        public IList<decimal?> Bids { get; set; } = new List<decimal?>();

        public IList<IList<MatrixCell>> Cells { get; set; } = new List<IList<MatrixCell>>();

        public DateTime DateTime { get; set; }

        public override string ToString()
        {
            return $"{AssetPair} - {DateTime}";
        }
    }
}
