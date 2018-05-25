namespace Lykke.Service.ArbitrageDetector.Models
{
    /// <summary>
    /// Represents an arbitrage matrix cell.
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
