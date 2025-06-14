using Entities;
using Microsoft.EntityFrameworkCore;

namespace DAL
{
    public class PumpSessionRepository : IPumpSessionRepository
    {
        private readonly AppDbContext _db;

        public PumpSessionRepository(AppDbContext db)
        {
            _db = db;
        }

        public async Task<PumpSession> GetPumpSessionById(int id)
        {
            return await _db.PumpSessions.FirstOrDefaultAsync(s => s.Id == id);
        }
        public async Task<int?> GetLatestSessionIdByDeviceIdentifier(Guid id)
        {
            return await _db.PumpSessions
                .Where(s => s.Pump.IoTDevice.DeviceIdentifier == id && s.EndTime == null)
                .OrderByDescending(s => s.StartTime)
                .Select(s => (int?)s.Id)
                .FirstOrDefaultAsync();
        }
    }
}
