using Entities;
using Entities.DTOs;

namespace Business.Services
{
    public interface IIoTDevicesService
    {
        public Task<IoTDevice> GetIoTDeviceByDeviceId(Guid id);
        public Task<int> UpdateDeviceStatus(UpdateStatusRequestDTO dto);
    }
}
