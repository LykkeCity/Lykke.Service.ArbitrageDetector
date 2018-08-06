namespace Lykke.Service.ArbitrageDetector.Core.Domain
{
    /// <summary>
    /// Represents fees of an exhange.
    /// </summary>
    public class ExchangeFees
    {
        /// <summary>
        /// Exchange name.
        /// </summary>
        public string ExchangeName { get; set; }

        /// <summary>
        /// Comulative fee for both deposit and withdrawal fees.
        /// </summary>
        public decimal DepositFee { get; set; }

        /// <summary>
        /// Trading fee.
        /// </summary>
        public decimal TradingFee { get; set; }

        /// <summary>
        /// Default fees - deposit = 0%, trading = 0%.
        /// </summary>
        public static ExchangeFees Default => new ExchangeFees();

        /// <inheritdoc />
        public override string ToString()
        {
            return $"{ExchangeName} - deposit: {DepositFee}, trading: {TradingFee}";
        }
    }
}
