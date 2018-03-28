using Lykke.Service.ArbitrageDetector.Core.Domain;

namespace Lykke.Service.ArbitrageDetector.Services.Model
{
    /// <summary>
    /// represents a record af arbitrage situation evvent.
    /// </summary>
    internal sealed class ArbitrageLine
    {
        /// <summary>
        /// BidPrice price.
        /// </summary>
        public decimal BidPrice { get; set; }

        /// <summary>
        /// AskPrice price.
        /// </summary>
        public decimal AskPrice { get; set; }

        /// <summary>
        /// AskPrice or BidPrice can be equal to 0.
        /// </summary>
        public decimal Price => AskPrice > BidPrice ? AskPrice : BidPrice;

        /// <summary>
        /// Volume of ask or bid.
        /// </summary>
        public decimal Volume { get; set; }

        /// <summary>
        /// Volume and price of ask or bid.
        /// </summary>
        public VolumePrice VolumePrice => new VolumePrice(Price, Volume);

        /// <summary>
        /// AskPrice or BidPrice cross rate.
        /// </summary>
        public CrossRate CrossRate { get; set; }
    }
}
