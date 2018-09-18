namespace Lykke.Service.ArbitrageDetector.Client.Models
{
    /// <summary>
    /// Represents an arbitrage matrix cell.
    /// </summary>
    public sealed class MatrixCell
    {
        /// <summary>
        /// Spread.
        /// </summary>
        public decimal? Spread { get; set; }

        /// <summary>
        /// Volume.
        /// </summary>
        public decimal? Volume { get; set; }
    }
}
