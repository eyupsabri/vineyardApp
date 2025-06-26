using AutoMapper;
using Entities;
using Entities.DTOs;


namespace VineyardApp.AutoMapper
{
    public class AutoMapperProfile : Profile
    {
        public AutoMapperProfile()
        {
            CreateMap<IoTDevice, IoTPollingDTO>()
                .ForMember(dest => dest.DesiredState, opt => opt.MapFrom(src => src.Pump.DesiredState))
                .ForMember(dest => dest.IsManualOverride, opt => opt.MapFrom(src => src.Pump.IsManualOverride))
                .ForMember(dest => dest.LastDesiredChange, opt => opt.MapFrom(src => src.Pump.LastDesiredChange))
                .ForMember(dest => dest.DeviceIdentifier, opt => opt.MapFrom(src => src.DeviceIdentifier))
                .ForMember(dest => dest.ActualState, opt => opt.MapFrom(src => src.Pump.ActualState))
                .ForMember(dest => dest.LastActualChange, opt => opt.MapFrom(src => src.Pump.LastActualChange));

        }
    }
}
