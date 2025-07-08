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

            CreateMap<Pump, PumpResponseDTO>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.ActualState, opt => opt.MapFrom(src => src.ActualState))
                .ForMember(dest => dest.IsManualOverride, opt => opt.MapFrom(src => src.IsManualOverride))
                .ForMember(dest => dest.LastHeartbeat, opt => opt.MapFrom(src => src.LastHeartbeat))
                .ForMember(dest => dest.NeedsAttention, opt => opt.MapFrom(src => src.NeedsAttention))
                .ForMember(dest => dest.DeviceIdentifier, opt => opt.MapFrom(src => src.IoTDevice.DeviceIdentifier))
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.IoTDevice.Name));
        }
    }
}
