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

            for (var row = 0; row < matrix.Value.GetLength(0); row += 1)
            {
                var cellRow = new List<MatrixCell>();

                var exchangeName = matrix.Value[row, 0].ask.Source;
                var isActual = (DateTime.UtcNow - matrix.Value[row, 0].ask.Timestamp).TotalSeconds < 10;
                Exchanges.Add(new Exchange(exchangeName, isActual));
                Asks.Add(matrix.Value[row, 0].ask.BestAsk?.Price);
                // row starts
                for (var col = 0; col < matrix.Value.GetLength(1); col += 1)
                {
                    if (row == 0)
                        Bids.Add(matrix.Value[row, col].bid.BestBid?.Price);

                    // The same exchanges
                    if (row == col)
                    {
                        cellRow.Add(null);
                        continue;
                    }

                    var tuple = matrix.Value[row, col];

                    MatrixCell matrixCell;
                    if (tuple.ask.BestAsk == null || tuple.bid.BestBid == null)
                    {
                        matrixCell = new MatrixCell(null, null);
                        cellRow.Add(matrixCell);
                        continue;
                    }

                    var spread = (tuple.ask.BestAsk.Value.Price - tuple.bid.BestBid.Value.Price) / tuple.bid.BestBid.Value.Price * 100;
                    matrixCell = new MatrixCell(spread, null);
                    cellRow.Add(matrixCell);
                }
                // row ends
                Cells.Add(cellRow);
            }

        }
    }
}
