namespace Lykke.Service.ArbitrageDetector.Client.Models
{
    /// <summary>
    /// Represents one of the main property of the arbitrage.
    /// </summary>
    public enum ArbitrageProperty
    {
        /// <summary>
        /// None.
        /// </summary>
        None = 0,

        /// <summary>
        /// PnL.
        /// </summary>
        PnL = 1,

        /// <summary>
        /// Volume.
        /// </summary>
        Volume = 2,

        /// <summary>
        /// Spread.
        /// </summary>
        Spread = 3
    }
}
