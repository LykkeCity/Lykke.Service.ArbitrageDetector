using System;

namespace Lykke.Service.ArbitrageDetector.Client.Models 
{
    /// <summary>
    /// Arbitrage Detector Client Settings.
    /// </summary>
    public class ArbitrageDetectorServiceClientSettings
    {
        /// <summary>
        /// Service url.
        /// </summary>
        public string ServiceUrl { get; set; }

        /// <inheritdoc />
        public ArbitrageDetectorServiceClientSettings()
        {
        }

        /// <inheritdoc />
        public ArbitrageDetectorServiceClientSettings(string serviceUrl)
        {
            ServiceUrl = string.IsNullOrWhiteSpace(serviceUrl) ? throw new ArgumentNullException(nameof(serviceUrl)) : serviceUrl;
        }
    }
}
