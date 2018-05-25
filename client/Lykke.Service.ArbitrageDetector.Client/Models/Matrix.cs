using System;
using System.Collections.Generic;

namespace Lykke.Service.ArbitrageDetector.Client.Models
{
    /// <summary>
    /// Represents an arbitrage matrix.
    /// </summary>
    public sealed class Matrix
    {
        /// <summary>
        /// Asset pair.
        /// </summary>
        public string AssetPair { get; set; }

        /// <summary>
        /// Exchanges.
        /// </summary>
        public IList<string> Exchanges { get; set; } = new List<string>();

        /// <summary>
        /// Asks.
        /// </summary>
        public IList<decimal?> Asks { get; set; } = new List<decimal?>();

        /// <summary>
        /// Bids.
        /// </summary>
        public IList<decimal?> Bids { get; set; } = new List<decimal?>();

        /// <summary>
        /// Cells.
        /// </summary>
        public MatrixCell[,] Cells { get; set; }

        /// <summary>
        /// Alternative cells.
        /// </summary>
        public IList<IList<MatrixCell>> AnotherCells { get; set; }
    }
}
