namespace Lykke.Service.ArbitrageDetector.Core.Domain
{
    /// <summary>
    /// Status of arbitrage situation record.
    /// </summary>
    public enum ArbitrageHistoryStatus
    {
        /// <summary>
        /// None.
        /// </summary>
        None = 0,

        /// <summary>
        /// Started.
        /// </summary>
        Started = 1,

        /// <summary>
        /// Ended.
        /// </summary>
        Ended = 2
    }
}
