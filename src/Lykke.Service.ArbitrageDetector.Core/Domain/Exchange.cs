namespace Lykke.Service.ArbitrageDetector.Core.Domain
{
    /// <summary>
    /// Represents an exchange.
    /// </summary>
    public sealed class Exchange
    {
        /// <summary>
        /// Exchange name.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Is exchange actual.
        /// </summary>
        public bool IsActual { get; set; }

        /// <summary>
        /// Fees.
        /// </summary>
        public ExchangeFees Fees { get; set; }

        public Exchange(string name, bool isActual, ExchangeFees fees)
        {
            Name = name;
            IsActual = isActual;
            Fees = fees;
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return Name;
        }
    }
}
