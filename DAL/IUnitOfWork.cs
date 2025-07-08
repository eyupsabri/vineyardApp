using Microsoft.EntityFrameworkCore.Storage;

namespace DAL
{
    public interface IUnitOfWork : IDisposable
    {
        IIoTDeviceRepository IoTDevicesRepo { get; }
        IPumpSessionRepository PumpSessionsRepo { get; }
        IUserRepository UserRepo { get; }
        IPumpRepository PumpRepo { get; }
        Task<IDbContextTransaction> BeginTransactionAsync();

        Task<int> SaveChangesAsync();
    }

}
