namespace Lykke.Service.ArbitrageDetector.Core.Domain
{
    /// <summary>
    /// Represents a matrix cell.
    /// </summary>
    public sealed class MatrixCell
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
