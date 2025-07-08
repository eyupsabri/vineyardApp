using Entities;

namespace DAL
{
    public interface IPumpRepository
    {
        public IQueryable<Pump> QueryWithDevice();
    }
}
