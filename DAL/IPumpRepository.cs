using Entities;

namespace DAL
{
    public interface IPumpRepository
    {
        public IQueryable<Pump> QueryWithDevice();

        public Task<List<Pump>> GetPumpsById(List<Guid> ids);
    }
}
