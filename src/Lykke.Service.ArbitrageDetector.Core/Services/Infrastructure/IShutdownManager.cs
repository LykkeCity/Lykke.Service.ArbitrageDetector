using System.Threading.Tasks;
using Common;

namespace Lykke.Service.ArbitrageDetector.Core.Services.Infrastructure
{
    public interface IShutdownManager
    {
        Task StopAsync();

        void Register(IStopable stopable);
    }
}
