using System;
using System.Collections.Generic;
using DomainMatrix = Lykke.Service.ArbitrageDetector.Core.Domain.Matrix;

namespace Lykke.Service.ArbitrageDetector.Models
{
    /// <summary>
    /// Represents an arbitrage situation.
    /// </summary>
    public sealed class Matrix
    {
        public string AssetPair { get; set; }

        public IList<string> Exchanges { get; set; } = new List<string>();

        public IList<decimal?> Asks { get; set; } = new List<decimal?>();

        public IList<decimal?> Bids { get; set; } = new List<decimal?>();


        public MatrixCell[,] Cells { get; set; }

        public IList<IList<MatrixCell>> AnotherCells { get; set; }


        public Matrix(DomainMatrix matrix)
        {
            if (matrix == null)
                throw new ArgumentNullException(nameof(matrix) + "." + nameof(matrix.AssetPair));

            if (string.IsNullOrWhiteSpace(matrix.AssetPair))
                throw new ArgumentOutOfRangeException(nameof(matrix) + "." + nameof(matrix.AssetPair));

            AssetPair = matrix.AssetPair;

            Cells = new MatrixCell[matrix.Value.GetLength(0), matrix.Value.GetLength(1)];

            for (var row = 0; row < matrix.Value.GetLength(0); row += 1)
            {
                var cellRow = new List<MatrixCell>();

                Exchanges.Add(matrix.Value[row, 0].ask.Source);
                Asks.Add(matrix.Value[row, 0].ask.BestAsk?.Price);
                // row starts
                for (var col = 0; col < matrix.Value.GetLength(1); col += 1)
                {
                    if (row == 0)
                        Bids.Add(matrix.Value[row, 0].bid.BestBid?.Price);

                    // Empty top left corner
                    if (row == col)
                    {
                        Cells[row, col] = null;
                        cellRow.Add(null);
                        continue;
                    }

                    var tuple = matrix.Value[row, col];

                    MatrixCell matrixCell;
                    if (tuple.ask.BestAsk == null || tuple.bid.BestBid == null)
                    {
                        matrixCell = new MatrixCell(null, null);
                        Cells[row, col] = matrixCell;
                        cellRow.Add(matrixCell);
                        continue;
                    }

                    var spread = (tuple.ask.BestAsk.Value.Price - tuple.bid.BestBid.Value.Price) / tuple.bid.BestBid.Value.Price * 100;
                    matrixCell = new MatrixCell(spread, null);
                    Cells[row, col] = matrixCell;
                    cellRow.Add(matrixCell);
                }
                // row ends
                AnotherCells.Add(cellRow);
            }

        }
    }
}
