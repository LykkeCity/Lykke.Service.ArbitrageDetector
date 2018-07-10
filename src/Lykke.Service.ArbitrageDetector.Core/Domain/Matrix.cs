using System;
using System.Collections.Generic;

namespace Lykke.Service.ArbitrageDetector.Core.Domain
{
    public sealed class Matrix
    {
        public string AssetPair { get; set; }

        public IList<Exchange> Exchanges { get; set; } = new List<Exchange>();

        public IList<decimal?> Asks { get; set; } = new List<decimal?>();

        public IList<decimal?> Bids { get; set; } = new List<decimal?>();

        public IList<IList<MatrixCell>> Cells { get; set; } = new List<IList<MatrixCell>>();

        public DateTime DateTime { get; set; }

        public Matrix(string assetPair)
        {
            AssetPair = string.IsNullOrEmpty(assetPair) ? throw new ArgumentNullException(nameof(assetPair)) : assetPair;
        }

        public override string ToString()
        {
            return $"{AssetPair} - {DateTime}";
        }
    }
}
