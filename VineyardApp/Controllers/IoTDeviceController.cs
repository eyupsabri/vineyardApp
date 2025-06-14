using AutoMapper;
using Business.Services;
using Entities.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace VineyardApp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class IoTDeviceController : ControllerBase
    {
        private IIoTDevicesService _iotDeviceService;
        private readonly IMapper _mapper;

        public IoTDeviceController(IIoTDevicesService iotDeviceService, IMapper mapper)
        {
            _iotDeviceService = iotDeviceService;
            _mapper = mapper;
        }

        [HttpGet("[action]")]
        public async Task<IActionResult> GetDesiredDeviceStatus(Guid deviceIdentifier)
        {
            var iotDevice = await _iotDeviceService.GetIoTDeviceByDeviceId(deviceIdentifier);
            if (iotDevice == null)
            {
                return NotFound($"Device with identifier {deviceIdentifier} not found.");
            }
            var pollingDTO = _mapper.Map<IoTPollingDTO>(iotDevice);
            return Ok(pollingDTO);
        }

        [HttpGet("[action]")]
        public async Task<IActionResult> UpdateIoTDeviceStatus([FromQuery] UpdateStatusRequestDTO dto)
        {
            var iotDevice = await _iotDeviceService.GetIoTDeviceByDeviceId(dto.DeviceIdentifier);

            if (iotDevice == null || iotDevice.Pump == null)
                return NotFound("Device or pump not found");

            var pump = iotDevice.Pump;
            pump.LastHeartbeat = DateTime.UtcNow;

            var success = await _iotDeviceService.UpdateDeviceStatus(pump, dto);
            if (!success)
            {
                return StatusCode(500, "Failed to update device status");
            }

            return Ok();
        }
    }
}
