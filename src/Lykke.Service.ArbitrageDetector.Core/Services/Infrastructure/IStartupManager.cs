using System.Threading.Tasks;

namespace Lykke.Service.ArbitrageDetector.Core.Services.Infrastructure
{
    public interface IStartupManager
    {
        Task StartAsync();
    }
}
