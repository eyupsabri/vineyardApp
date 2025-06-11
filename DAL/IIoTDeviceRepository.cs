using Entities;

namespace DAL
{
    public interface IIoTDeviceRepository
    {

        public Task<IoTDevice?> GetIoTDeviceByDeviceId(Guid id);
        public Task<IoTDevice?> GetDetailedIoTDeviceByDeviceId(Guid id);
        public Task<bool> SaveChanges();
    }
}
