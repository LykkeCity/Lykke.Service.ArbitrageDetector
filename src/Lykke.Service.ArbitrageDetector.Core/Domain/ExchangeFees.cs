namespace Lykke.Service.ArbitrageDetector.Core.Domain
{
    public class ExchangeFees
    {
        public string ExchangeName { get; }

        public decimal DepositFee { get; }

        public decimal TradingFee { get; }

        public ExchangeFees(string exchangeName, decimal depositFee, decimal tradingFee)
        {
            ExchangeName = exchangeName;
            DepositFee = depositFee;
            TradingFee = tradingFee;
        }

        public override string ToString()
        {
            return $"{ExchangeName} - deposit: {DepositFee}, trading: {TradingFee}";
        }
    }
}
