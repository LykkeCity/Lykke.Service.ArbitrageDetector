using System;

namespace Lykke.Service.ArbitrageDetector.Client.Models 
{
    public class ArbitrageDetectorServiceClientSettings
    {
        public string ServiceUrl { get; set; }

        public ArbitrageDetectorServiceClientSettings()
        {
        }

        public ArbitrageDetectorServiceClientSettings(string serviceUrl)
        {
            ServiceUrl = string.IsNullOrWhiteSpace(serviceUrl) ? throw new ArgumentNullException(nameof(serviceUrl)) : serviceUrl;
        }
    }
}
