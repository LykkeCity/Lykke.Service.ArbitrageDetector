using System.Collections.Generic;
using Lykke.Service.ArbitrageDetector.Core.Domain;

namespace Lykke.Service.ArbitrageDetector.Core.Services
{
    public interface IOrderBooksService
    {
        IReadOnlyList<OrderBook> GetAll();

        IReadOnlyList<OrderBook> GetOrderBooks(string exchange, string assetPair);

        OrderBook GetOrderBook(string exchange, string assetPair);

        AssetPair InferBaseAndQuoteAssets(string assetPairStr);
    }
}
