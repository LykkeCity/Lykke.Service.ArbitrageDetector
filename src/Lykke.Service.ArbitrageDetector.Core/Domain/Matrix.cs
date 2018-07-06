using System;
using System.Collections.Generic;
using Lykke.Service.ArbitrageDetector.Core.Domain.Interfaces;

namespace Lykke.Service.ArbitrageDetector.Core.Domain
{
    public sealed class Matrix : IMatrix
    {
        public string AssetPair { get; set; }

        public IList<Exchange> Exchanges { get; set; } = new List<Exchange>();

        public IList<decimal?> Asks { get; set; } = new List<decimal?>();

        public IList<decimal?> Bids { get; set; } = new List<decimal?>();

        public IList<IList<MatrixCell>> Cells { get; set; } = new List<IList<MatrixCell>>();

        public Matrix(string assetPair)
        {
            AssetPair = string.IsNullOrEmpty(assetPair) ? throw new ArgumentNullException(nameof(assetPair)) : assetPair;
        }
    }
}
