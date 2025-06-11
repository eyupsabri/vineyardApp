using DAL;
using Entities;

namespace Business.Services
{
    public class IoTDevicesService : IIoTDevicesService
    {
        private IIoTDeviceRepository _repo;

        public IoTDevicesService(IIoTDeviceRepository repository)
        {
            _repo = repository;
        }

        public async Task<IoTDevice> GetIoTDeviceByDeviceId(Guid id)
        {
            return await _repo.GetIoTDeviceByDeviceId(id);
        }

        public async Task<bool> ChangeActualDeviceStatus(Guid deviceIdentifier, bool status)
        {
            var device = await _repo.GetIoTDeviceByDeviceId(deviceIdentifier);
            if (device == null)
            {
                return false;
            }
            device.Pump.ActualState = status;
            device.Pump.LastActualStateChange = DateTime.UtcNow;

            return await _repo.SaveChanges();
        }
    }
}
