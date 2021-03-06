﻿using System.Collections.Generic;
using Lykke.Service.ArbitrageDetector.Core.Domain;

namespace Lykke.Service.ArbitrageDetector.Core.Services
{
    public interface ILykkeArbitrageDetectorService
    {
        IEnumerable<LykkeArbitrageRow> GetArbitrages(string target, string source, ArbitrageProperty property = default, decimal minValue = 0);
    }
}
