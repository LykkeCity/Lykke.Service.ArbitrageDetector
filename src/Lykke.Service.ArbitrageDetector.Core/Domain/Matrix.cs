using System;
using System.Collections.Generic;
using System.Linq;

namespace Lykke.Service.ArbitrageDetector.Core.Domain
{
    public sealed class Matrix
    {
        public string AssetPair { get; set; }

        public IList<Exchange> Exchanges { get; set; } = new List<Exchange>();

        public IList<decimal?> Asks { get; set; } = new List<decimal?>();

        public IList<decimal?> Bids { get; set; } = new List<decimal?>();

        public IList<IList<MatrixCell>> Cells { get; set; } = new List<IList<MatrixCell>>();

        public DateTime DateTime { get; set; }

        public Matrix(string assetPair)
        {
            AssetPair = string.IsNullOrEmpty(assetPair) ? throw new ArgumentNullException(nameof(assetPair)) : assetPair;
        }

        public decimal? GetLowestSpread(string exchangeName)
        {
            var lykkeExchange = Exchanges.SingleOrDefault(x => x.Name.Equals(exchangeName, StringComparison.OrdinalIgnoreCase));

            if (lykkeExchange == null) //|| !lykkeExchange.IsActual)
                return null;

            var lykkeIndex = Exchanges.IndexOf(lykkeExchange);
            var lykkeRow = Cells.ElementAt(lykkeIndex);
            var minSpreadInRow = lykkeRow.Where(x => x?.Spread != null).Min(x => x.Spread) ?? 0;
            var minSpreadInColumn = Cells.Select(x => x.ElementAt(lykkeIndex)).Where(x => x?.Spread != null).Min(x => x.Spread) ?? 0;
            var result = Math.Min(minSpreadInRow, minSpreadInColumn);

            return result;
        }

        public override string ToString()
        {
            return $"{AssetPair} - {DateTime}";
        }
    }
}
