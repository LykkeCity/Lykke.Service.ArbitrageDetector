using Lykke.Sdk.Settings;
using Lykke.Service.ArbitrageDetector.Settings.InProcServices;
using Lykke.Service.ArbitrageDetector.Settings.OutProcServices;

namespace Lykke.Service.ArbitrageDetector.Settings
{
    public class AppSettings : BaseAppSettings
    {
        public ArbitrageDetectorSettings ArbitrageDetector { get; set; }
        public AssetsServiceClientSettings AssetsServiceClient { get; set; }
        public OrderBooksCacheProviderClientSettings OrderBooksCacheProviderClient { get; set; }
    }
}
