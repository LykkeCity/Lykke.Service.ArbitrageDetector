namespace Lykke.Service.ArbitrageDetector.Core.Domain
{
    public sealed class MatrixCell
    {
        public decimal? Spread { get; }

        public decimal? Volume { get; }

        public MatrixCell(decimal? spread, decimal? volume)
        {
            Spread = spread;
            Volume = volume;
        }
    }
}
