namespace Lykke.Service.ArbitrageDetector.Client.Models
{
    /// <summary>
    /// Represents an exchange.
    /// </summary>
    public sealed class Exchange
    {
        /// <summary>
        /// Name.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Is data from exchange actual.
        /// </summary>
        public bool IsActual { get; set; }

        /// <summary>
        /// Fees.
        /// </summary>
        public ExchangeFees Fees { get; set; }

        /// <inheritdoc />
        public override string ToString()
        {
            return Name;
        }
    }
}
