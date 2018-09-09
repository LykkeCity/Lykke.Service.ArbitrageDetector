namespace Lykke.Service.ArbitrageDetector.Core.Domain
{
    public sealed class Exchange
    {
        public string Name { get; }

        public bool IsActual { get; }

        public ExchangeFees Fees { get; }

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
