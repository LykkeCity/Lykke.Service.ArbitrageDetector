using System.Collections.Generic;
using Lykke.SettingsReader.Attributes;

namespace Lykke.Service.ArbitrageDetector.Settings
{
    public class RabbitMqSettings
    {
        [AmqpCheck]
        public string ConnectionString { get; set; }

        public IEnumerable<string> Exchanges { get; set; }
    }
}
