using Entities;
using Microsoft.EntityFrameworkCore.Storage;

namespace DAL
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly AppDbContext _context;
        public IIoTDeviceRepository IoTDevicesRepo { get; }
        public IPumpSessionRepository PumpSessionsRepo { get; }
        public IUserRepository UserRepo { get; }

        public UnitOfWork(AppDbContext context, IIoTDeviceRepository iotDevices, IPumpSessionRepository pumpSessions, IUserRepository userRepo)
        {
            _context = context;
            IoTDevicesRepo = iotDevices;
            PumpSessionsRepo = pumpSessions;
            UserRepo = userRepo;
        }

        public async Task<int> SaveChangesAsync()
        {
            try
            {
                return await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                // Optional: log the exception here
                // e.g., _logger.LogError(ex, "SaveChangesAsync failed.");

                return -1;
            }
        }

        public async Task<IDbContextTransaction> BeginTransactionAsync() => await _context.Database.BeginTransactionAsync();
        public void Dispose() => _context.Dispose();
    }
}
