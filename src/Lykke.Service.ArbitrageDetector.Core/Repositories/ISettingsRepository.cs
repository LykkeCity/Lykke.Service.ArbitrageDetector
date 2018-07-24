using System.Threading.Tasks;
using Lykke.Service.ArbitrageDetector.Core.Domain;

namespace Lykke.Service.ArbitrageDetector.Core.Repositories
{
    public interface ISettingsRepository
    {
        Task<ISettings> GetAsync();

        Task InsertOrReplaceAsync(ISettings settings);

        Task<bool> DeleteAsync();
    }
}
