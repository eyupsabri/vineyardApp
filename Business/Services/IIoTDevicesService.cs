using Business.Results;
using Entities;
using Entities.DTOs;

namespace Business.Services
{
    public interface IIoTDevicesService
    {
        public Task<IoTDevice> GetIoTDeviceByDeviceId(Guid id);
        public Task<int> UpdateDeviceStatus(UpdateStatusRequestDTO dto);
        public Task<OperationResult> SetDesiredState(ChangePumpStateDTO dto);
        public Task<OperationResult> ClearManualOverride(ClearManualOverrideDTO dto);
        public Task<List<Pump>> GetPumpsWithId(List<Guid> id);
    }
}
