using Business.Results;
using DAL;
using Entities;
using Entities.DTOs;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using static Entities.Enums.DbEnums;
using static Entities.Enums.Enums;

namespace Business.Services
{
    public class IoTDevicesService : IIoTDevicesService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMessagePublisher _publisher;
        private readonly ILogger<IIoTDevicesService> _logger;



        public IoTDevicesService(IUnitOfWork unitOfWork, IMessagePublisher publisher, ILogger<IIoTDevicesService> logger)
        {
            _unitOfWork = unitOfWork;
            _publisher = publisher;
            _logger = logger;
        }

        public async Task<OperationResult> ClearManualOverride(ClearManualOverrideDTO dto)
        {
            var device = await GetIoTDeviceByDeviceId(dto.DeviceIdentifier);
            if (device?.Pump == null) return OperationResult.NotFound();

            using var tx = await _unitOfWork.BeginTransactionAsync();
            try
            {
                device.Pump.IsManualOverride = false;
                device.Pump.DesiredState = device.Pump.ActualState;
                device.Pump.LastDesiredChange = null;
                var saved = await _unitOfWork.SaveChangesAsync();
                if (saved <= 0)
                {
                    _logger.LogWarning("Failed to clear manual override for device {Id}", dto.DeviceIdentifier);
                    await tx.RollbackAsync();
                    return OperationResult.Failure("Failed to clear manual override.");
                }
                var topic = $"vineyard/{dto.DeviceIdentifier}/overrideclear";
                await _publisher.PublishAsync(topic, string.Empty);
                await tx.CommitAsync(); // Commit the transaction if everything is successful
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to clear manual override for device {Id}", dto.DeviceIdentifier);
                await tx.RollbackAsync();
                return OperationResult.Failure("Failed to clear manual override.");
            }

            return OperationResult.Success();
        }

        public async Task<IoTDevice> GetIoTDeviceByDeviceId(Guid id)
        {
            return await _unitOfWork.IoTDevicesRepo.GetIoTDeviceByDeviceId(id);
        }
        //App den geleni publish et
        public async Task<OperationResult> SetDesiredState(ChangePumpStateDTO dto)
        {
            var device = await GetIoTDeviceByDeviceId(dto.DeviceIdentifier);
            if (device == null)
            {
                _logger.LogWarning("Device {Id} not found", dto.DeviceIdentifier);

                return OperationResult.NotFound();
            }

            var pump = device.Pump;
            if (pump.IsManualOverride)
            {
                _logger.LogInformation("Pump {Id} in manual override, skipping", dto.DeviceIdentifier);
                return OperationResult.Failure("Pump is in manual override.");
            }
            var now = DateTime.UtcNow;
            var last = pump.LastDesiredChange ?? DateTime.UnixEpoch;
            var elapsed = now - last;
            if (elapsed < TimeSpan.FromMinutes(1))
            {
                _logger.LogInformation("Pump {Id} toggled {ElapsedSec}s ago (<60s), skipping", dto.DeviceIdentifier, elapsed.TotalSeconds);
                return OperationResult.Conflict($"Wait {(60 - (int)elapsed.TotalSeconds)}s before toggling.");
            }
            _logger.LogInformation("Current DesiredState for {Id} is {Old}, requested {New}", dto.DeviceIdentifier, pump.DesiredState, dto.DesiredState);
            if (pump.DesiredState != dto.DesiredState)
            {
                _logger.LogInformation("Updating DesiredState for {Id} to {New}", dto.DeviceIdentifier, dto.DesiredState);


                using var tx = await _unitOfWork.BeginTransactionAsync();
                try
                {

                    // 1) Update and save DB
                    pump.DesiredState = dto.DesiredState;
                    pump.LastDesiredChange = now;
                    // reset the retry machinery because it's a new command
                    pump.RetryCount = 0;
                    pump.LastRetry = null;
                    pump.NeedsAttention = false;
                    var saved = await _unitOfWork.SaveChangesAsync();
                    if (saved <= 0)
                    {
                        await tx.RollbackAsync();
                        return OperationResult.Failure("Failed to record desired state.");
                    }

                    _logger.LogInformation(saved > 0 ? "[DB] SaveChangesAsync succeeded" : "[DB] SaveChangesAsync did not persist any rows");
                    // 2) Publish to MQTT

                    var topic = $"vineyard/{dto.DeviceIdentifier}/command";
                    var payload = JsonSerializer.Serialize(new { desired = dto.DesiredState });

                    _logger.LogInformation("[MQTT] Publishing to {Topic}: {Payload}", topic, payload);
                    await _publisher.PublishAsync(topic, payload);
                    await tx.CommitAsync(); // Commit the transaction if everything is successful
                    _logger.LogInformation("[MQTT] Publish succeeded for {Topic}", topic);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Publish failed, rolling back DB change");
                    await tx.RollbackAsync();
                    return OperationResult.Failure("Unable to send command, please try again.");
                }

            }

            return OperationResult.Success();
        }



        //Subscriber'dan geleni DB'ye kaydet
        public async Task<int> UpdateDeviceStatus(UpdateStatusRequestDTO dto)
        {
            var iotDevice = await GetIoTDeviceByDeviceId(dto.DeviceIdentifier);
            if (iotDevice == null || iotDevice.Pump == null)
                return -1;

            var now = DateTime.UtcNow;
            var pump = iotDevice.Pump;
            var previousHeartbeat = pump.LastHeartbeat;
            pump.LastHeartbeat = now;
            ////////
            bool actualStateChanged = pump.ActualState != dto.ActualState;

            _logger.LogInformation("[Service] Pump {PumpId} current ActualState={Old}, incoming={New}", pump.Id, pump.ActualState, dto.ActualState);
            if (actualStateChanged)
            {
                pump.ActualState = dto.ActualState;
                pump.LastActualChange = now;

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
                        StartTime = now,
                        StartSource = changeSource,
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
                        pump.DesiredState = dto.ActualState;
                        var session = await _unitOfWork.PumpSessionsRepo.GetPumpSessionById(sessionId.Value);
                        session.EndTime = DateTime.UtcNow;
                        session.EndSource = changeSource;
                        if (previousHeartbeat.HasValue && (now - previousHeartbeat.Value) > TimeSpan.FromMinutes(5))
                        {
                            session.EndTime = previousHeartbeat;
                            session.WasInterrupted = true;
                            session.EndSource = null;
                        }
                    }

                    pump.IsManualOverride = false;

                }


            }
            return await _unitOfWork.SaveChangesAsync(); // Save changes to the database and return success status

        }
    }
}
