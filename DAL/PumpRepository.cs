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
    }
}
