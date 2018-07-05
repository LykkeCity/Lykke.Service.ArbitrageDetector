namespace Lykke.Service.ArbitrageDetector.Core.Domain.Interfaces
{
    public interface IExchange
    {
        string Name { get; set; }

        bool IsActual { get; set; }
    }
}
