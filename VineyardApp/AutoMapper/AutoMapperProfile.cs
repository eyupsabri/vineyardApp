using AutoMapper;
using Entities;
using Entities.DTOs;


namespace VineyardApp.AutoMapper
{
    public class AutoMapperProfile : Profile
    {
        public AutoMapperProfile()
        {
            CreateMap<IoTDevice, IoTPollingDTO>();

        }
    }
}
