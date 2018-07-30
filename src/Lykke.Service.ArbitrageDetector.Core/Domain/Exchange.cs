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

        public Exchange(string name, bool isActual)
        {
            Name = name;
            IsActual = isActual;
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return Name;
        }
    }
}
