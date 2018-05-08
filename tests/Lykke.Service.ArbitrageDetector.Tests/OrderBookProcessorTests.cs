using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Common;
using Common.Log;
using Lykke.Service.ArbitrageDetector.Core;
using Lykke.Service.ArbitrageDetector.Core.Domain;
using Lykke.Service.ArbitrageDetector.Services;
using Xunit;

namespace Lykke.Service.ArbitrageDetector.Tests
{
    public class OrderBookProcessorTests
    {
        [Fact]
        public async Task InvalidOrderBook_WriteToLog_ThrottlingTest()
        {
            var orderBookProcessor = new OrderBookProcessor(new LogToConsole(), null);
            orderBookProcessor.WriteToLogDelayInMilliseconds = 20; // 5 * 60 * 1000;

            var zeroBidPrice = "{ \"source\":\"SomeExchange\", \"asset\":\"XXXYYY\", \"timestamp\":\"2018 - 04 - 25T21: 19:00\"," +
                            "\"asks\":[ { \"price\":1, \"volume\":3 } ], \"bids\":[ { \"price\":0, \"volume\":100 } ] }";
            var zeroAskPrice = "{ \"source\":\"SomeExchange\", \"asset\":\"XXXYYY\", \"timestamp\":\"2018 - 04 - 25T21: 19:00\"," +
                               "\"asks\":[ { \"price\":0, \"volume\":3 } ], \"bids\":[ { \"price\":0.6, \"volume\":100 } ] }";

            var zeroBidVolume = "{ \"source\":\"SomeExchange\", \"asset\":\"XXXYYY\", \"timestamp\":\"2018 - 04 - 25T21: 19:00\"," +
                               "\"asks\":[ { \"price\":1, \"volume\":3 } ], \"bids\":[ { \"price\":0.6, \"volume\":0 } ] }";
            var zeroAskVolume = "{ \"source\":\"SomeExchange\", \"asset\":\"XXXYYY\", \"timestamp\":\"2018 - 04 - 25T21: 19:00\"," +
                               "\"asks\":[ { \"price\":1, \"volume\":0 } ], \"bids\":[ { \"price\":0.6, \"volume\":100 } ] }";

            var negativeSpread = "{ \"source\":\"SomeExchange\", \"asset\":\"XXXYYY\", \"timestamp\":\"2018 - 04 - 25T21: 19:00\"," +
                               "\"asks\":[ { \"price\":1, \"volume\":3 } ], \"bids\":[ { \"price\":2, \"volume\":100 } ] }";

            for (var i = 0; i < 100; i++)
            {
                orderBookProcessor.Process(zeroBidPrice.ToUtf8Bytes());
                orderBookProcessor.Process(zeroAskPrice.ToUtf8Bytes());
                orderBookProcessor.Process(zeroBidVolume.ToUtf8Bytes());
                orderBookProcessor.Process(zeroAskVolume.ToUtf8Bytes());
                orderBookProcessor.Process(negativeSpread.ToUtf8Bytes());
            }

            var temp = 0;
        }
    }
}
