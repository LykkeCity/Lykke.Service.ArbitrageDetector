using System.Threading.Tasks;

namespace Lykke.Service.ArbitrageDetector.Core.Repositories
{
    public interface ISettingsRepository
    {
        Task<ISettings> GetAsync();

        Task InsertOrReplaceAsync(ISettings settings);

        Task<bool> DeleteAsync();
    }
}
