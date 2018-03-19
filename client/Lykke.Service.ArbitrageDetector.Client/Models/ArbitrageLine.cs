namespace Lykke.Service.ArbitrageDetector.Client.Models
{
    public sealed class ArbitrageLine
    {
        public decimal Bid { get; set; }

        public decimal Ask { get; set; }

        public decimal Price => Ask > Bid ? Ask : Bid;  // Ask or Bid can be is equal to 0

        public decimal Volume { get; set; }

        public VolumePrice VolumePrice => new VolumePrice(Price, Volume);

        public CrossRate CrossRate { get; set; }    //  Ask or Bid cross rate
    }
}
