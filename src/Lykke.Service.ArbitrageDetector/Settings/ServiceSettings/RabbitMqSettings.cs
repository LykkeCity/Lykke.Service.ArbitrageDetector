using Lykke.SettingsReader.Attributes;

namespace Lykke.Service.ArbitrageDetector.Settings.ServiceSettings
{
    public class RabbitMqSettings
    {
        [AmqpCheck]
        public string ConnectionString { get; set; }

        public string Exchange { get; set; }
    }
}
