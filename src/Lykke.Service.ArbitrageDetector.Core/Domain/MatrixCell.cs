using Lykke.Service.ArbitrageDetector.Core.Domain.Interfaces;

namespace Lykke.Service.ArbitrageDetector.Core.Domain
{
    /// <summary>
    /// Represents an arbitrage matrix cell.
    /// </summary>
    public sealed class MatrixCell : IMatrixCell
    {
        public decimal? Spread { get; set; }

        public decimal? Volume { get; set; }

        public MatrixCell(decimal? spread, decimal? volume)
        {
            Spread = spread;
            Volume = volume;
        }
    }
}
