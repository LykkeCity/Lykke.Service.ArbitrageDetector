using System;
using System.Linq;
using System.Threading.Tasks;
using Autofac;
using Common;
using Common.Log;
using Lykke.Common.Log;
using Lykke.RabbitMqBroker;
using Lykke.RabbitMqBroker.Subscriber;
using Lykke.Service.ArbitrageDetector.Core.Services;

namespace Lykke.Service.ArbitrageDetector.RabbitMq.Subscribers
{
    internal sealed class OrderBooksSubscriber : IStartable, IStopable, IMessageDeserializer<byte[]>
    {
        private readonly string _connectionString;
        private readonly string _exchangeName;
        private RabbitMqSubscriber<RabbitMq.Models.OrderBook> _subscriber;

        private readonly IArbitrageDetectorService _arbitrageDetectorService;
        private readonly ILykkeArbitrageDetectorService _lykkeArbitrageDetectorService;
        private readonly ILykkeExchangeService _lykkeExchangeService;
        private readonly ILogFactory _logFactory;
        private readonly ILog _log;

        public OrderBooksSubscriber(
            string connectionString,
            string exchangeName,
            ILykkeExchangeService lykkeExchangeService,
            IArbitrageDetectorService arbitrageDetectorService,
            ILykkeArbitrageDetectorService lykkeArbitrageDetectorService,
            ILogFactory logFactory)
        {
            _connectionString = connectionString;
            _exchangeName = exchangeName;
            
            _arbitrageDetectorService = arbitrageDetectorService;
            _lykkeArbitrageDetectorService = lykkeArbitrageDetectorService;
            _lykkeExchangeService = lykkeExchangeService;
            _logFactory = logFactory;
            _log = logFactory.CreateLog(this);
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

            _subscriber = new RabbitMqSubscriber<RabbitMq.Models.OrderBook>(_logFactory, settings,
                    new ResilientErrorHandlingStrategy(_logFactory, settings, TimeSpan.FromSeconds(10)))
                .SetMessageDeserializer(new JsonMessageDeserializer<RabbitMq.Models.OrderBook>())
                .SetMessageReadStrategy(new MessageReadQueueStrategy())
                .Subscribe(ProcessMessageAsync)
                .CreateDefaultBinding()
                .Start();
        }

        private Task ProcessMessageAsync(RabbitMq.Models.OrderBook orderBook)
        {
            try
            {
                var assetPair = _lykkeExchangeService.InferBaseAndQuoteAssets(orderBook.AssetPairStr);

                if (assetPair == null)
                    return Task.CompletedTask;

                var domain = new Core.Domain.OrderBook(orderBook.Source, assetPair,
                    orderBook.Bids.Select(x => new Core.Domain.VolumePrice(x.Price, x.Volume)).ToList(),
                    orderBook.Asks.Select(x => new Core.Domain.VolumePrice(x.Price, x.Volume)).ToList(),
                    orderBook.Timestamp);

                _arbitrageDetectorService.Process(domain);
                _lykkeArbitrageDetectorService.Process(domain);
            }
            catch (Exception ex)
            {
                _log.Error(ex);
                throw;
            }

            return Task.CompletedTask;
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
