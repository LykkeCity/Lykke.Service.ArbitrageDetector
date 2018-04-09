using Lykke.Service.ArbitrageDetector.Core.Domain;
using Xunit;

namespace Lykke.Service.ArbitrageDetector.Tests
{
    public class VolumePriceTests
    {
        [Fact]
        public void ReciprocalTest()
        {
            var volumePrice = new VolumePrice(8999.95m, 7);
            var reciprocal = volumePrice.Reciprocal();

            Assert.Equal(reciprocal.Price, 1 / 8999.95m);
            Assert.Equal(reciprocal.Volume, 8999.95m * 7);
        }   
    }       
}           
