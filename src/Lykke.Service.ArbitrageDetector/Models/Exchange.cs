using System;

namespace Lykke.Service.ArbitrageDetector.Models
{
    /// <summary>
    /// Represents an arbitrage matrix cell.
    /// </summary>
    public sealed class Exchange
    {
        public string Name { get; set; }

        public bool IsActual { get; set; }

        public Exchange(string name, bool isActual)
        {
            Name = name;
            IsActual = isActual;
        }
    }
}
