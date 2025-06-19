using DAL;
using Entities;
using Entities.DTOs;
using static Entities.Enums.DbEnums;
using static Entities.Enums.Enums;

namespace Business.Services
{
    public class IoTDevicesService : IIoTDevicesService
    {
        private readonly IUnitOfWork _unitOfWork;

        public IoTDevicesService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<IoTDevice> GetIoTDeviceByDeviceId(Guid id)
        {
            return await _unitOfWork.IoTDevicesRepo.GetIoTDeviceByDeviceId(id);
        }

        //public async Task<int> UpdateDeviceStatus(Pump pump, UpdateStatusRequestDTO dto)
        public async Task<int> UpdateDeviceStatus(UpdateStatusRequestDTO dto)
        {
            var iotDevice = await GetIoTDeviceByDeviceId(dto.DeviceIdentifier);
            if (iotDevice == null || iotDevice.Pump == null)
                return -1;

            var pump = iotDevice.Pump;
            pump.LastHeartbeat = DateTime.UtcNow;
            ////////
            bool actualStateChanged = pump.ActualState != dto.ActualState;


            if (actualStateChanged)
            {
                pump.ActualState = dto.ActualState;


                PumpStateChangeSource changeSource = dto.TriggeredBy switch
                {
                    DeviceTriggerSource.Manual => PumpStateChangeSource.Manual,
                    DeviceTriggerSource.IoT => PumpStateChangeSource.IoT,
                    _ => PumpStateChangeSource.App
                };

                if (dto.ActualState)
                {
                    pump.Sessions.Add(new PumpSession
                    {
                        PumpId = pump.Id,
                        StartTime = DateTime.UtcNow,
                        ChangeSource = changeSource,
                    });

                    if (changeSource == PumpStateChangeSource.IoT || changeSource == PumpStateChangeSource.Manual)
                    {
                        pump.IsManualOverride = true;
                    }
                }
                else
                {
                    var sessionId = await _unitOfWork.PumpSessionsRepo.GetLatestSessionIdByDeviceIdentifier(dto.DeviceIdentifier);

                    if (sessionId != null)
                    {
                        var session = await _unitOfWork.PumpSessionsRepo.GetPumpSessionById(sessionId.Value);
                        session.EndTime = DateTime.UtcNow;
                        session.ChangeSource = changeSource;
                    }

                    pump.IsManualOverride = false;
                }


            }
            return await _unitOfWork.SaveChangesAsync(); // Save changes to the database and return success status

        }
    }
}
