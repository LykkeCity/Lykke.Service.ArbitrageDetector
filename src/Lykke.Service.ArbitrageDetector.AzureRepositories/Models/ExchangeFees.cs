
namespace Lykke.Service.ArbitrageDetector.AzureRepositories.Models
{
    public class ExchangeFees
    {
        public string ExchangeName { get; set; }

        public decimal DepositFee { get; set; }

        public decimal TradingFee { get; set; }

        public override string ToString()
        {
            return $"{ExchangeName} - deposit: {DepositFee}, trading: {TradingFee}";
        }
    }
}
