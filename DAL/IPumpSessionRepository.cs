using Entities;

namespace DAL
{
    public interface IPumpSessionRepository
    {
        public Task<PumpSession> GetPumpSessionById(int id);
        public Task<int?> GetLatestSessionIdByDeviceIdentifier(Guid id);
    }
}
