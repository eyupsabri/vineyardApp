using AutoMapper;
using Business.Results;
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


        [HttpPost("SetDesiredState")]
        public async Task<IActionResult> SetDesiredState(ChangePumpStateDTO dto)
        {
            OperationResult result = OperationResult.NotFound();
            try
            {
                result = await _iotDeviceService.SetDesiredState(dto);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }


            if (result.IsNotFound) return NotFound();
            if (result.IsNotFound) return Conflict(new { message = result.ErrorMessage });
            if (result.IsFailure) return BadRequest(new { message = result.ErrorMessage });
            return Ok(); // Value is your anonymous object { desiredState = ... }
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
            //return Ok(new { desiredState = iotDevice.Pump.DesiredState, lastChanged = iotDevice.Pump.LastStateChanged, deviceIdentifier = iotDevice.DeviceIdentifier, isManualOverride = iotDevice.Pump.IsManualOverride, actualState = iotDevice.Pump.ActualState });
        }

        [HttpPost("[action]")]
        public async Task<IActionResult> ClearManualOverrride([FromBody] ClearManualOverrideDTO dto)
        {
            try
            {
                var result = await _iotDeviceService.ClearManualOverride(dto);
                if (result.IsNotFound) return NotFound($"Device with identifier {dto.DeviceIdentifier} not found.");
                if (result.IsConflict) return Conflict(new { message = result.ErrorMessage });
                if (result.IsFailure) return BadRequest(new { message = result.ErrorMessage });
                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

    }
}
