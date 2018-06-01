using System.Threading.Tasks;
using Lykke.Service.ArbitrageDetector.Core;

namespace Lykke.Service.ArbitrageDetector.AzureRepositories
{
    public interface ISettingsRepository
    {
        Task<ISettings> GetAsync();

        Task InsertOrReplaceAsync(ISettings settings);

        Task<bool> DeleteAsync();
    }
}
