using System.Threading.Tasks;

namespace Lykke.Service.ArbitrageDetector.Core.Services
{
    public interface IStartupManager
    {
        Task StartAsync();
    }
}