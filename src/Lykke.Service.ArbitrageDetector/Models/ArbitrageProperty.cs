namespace Lykke.Service.ArbitrageDetector.Models
{
    /// <summary>
    /// Represents one of the main property of the arbitrage.
    /// </summary>
    public enum ArbitrageProperty
    {
        None = 0,

        PnL = 1,

        Volume = 2,

        Spread = 3
    }
}
