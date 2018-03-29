using Lykke.Service.ArbitrageDetector.Core;

namespace Lykke.Service.ArbitrageDetector.Settings.ServiceSettings
{
    public class ArbitrageDetectorSettings
    {
        public StartupSettings Main { get; set; }

        public DbSettings Db { get; set; }

        public RabbitMqSettings RabbitMq { get; set; }
    }
}
