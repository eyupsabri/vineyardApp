using Entities;

namespace DAL
{
    public interface IIoTDeviceRepository
    {

        public Task<IoTDevice?> GetIoTDeviceByDeviceId(Guid id);

    }
}
