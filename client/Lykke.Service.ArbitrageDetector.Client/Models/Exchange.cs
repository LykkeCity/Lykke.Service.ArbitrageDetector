using System;

namespace Lykke.Service.ArbitrageDetector.Client.Models
{
    /// <summary>
    /// Represents an arbitrage matrix cell.
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
        /// Constructor.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="isActual"></param>
        public Exchange(string name, bool isActual)
        {
            Name = name;
            IsActual = isActual;
        }
    }
}
