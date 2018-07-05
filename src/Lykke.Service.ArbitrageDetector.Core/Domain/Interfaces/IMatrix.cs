using System.Collections.Generic;

namespace Lykke.Service.ArbitrageDetector.Core.Domain.Interfaces
{
    public interface IMatrix
    {
        string AssetPair { get; set; }

        IList<IExchange> Exchanges { get; set; }

        IList<decimal?> Asks { get; set; }

        IList<decimal?> Bids { get; set; }

        IList<IList<IMatrixCell>> Cells { get; set; }
    }
}
