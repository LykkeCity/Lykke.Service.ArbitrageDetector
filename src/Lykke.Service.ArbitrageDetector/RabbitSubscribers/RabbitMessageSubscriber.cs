using System;
using System.Threading.Tasks;
using Autofac;
using Common;
using Common.Log;
using Lykke.RabbitMqBroker;
using Lykke.RabbitMqBroker.Subscriber;
using Lykke.Service.ArbitrageDetector.Core.Services;

namespace Lykke.Service.ArbitrageDetector.RabbitSubscribers
{
    public class RabbitMessageSubscriber : IStartable, IStopable, IMessageDeserializer<byte[]>
    {
        private readonly ILog _log;
        private readonly IOrderBookProcessor _orderBookProcessor;
        private readonly string _connectionString;
        private readonly string _exchangeName;
        private RabbitMqSubscriber<byte[]> _subscriber;

        public RabbitMessageSubscriber(
            ILog log,
            IOrderBookProcessor orderBookProcessor,
            IShutdownManager shutdownManager,
            string connectionString,
            string exchangeName)
        {
            _log = log;
            _orderBookProcessor = orderBookProcessor;
            _connectionString = connectionString;
            _exchangeName = exchangeName;
            shutdownManager.Register(this);
        }

        public void Start()
        {
            var settings = RabbitMqSubscriptionSettings
                .CreateForSubscriber(_connectionString, _exchangeName, "arbitragedetector")
                .MakeDurable();

            _subscriber = new RabbitMqSubscriber<byte[]>(settings,
                    new ResilientErrorHandlingStrategy(_log, settings,
                        retryTimeout: TimeSpan.FromSeconds(10),
                        next: new DeadQueueErrorHandlingStrategy(_log, settings)))
                .SetMessageDeserializer(this)
                .Subscribe(ProcessMessageAsync)
                .CreateDefaultBinding()
                .SetLogger(_log)
                .Start();
        }

        private async Task ProcessMessageAsync(byte[] item)
        {
            try
            {
                _orderBookProcessor.Process(item);
            }
            catch (Exception ex)
            {
                await _log.WriteErrorAsync(nameof(RabbitMessageSubscriber), nameof(ProcessMessageAsync), ex);
                throw;
            }
        }

        public void Dispose()
        {
            _subscriber?.Dispose();
        }

        public void Stop()
        {
            _subscriber?.Stop();
        }

        public byte[] Deserialize(byte[] data)
        {
            return data;
        }
    }
}
