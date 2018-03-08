using System;
using Lykke.Service.ArbitrageDetector.Client.Models;
using Xunit;

namespace Lykke.Service.ArbitrageDetector.Client.Tests
{
    /// <summary>
    /// Main fixture, initialize client only once for all tests with [Collection("Client collection")] attribute.
    /// </summary>
    public class ClientFixture : IDisposable
    {
        protected IArbitrageDetectorClient Client { get; private set; }

        public ClientFixture()
        {
            // Must be started
            var settings = new ArbitrageDetectorServiceClientSettings("http://localhost:5000");
            Client = new ArbitrageDetectorClient(settings);
        }

        public void Dispose()
        {
        }
    }

    [CollectionDefinition("Client collection")]
    public class DatabaseCollection : ICollectionFixture<ClientFixture>
    {
        // This class has no code, and is never created. Its purpose is simply
        // to be the place to apply [CollectionDefinition] and all the
        // ICollectionFixture<> interfaces.
    }
}
