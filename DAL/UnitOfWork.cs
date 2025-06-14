using Entities;

namespace DAL
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly AppDbContext _context;
        public IIoTDeviceRepository IoTDevicesRepo { get; }
        public IPumpSessionRepository PumpSessionsRepo { get; }

        public UnitOfWork(AppDbContext context, IIoTDeviceRepository iotDevices, IPumpSessionRepository pumpSessions)
        {
            _context = context;
            IoTDevicesRepo = iotDevices;
            PumpSessionsRepo = pumpSessions;
        }

        public async Task<bool> SaveChangesAsync() => await _context.SaveChangesAsync() > 0;

        public void Dispose() => _context.Dispose();
    }
}
