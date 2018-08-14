namespace Lykke.Service.ArbitrageDetector.Models
{
    public sealed class Exchange
    {
        public string Name { get; set; }

        public bool IsActual { get; set; }

        public ExchangeFees Fees { get; set; }

        public Exchange(string name, bool isActual, ExchangeFees fees)
        {
            Name = name;
            IsActual = isActual;
            Fees = fees;
        }

        public override string ToString()
        {
            return Name;
        }
    }
}
