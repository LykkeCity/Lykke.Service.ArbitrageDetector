using System;

namespace Lykke.Service.ArbitrageDetector.Client.Models
{
    public sealed class ArbitrageHistory
    {
        public Arbitrage Arbitrage { get; }

        public ArbitrageHistoryType Type { get; }

        public DateTime Timestamp { get; }

        public ArbitrageHistory(Arbitrage arbitrage, ArbitrageHistoryType type)
        {
            if (type == ArbitrageHistoryType.None)
                throw new ArgumentException(nameof(type));

            Arbitrage = arbitrage ?? throw new ArgumentNullException(nameof(arbitrage));
            Type = type;
            Timestamp = DateTime.UtcNow;
        }
    }
}
