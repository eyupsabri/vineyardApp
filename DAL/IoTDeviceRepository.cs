using Entities;
using Microsoft.EntityFrameworkCore;

namespace DAL
{
    public class IoTDeviceRepository : IIoTDeviceRepository
    {
        private readonly AppDbContext _db;

        public IoTDeviceRepository(AppDbContext db)
        {
            _db = db;
        }
        //IoT Device function only 
        public async Task<IoTDevice?> GetIoTDeviceByDeviceId(Guid id)
        {
            return await _db.IoTDevices
                .Include(d => d.Pump)
                .FirstOrDefaultAsync(d => d.DeviceIdentifier == id);
        }



    }
}
