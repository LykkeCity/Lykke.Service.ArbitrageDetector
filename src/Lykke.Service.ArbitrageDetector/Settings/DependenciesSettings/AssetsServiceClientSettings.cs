using Lykke.SettingsReader.Attributes;

namespace Lykke.Service.ArbitrageDetector.Settings.DependenciesSettings
{
    public class AssetsServiceClientSettings
    {
        [HttpCheck("api/isalive")]
        public string ServiceUrl { get; set; }
    }
}
