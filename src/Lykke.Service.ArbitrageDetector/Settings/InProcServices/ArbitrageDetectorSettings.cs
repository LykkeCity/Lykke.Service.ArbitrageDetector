namespace Lykke.Service.ArbitrageDetector.Settings.InProcServices
{
    public class ArbitrageDetectorSettings
    {
        public DbSettings Db { get; set; }

        public RabbitMqSettings RabbitMq { get; set; }
    }
}
