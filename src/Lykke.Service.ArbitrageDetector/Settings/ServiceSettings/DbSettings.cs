using Lykke.SettingsReader.Attributes;

namespace Lykke.Service.ArbitrageDetector.Settings.ServiceSettings
{
    public class DbSettings
    {
        [AzureTableCheck]
        public string LogsConnString { get; set; }
    }
}
