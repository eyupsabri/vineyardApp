using Entities;
using Entities.DTOs;

namespace Business.Services
{
    public interface IIoTDevicesService
    {
        public Task<IoTDevice> GetIoTDeviceByDeviceId(Guid id);
        public Task<bool> UpdateDeviceStatus(Pump pump, UpdateStatusRequestDTO dto);
    }
}
