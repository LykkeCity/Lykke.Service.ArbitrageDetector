using System;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Lykke.Service.ArbitrageDetector.Client.Tests
{
    [Collection("Client collection")]
    public class ClientTest : ClientFixture
    {
        [Fact]
        public async Task GetOrderBooksTest()
        {
            var orderBooks = await Client.GetOrderBooksAsync();

            Assert.NotNull(orderBooks);
            Assert.NotEmpty(orderBooks);
            Assert.NotEmpty(orderBooks.First().Source);
            Assert.True(!orderBooks.First().AssetPair.IsEmpty());
            Assert.NotEqual(default(DateTime), orderBooks.First().Timestamp);
            Assert.NotEmpty(orderBooks.First().Asks);
            Assert.NotEmpty(orderBooks.First().Bids);
            Assert.NotEqual(default(decimal), orderBooks.First().Asks.First().Price);
            Assert.NotEqual(default(decimal), orderBooks.First().Asks.First().Volume);
        }
    }
}
