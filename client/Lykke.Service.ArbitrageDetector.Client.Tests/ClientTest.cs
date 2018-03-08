﻿using System;
using System.Linq;
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
            Assert.NotEmpty(orderBooks);
            Assert.NotEmpty(orderBooks.First().Source);
            Assert.NotEmpty(orderBooks.First().AssetPair);
            Assert.NotEqual(default(DateTime), orderBooks.First().Timestamp);
            Assert.NotEmpty(orderBooks.First().Asks);
            Assert.NotEmpty(orderBooks.First().Bids);
            Assert.NotEqual(default(decimal), orderBooks.First().Asks.First().Price);
            Assert.NotEqual(default(decimal), orderBooks.First().Asks.First().Volume);
        }
    }
}
