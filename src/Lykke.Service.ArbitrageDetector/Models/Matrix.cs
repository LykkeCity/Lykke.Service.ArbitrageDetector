using System;
using System.Collections.Generic;
using System.Linq;
using DomainMatrix = Lykke.Service.ArbitrageDetector.Core.Domain.Matrix;
using DomainMatrixCell = Lykke.Service.ArbitrageDetector.Core.Domain.MatrixCell;

namespace Lykke.Service.ArbitrageDetector.Models
{
    /// <summary>
    /// Represents an arbitrage situation.
    /// </summary>
    public sealed class Matrix
    {
        public string AssetPair { get; set; }

        public IList<Exchange> Exchanges { get; set; } = new List<Exchange>();

        public IList<decimal?> Asks { get; set; } = new List<decimal?>();

        public IList<decimal?> Bids { get; set; } = new List<decimal?>();

        public IList<IList<MatrixCell>> Cells { get; set; } = new List<IList<MatrixCell>>();


        public Matrix(DomainMatrix matrix)
        {
            if (matrix == null)
                throw new ArgumentNullException(nameof(matrix) + "." + nameof(matrix.AssetPair));

            if (string.IsNullOrWhiteSpace(matrix.AssetPair))
                throw new ArgumentOutOfRangeException(nameof(matrix) + "." + nameof(matrix.AssetPair));

            AssetPair = matrix.AssetPair;
            Exchanges = matrix.Exchanges.Select(x => new Exchange(x.Name, x.IsActual)).ToList();
            Bids = matrix.Bids;
            Asks = matrix.Asks;
            foreach (var rows in matrix.Cells)
            {
                var row = new List<MatrixCell>();
                foreach (var cell in rows)
                    if (cell == null)
                        row.Add(null);
                    else
                        row.Add(new MatrixCell(cell.Spread, cell.Volume));

                Cells.Add(row);
            }
        }
    }
}
