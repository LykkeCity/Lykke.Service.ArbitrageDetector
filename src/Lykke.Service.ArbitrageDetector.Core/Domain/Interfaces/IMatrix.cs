using System;
using System.Collections.Generic;

namespace Lykke.Service.ArbitrageDetector.Core.Domain.Interfaces
{
    public interface IMatrix
    {
        string AssetPair { get; set; }

        IList<Exchange> Exchanges { get; set; }

        IList<decimal?> Asks { get; set; }

        IList<decimal?> Bids { get; set; }

        IList<IList<MatrixCell>> Cells { get; set; }

        DateTime DateTime { get; set; }
    }
}
