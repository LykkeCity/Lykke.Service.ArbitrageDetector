using Lykke.Service.ArbitrageDetector.Settings.InProcServices;
using Lykke.Service.ArbitrageDetector.Settings.OutProcServices;
using Lykke.Service.ArbitrageDetector.Settings.SlackNotifications;

namespace Lykke.Service.ArbitrageDetector.Settings
{
    public class AppSettings
    {
        public ArbitrageDetectorSettings ArbitrageDetector { get; set; }
        public SlackNotificationsSettings SlackNotifications { get; set; }
        public AssetsServiceClientSettings AssetsServiceClient { get; set; }
        public OrderBooksCacheProviderClientSettings OrderBooksCacheProviderClient { get; set; }
    }
}
