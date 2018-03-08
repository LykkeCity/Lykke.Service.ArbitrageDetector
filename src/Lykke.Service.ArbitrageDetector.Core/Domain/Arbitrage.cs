using System;

namespace Lykke.Service.ArbitrageDetector.Core.Domain
{
    public struct Arbitrage
    {
        public CrossRate LowAsk{ get; }

        public CrossRate HighBid { get; }

        public Arbitrage(CrossRate lowAsk, CrossRate highBid)
        {
            LowAsk = lowAsk ?? throw new ArgumentNullException(nameof(lowAsk));
            HighBid = highBid ?? throw new ArgumentNullException(nameof(highBid));
        }
    }
}
