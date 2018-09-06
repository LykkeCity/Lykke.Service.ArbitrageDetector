using AutoMapper;
using Lykke.Service.ArbitrageDetector.Client.Models;

namespace Lykke.Service.ArbitrageDetector
{
    public class AutoMapperProfile : Profile
    {
        public AutoMapperProfile()
        {
            CreateMap<Core.Domain.Arbitrage, Arbitrage>();
            CreateMap<Core.Domain.Arbitrage, ArbitrageRow>().ConvertUsing<ArbitrageRowConverter>();
            CreateMap<Core.Domain.AssetPair, AssetPair>();
            CreateMap<Core.Domain.Exchange, Exchange>();
            CreateMap<Core.Domain.ExchangeFees, ExchangeFees>();
            CreateMap<Core.Domain.LykkeArbitrageRow, LykkeArbitrageRow>();
            CreateMap<Core.Domain.Matrix, Matrix>();
            CreateMap<Core.Domain.MatrixCell, MatrixCell>();
            CreateMap<Core.Domain.OrderBook, OrderBook>();
            CreateMap<Core.Domain.OrderBook, OrderBookRow>();
            CreateMap<Core.Domain.Settings, Client.Models.Settings>();
            CreateMap<Client.Models.Settings, Core.Domain.Settings>();
            CreateMap<Core.Domain.SynthOrderBook, SynthOrderBook>();
            CreateMap<Core.Domain.VolumePrice, VolumePrice>();
        }
    }

    public class ArbitrageRowConverter : ITypeConverter<Core.Domain.Arbitrage, ArbitrageRow>
    {
        public ArbitrageRow Convert(Core.Domain.Arbitrage source, ArbitrageRow destination, ResolutionContext context)
        {
            return new ArbitrageRow
            {
                AssetPair = new AssetPair { Base = source.AssetPair.Base, Quote = source.AssetPair.Quote },
                BidSource = source.BidSynth.Source,
                AskSource = source.AskSynth.Source,
                BidConversionPath = source.BidSynth.ConversionPath,
                AskConversionPath = source.AskSynth.ConversionPath,
                Bid = new VolumePrice { Price = source.Bid.Price, Volume = source.Bid.Volume },
                Ask = new VolumePrice { Price = source.Ask.Price, Volume = source.Ask.Volume },
                Spread = source.Spread,
                Volume = source.Volume,
                PnL = source.PnL,
                StartedAt = source.StartedAt,
                EndedAt = source.EndedAt
            };
        }
    }
}
