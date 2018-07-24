using System;
using System.Collections.Generic;
using System.Linq;
using DomainMatrix = Lykke.Service.ArbitrageDetector.Core.Domain.Matrix;
using DomainExchange = Lykke.Service.ArbitrageDetector.Core.Domain.Exchange;
using DomainMatrixCell = Lykke.Service.ArbitrageDetector.Core.Domain.MatrixCell;

namespace Lykke.Service.ArbitrageDetector.AzureRepositories.Models
{
    public sealed class MatrixBlob
    {
        public string AssetPair { get; set; }

        public IList<Exchange> Exchanges { get; set; } = new List<Exchange>();

        public IList<decimal?> Asks { get; set; } = new List<decimal?>();

        public IList<decimal?> Bids { get; set; } = new List<decimal?>();

        public IList<IList<MatrixCell>> Cells { get; set; } = new List<IList<MatrixCell>>();

        public DateTime DateTime { get; set; }

        public MatrixBlob()
        {
        }

        public MatrixBlob(DomainMatrix matrix)
        {
            if (matrix == null)
                throw new ArgumentNullException(nameof(matrix));

            if (string.IsNullOrWhiteSpace(matrix.AssetPair))
                throw new ArgumentOutOfRangeException(nameof(matrix.AssetPair));

            AssetPair = matrix.AssetPair;
            Exchanges = matrix.Exchanges.Select(x => new Exchange(x.Name, x.IsActual)).ToList();
            Bids = matrix.Bids;
            Asks = matrix.Asks;
            foreach (var rows in matrix.Cells)
            {
                var row = new List<MatrixCell>();
                foreach (var cell in rows)
                    if (cell == null)
                        row.Add(null);
                    else
                        row.Add(new MatrixCell(cell.Spread, cell.Volume));

                Cells.Add(row);
            }

            DateTime = matrix.DateTime;
        }

        public DomainMatrix Matrix()
        {
            var result = new DomainMatrix(AssetPair);

            result.AssetPair = AssetPair;
            result.Exchanges = Exchanges.Select(x => new DomainExchange(x.Name, x.IsActual)).ToList();
            result.Bids = Bids;
            result.Asks = Asks;
            foreach (var rows in Cells)
            {
                var row = new List<DomainMatrixCell>();
                foreach (var cell in rows)
                    if (cell == null)
                        row.Add(null);
                    else
                        row.Add(new DomainMatrixCell(cell.Spread, cell.Volume));

                result.Cells.Add(row);
            }

            result.DateTime = DateTime;

            return result;
        }

        public override string ToString()
        {
            return $"{AssetPair} - {DateTime}";
        }
    }
}
