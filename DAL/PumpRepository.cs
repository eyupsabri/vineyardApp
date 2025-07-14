using Entities;
using Microsoft.EntityFrameworkCore;

namespace DAL
{
    public class PumpRepository : IPumpRepository
    {
        private readonly AppDbContext _context;
        public PumpRepository(AppDbContext context)
        {
            _context = context;
        }
        public IQueryable<Pump> QueryWithDevice()
        {
            return _context.Pumps
                .Include(p => p.IoTDevice);

        }

        public async Task<List<Pump>> GetPumpsById(List<Guid> ids)
        {
            return await _context.Pumps
                .Include(p => p.IoTDevice)
                .Where(p => ids.Contains(p.IoTDevice.DeviceIdentifier))
                .ToListAsync();
        }

    }
}
