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
        public decimal DepositFee { get; set; }

        /// <summary>
        /// Trading fee.
        /// </summary>
        public decimal TradingFee { get; set; }

        public ExchangeFees()
        {
        }

        public ExchangeFees(DomainExchangeFees domain)
        {
            ExchangeName = domain.ExchangeName;
            DepositFee = domain.DepositFee;
            TradingFee = domain.TradingFee;
        }

        public DomainExchangeFees ToDomain()
        {
            var result = new DomainExchangeFees
            {
                ExchangeName = ExchangeName,
                DepositFee = DepositFee,
                TradingFee = TradingFee
            };

            return result;
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return $"{ExchangeName} - deposit: {DepositFee}, trading: {TradingFee}";
        }
    }
}
