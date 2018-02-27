using System.Collections.Generic;
using Lykke.SettingsReader.Attributes;

namespace Lykke.Service.ArbitrageDetector.Settings.ServiceSettings
{
    public class ArbitrageDetectorSettings
    {
        [AzureTableCheck]
        public string LogsConnString { get; set; }

        public int ArbitrageDetectorExecutionDelayInSeconds { get; set; }

        public IReadOnlyCollection<string> WantedCurrencies { get; set; }

        public RabbitMqSettings RabbitMq { get; set; }
    }

    public class RabbitMqSettings
    {
        [AmqpCheck]
        public string ConnectionString { get; set; }

        public string Exchange { get; set; }
    }
}
