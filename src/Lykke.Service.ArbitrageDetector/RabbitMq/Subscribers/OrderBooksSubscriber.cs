using System;
using System.Linq;
using System.Threading.Tasks;
using Autofac;
using Common;
using Common.Log;
using Lykke.Common.Log;
using Lykke.RabbitMqBroker;
using Lykke.RabbitMqBroker.Subscriber;
using Lykke.Service.ArbitrageDetector.Core.Handlers;
using Lykke.Service.ArbitrageDetector.Core.Services;

namespace Lykke.Service.ArbitrageDetector.RabbitMq.Subscribers
{
    internal sealed class OrderBooksSubscriber : IStartable, IStopable
    {
        private const string QueuePostfix = ".ArbitrageDetector";
        private readonly string _connectionString;
        private readonly string _exchangeName;
        private RabbitMqSubscriber<Models.OrderBook> _subscriber;

        private readonly IOrderBooksService _orderBooksService;
        private readonly IOrderBookHandler[] _orderBookHandlers;
        private readonly ILogFactory _logFactory;
        private readonly ILog _log;

        public OrderBooksSubscriber(
            string connectionString,
            string exchangeName,
            IOrderBooksService orderBooksService,
            IOrderBookHandler[] orderBookHandlers,
            ILogFactory logFactory)
        {
            _connectionString = connectionString;
            _exchangeName = exchangeName;
            
            _orderBooksService = orderBooksService;
            _orderBookHandlers = orderBookHandlers;
            _logFactory = logFactory;
            _log = logFactory.CreateLog(this);
        }

        public void Start()
        {
            var settings = new RabbitMqSubscriptionSettings
            {
                ConnectionString = _connectionString,
                ExchangeName = _exchangeName,
                QueueName = _exchangeName + QueuePostfix,
                IsDurable = false
            };

            _subscriber = new RabbitMqSubscriber<Models.OrderBook>(_logFactory, settings,
                    new ResilientErrorHandlingStrategy(_logFactory, settings, TimeSpan.FromSeconds(10)))
                .SetMessageDeserializer(new JsonMessageDeserializer<Models.OrderBook>())
                .SetMessageReadStrategy(new MessageReadQueueStrategy())
                .Subscribe(ProcessMessageAsync)
                .CreateDefaultBinding()
                .Start();
        }

        public void Stop()
        {
            _subscriber?.Stop();
        }

        public void Dispose()
        {
            _subscriber?.Dispose();
        }

        private async Task ProcessMessageAsync(Models.OrderBook orderBook)
        {
            try
            {
                var assetPair = _orderBooksService.InferBaseAndQuoteAssets(orderBook.AssetPairStr);

                if (assetPair == null)
                    return;

                var domain = new Core.Domain.OrderBook(orderBook.Source, assetPair,
                    orderBook.Bids.Select(x => new Core.Domain.VolumePrice(x.Price, x.Volume)).ToList(),
                    orderBook.Asks.Select(x => new Core.Domain.VolumePrice(x.Price, x.Volume)).ToList(),
                    orderBook.Timestamp);

                await Task.WhenAll(_orderBookHandlers.Select(o => o.HandleAsync(domain)));
            }
            catch (Exception ex)
            {
                _log.Error(ex);
                throw;
            }
        }
    }
}
