using Entities;

namespace Business.Services
{
    public interface IIoTDevicesService
    {
        public Task<IoTDevice> GetIoTDeviceByDeviceId(Guid id);
        public Task<bool> ChangeActualDeviceStatus(Guid deviceIdentifier, bool status);
    }
}
