using DomainExchangeFees = Lykke.Service.ArbitrageDetector.Core.Domain.ExchangeFees;

namespace Lykke.Service.ArbitrageDetector.Models
{
    /// <summary>
    /// Represents fees of an exhange.
    /// </summary>
    public class ExchangeFees
    {
        /// <summary>
        /// Exchange name.
        /// </summary>
        public string ExchangeName { get; set; }

        /// <summary>
        /// Comulative fee for both deposit and withdrawal fees.
        /// </summary>
        public float DepositFee { get; set; }

        /// <summary>
        /// Taker fee.
        /// </summary>
        public float TakerFee { get; set; }

        public ExchangeFees()
        {
        }

        public ExchangeFees(DomainExchangeFees domain)
        {
            ExchangeName = domain.ExchangeName;
            DepositFee = domain.DepositFee;
            TakerFee = domain.TakerFee;
        }

        public DomainExchangeFees ToDomain()
        {
            var result = new DomainExchangeFees
            {
                ExchangeName = ExchangeName,
                DepositFee = DepositFee,
                TakerFee = TakerFee
            };

            return result;
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return $"{ExchangeName} - deposit: {DepositFee}, taker: {TakerFee}";
        }
    }
}
