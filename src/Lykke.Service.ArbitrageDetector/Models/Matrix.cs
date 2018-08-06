using System;
using System.Collections.Generic;
using System.Linq;
using DomainMatrix = Lykke.Service.ArbitrageDetector.Core.Domain.Matrix;

namespace Lykke.Service.ArbitrageDetector.Models
{
    public sealed class Matrix
    {
        public string AssetPair { get; set; }

        public IList<Exchange> Exchanges { get; set; } = new List<Exchange>();

        public IList<decimal?> Asks { get; set; } = new List<decimal?>();

        public IList<decimal?> Bids { get; set; } = new List<decimal?>();

        public IList<IList<MatrixCell>> Cells { get; set; } = new List<IList<MatrixCell>>();

        public DateTime DateTime { get; set; }

        public Matrix(DomainMatrix domain)
        {
            if (domain == null)
                throw new ArgumentNullException(nameof(domain));

            if (string.IsNullOrWhiteSpace(domain.AssetPair))
                throw new ArgumentOutOfRangeException(nameof(domain.AssetPair));

            AssetPair = domain.AssetPair;
            Exchanges = domain.Exchanges.Select(x => new Exchange(x.Name, x.IsActual, new ExchangeFees(x.Fees))).ToList();
            Bids = domain.Bids;
            Asks = domain.Asks;
            foreach (var rows in domain.Cells)
            {
                var row = new List<MatrixCell>();
                foreach (var cell in rows)
                    if (cell == null)
                        row.Add(null);
                    else
                        row.Add(new MatrixCell(cell.Spread, cell.Volume));

                Cells.Add(row);
            }

            DateTime = domain.DateTime;
        }
    }
}
