namespace Lykke.Service.ArbitrageDetector.Core.Domain.Interfaces
{
    public interface IMatrixCell
    {
        decimal? Spread { get; set; }

        decimal? Volume { get; set; }
    }
}
