using System;
using System.Threading.Tasks;
using Autofac;
using Common;
using Common.Log;
using Lykke.RabbitMqBroker;
using Lykke.RabbitMqBroker.Subscriber;
using Lykke.Service.ArbitrageDetector.Core.Services;
using Lykke.Service.ArbitrageDetector.Core.Services.Infrastructure;
using Lykke.Service.ArbitrageDetector.OrderBookHandlers;

namespace Lykke.Service.ArbitrageDetector.RabbitSubscribers
{
    internal sealed class RabbitMessageSubscriber : IStartable, IStopable, IMessageDeserializer<byte[]>
    {
        private readonly string _connectionString;
        private readonly string _exchangeName;
        private RabbitMqSubscriber<byte[]> _subscriber;

        private readonly OrderBookParser _orderBookParser;
        
        private readonly IArbitrageDetectorService _arbitrageDetectorService;
        private readonly ILykkeArbitrageDetectorService _lykkeArbitrageDetectorService;
        private readonly ILog _log;

        public RabbitMessageSubscriber(
            string connectionString,
            string exchangeName,
            IShutdownManager shutdownManager,
            OrderBookParser orderBookParser,
            IArbitrageDetectorService arbitrageDetectorService,
            ILykkeArbitrageDetectorService lykkeArbitrageDetectorService,
            ILog log)
        {
            _connectionString = !string.IsNullOrWhiteSpace(connectionString) ? connectionString : throw new ArgumentNullException(nameof(connectionString));
            _exchangeName = !string.IsNullOrWhiteSpace(exchangeName) ? exchangeName : throw new ArgumentNullException(nameof(exchangeName));

            shutdownManager.Register(this);
            _orderBookParser = orderBookParser ?? throw new ArgumentNullException(nameof(orderBookParser));
            _arbitrageDetectorService = arbitrageDetectorService ?? throw new ArgumentNullException(nameof(arbitrageDetectorService));
            _lykkeArbitrageDetectorService = lykkeArbitrageDetectorService ?? throw new ArgumentNullException(nameof(lykkeArbitrageDetectorService));
            _log = log ?? throw new ArgumentNullException(nameof(log));
        }

        public void Start()
        {
            var settings = new RabbitMqSubscriptionSettings
            {
                ConnectionString = _connectionString,
                ExchangeName = _exchangeName,
                QueueName = _exchangeName + ".ArbitrageDetector",
                IsDurable = false
            };

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
                var orderBook = _orderBookParser.Parse(item);

                _arbitrageDetectorService.Process(orderBook);
                _lykkeArbitrageDetectorService.Process(orderBook);                        
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
