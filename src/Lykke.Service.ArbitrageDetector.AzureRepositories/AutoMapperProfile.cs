using AutoMapper;

namespace Lykke.Service.ArbitrageDetector.AzureRepositories
{
    public class AutoMapperProfile : Profile
    {
        public AutoMapperProfile()
        {
            CreateMap<Core.Domain.Exchange, Models.Exchange>();
            CreateMap<Models.Exchange, Core.Domain.Exchange>();

            CreateMap<Core.Domain.ExchangeFees, Models.ExchangeFees>();
            CreateMap<Models.ExchangeFees, Core.Domain.ExchangeFees>();

            CreateMap<Core.Domain.Matrix, Models.MatrixBlob>();
            CreateMap<Models.MatrixBlob, Core.Domain.Matrix>();

            CreateMap<Core.Domain.MatrixCell, Models.MatrixCell>();
            CreateMap<Models.MatrixCell, Core.Domain.MatrixCell>();

            CreateMap<Core.Domain.Settings, Models.Settings>(MemberList.Source);
            CreateMap<Models.Settings, Core.Domain.Settings>();
        }
    }

}
