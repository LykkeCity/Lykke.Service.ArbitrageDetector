namespace Lykke.Service.ArbitrageDetector.Core.Services
{
    public interface IOrderBookProcessor
    {
        void Process(byte[] data);
    }
}
