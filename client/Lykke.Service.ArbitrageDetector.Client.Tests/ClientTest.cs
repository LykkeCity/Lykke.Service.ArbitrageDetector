using Xunit;

namespace Lykke.Service.ArbitrageDetector.Client.Tests
{
    [Collection("Client collection")]
    public class ClientTest : ClientFixture
    {
        [Fact]
        public async void GetOrderBooksTest()
        {
            var orderBooks = await Client.GetOrderBooksAsync();
            Assert.NotNull(orderBooks);
        }
    }
}
