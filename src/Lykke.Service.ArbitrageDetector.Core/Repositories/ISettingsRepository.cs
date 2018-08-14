using System.Threading.Tasks;
using Lykke.Service.ArbitrageDetector.Core.Domain;

namespace Lykke.Service.ArbitrageDetector.Core.Repositories
{
    public interface ISettingsRepository
    {
        Task<Settings> GetAsync();

        Task InsertOrReplaceAsync(Settings settings);

        Task<bool> DeleteAsync();
    }
}
