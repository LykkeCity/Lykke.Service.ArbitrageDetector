namespace Lykke.Service.ArbitrageDetector.AzureRepositories.Models
{
    public sealed class Exchange
    {
        public string Name { get; set; }

        public bool IsActual { get; set; }

        public Exchange(string name, bool isActual)
        {
            Name = name;
            IsActual = isActual;
        }

        public override string ToString()
        {
            return Name;
        }
    }
}
