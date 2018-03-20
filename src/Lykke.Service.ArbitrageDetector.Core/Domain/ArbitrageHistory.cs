using System;

namespace Lykke.Service.ArbitrageDetector.Core.Domain
{
    /// <summary>
    /// Represents a history record of arbitrage situation.
    /// </summary>
    public sealed class ArbitrageHistory
    {
        /// <summary>
        /// Arbitrage.
        /// </summary>
        public Arbitrage Arbitrage { get; }

        /// <summary>
        /// Status - started or ended.
        /// </summary>
        public ArbitrageHistoryStatus Status { get; }

        /// <summary>
        /// Timestamp.
        /// </summary>
        public DateTime Timestamp { get; }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="arbitrage"></param>
        /// <param name="status"></param>
        public ArbitrageHistory(Arbitrage arbitrage, ArbitrageHistoryStatus status)
        {
            if (status == ArbitrageHistoryStatus.None)
                throw new ArgumentException(nameof(status));

            Arbitrage = arbitrage ?? throw new ArgumentNullException(nameof(arbitrage));
            Status = status;
            Timestamp = DateTime.UtcNow;
        }
    }
}
