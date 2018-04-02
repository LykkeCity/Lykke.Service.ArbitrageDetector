﻿using System;
using System.Collections.Generic;

namespace Lykke.Service.ArbitrageDetector.Models
{
    /// <summary>
    /// Represents settings of Arbitrage Detector Service.
    /// </summary>
    public class Settings
    {
        /// <summary>
        /// Expiration time in milliseconds for order books and cross rates.
        /// </summary>
        public int ExpirationTimeInSeconds { get; set; }

        /// <summary>
        /// Wanted base assets.
        /// </summary>
        public IEnumerable<string> BaseAssets { get; set; }

        /// <summary>
        /// Intermediate assets.
        /// </summary>
        public IEnumerable<string> IntermediateAssets { get; set; }

        /// <summary>
        /// Quote asset for wanted assets.
        /// </summary>
        public string QuoteAsset { get; set; }

        /// <summary>
        /// Minimum spread.
        /// </summary>
        public int MinSpread { get; set; }

        /// <summary>
        /// Constructor.
        /// </summary>
        public Settings()
        {
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="expirationTimeInSeconds"></param>
        /// <param name="baseAssets"></param>
        /// <param name="quoteAsset"></param>
        /// <param name="intermediateAssets"></param>
        /// <param name="minSpread"></param>
        public Settings(int expirationTimeInSeconds, IEnumerable<string> baseAssets, IEnumerable<string> intermediateAssets, string quoteAsset, int minSpread)
        {
            ExpirationTimeInSeconds = expirationTimeInSeconds;
            BaseAssets = baseAssets ?? new List<string>();
            IntermediateAssets = intermediateAssets ?? new List<string>();
            QuoteAsset = quoteAsset;
            MinSpread = minSpread;
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="settings">Domain model</param>
        public Settings(Core.Settings settings)
            : this(settings.ExpirationTimeInSeconds, settings.BaseAssets, settings.IntermediateAssets, settings.QuoteAsset, settings.MinSpread)
        {
        }

        /// <summary>
        /// Converts object to domain model.
        /// </summary>
        /// <returns>Domain model</returns>
        public Core.Settings ToModel()
        {
            var domain = new Core.Settings(ExpirationTimeInSeconds, BaseAssets ?? new List<string>(), IntermediateAssets ?? new List<string>(), QuoteAsset ?? string.Empty, MinSpread);

            return domain;
        }
    }
}
