namespace Lykke.Service.ArbitrageDetector.Client.Models
{
    /// <summary>
    /// Represents an arbitrage situation.
    /// </summary>
    public sealed class Matrix
    {
        /// <summary>
        /// Asset Pair
        /// </summary>
        public string AssetPair { get; set; }

        /// <summary>
        /// Matrix
        /// </summary>
        public string[,] Value { get; set; } = new string[0, 0];
    }
}
