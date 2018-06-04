namespace Lykke.Service.ArbitrageDetector.Settings.ServiceSettings
{
    public class ArbitrageDetectorSettings
    {
        public DbSettings Db { get; set; }

        public RabbitMqSettings RabbitMq { get; set; }
    }
}
