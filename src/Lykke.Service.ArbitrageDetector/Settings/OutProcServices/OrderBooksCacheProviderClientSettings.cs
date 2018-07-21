using Lykke.SettingsReader.Attributes;

namespace Lykke.Service.ArbitrageDetector.Settings.OutProcServices
{
    public class OrderBooksCacheProviderClientSettings
    {
        [HttpCheck("api/isalive")]
        public string ServiceUrl { get; set; }
    }
}
