using System;
using System.Globalization;
using DomainMatrix = Lykke.Service.ArbitrageDetector.Core.Domain.Matrix;

namespace Lykke.Service.ArbitrageDetector.Models
{
    /// <summary>
    /// Represents an arbitrage situation.
    /// </summary>
    public sealed class Matrix
    {
        public string AssetPair { get; set; }

        public string[,] Value { get; set; } = new string[0, 0];

        public Matrix(DomainMatrix matrix)
        {
            if (string.IsNullOrWhiteSpace(matrix.AssetPair))
                throw new ArgumentOutOfRangeException(nameof(matrix) + "." + nameof(matrix.AssetPair));

            AssetPair = matrix.AssetPair;

            Value = new string[matrix.Value.GetLength(0) + 2, matrix.Value.GetLength(1) + 2];

            // TODO: change to not negative indexes
            for (var row = -2; row < matrix.Value.GetLength(0); row += 1)
            {
                // row starts
                for(var col = -2; col < matrix.Value.GetLength(1); col += 1)
                {
                    // Empty top left corner
                    if (row == col && row < 0)
                    {
                        Value[row + 2, col + 2] = "";
                    }

                    // Exchanges names
                    else if (row == -2 && col >= 0)
                    {
                        // Bid
                        var exchangeName = matrix.Value[0, col].bid.Source.ToUpper();
                        Value[0 + 2, col + 2] = exchangeName;
                    }
                    else if (col == -2 && row >= 0)
                    {
                        // Ask
                        var exchangeName = matrix.Value[row, 0].ask.Source.ToUpper();
                        Value[row + 2, 0 + 2] = exchangeName;
                    }

                    // Bids and asks
                    else if (row == -1 && col >= 0)
                    {
                        var bestBid = matrix.Value[0, col].bid.BestBid?.Price.ToString(CultureInfo.InvariantCulture) ?? "no bids";
                        Value[0 + 2, col + 2] = bestBid;
                    }
                    else if (col == -1 && row >= 0)
                    {
                        var bestAsk = matrix.Value[row, 0].ask.BestAsk?.Price.ToString(CultureInfo.InvariantCulture) ?? "no asks";
                        Value[0 + 2, col + 2] = bestAsk;
                    }
                    //else if (col< 0 || row< 0)
                    //{
                    //    // Empty cell
                    //}
                    else
                    {
                        var tuple = matrix.Value[row, col];

                        if (col == row)
                        {
                            Value[row + 2, col + 2] = "-";
                        }
                        else if (tuple.ask.BestAsk == null || tuple.bid.BestBid == null)
                        {
                            Value[row + 2, col + 2] = "-";
                        }
                        else
                        {
                            var spread = (tuple.ask.BestAsk.Value.Price - tuple.bid.BestBid.Value.Price) / tuple.bid.BestBid.Value.Price * 100;
                            Value[row + 2, col + 2] = spread.ToString("0.00");
                        }
                    }
                }
                // row ends
            }

        }
    }
}
