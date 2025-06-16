namespace DAL
{
    public interface IUnitOfWork : IDisposable
    {
        IIoTDeviceRepository IoTDevicesRepo { get; }
        IPumpSessionRepository PumpSessionsRepo { get; }
        Task<int> SaveChangesAsync();
    }

}
