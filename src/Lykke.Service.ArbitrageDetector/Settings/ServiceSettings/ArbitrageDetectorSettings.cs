using System.Collections.Generic;
using Lykke.SettingsReader.Attributes;

namespace Lykke.Service.ArbitrageDetector.Settings.ServiceSettings
{
    public class ArbitrageDetectorSettings
    {
        public DbSettings Db { get; set; }

        public int ArbitrageDetectorExecutionDelayInSeconds { get; set; }

        public int ExpirationTimeInSeconds { get; set; }

        public int HistoryMaxSize { get; set; }

        public string BaseCurrency { get; set; }

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
