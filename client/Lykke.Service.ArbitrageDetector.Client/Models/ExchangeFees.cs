namespace Lykke.Service.ArbitrageDetector.Client.Models
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
        public float DepositFee { get; set; }

        /// <summary>
        /// Taker fee.
        /// </summary>
        public float TakerFee { get; set; }

        /// <inheritdoc />
        public override string ToString()
        {
            return $"{ExchangeName} - deposit: {DepositFee}, taker: {TakerFee}";
        }
    }
}
