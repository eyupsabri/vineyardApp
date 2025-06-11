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

        [HttpGet]
        public async Task<IActionResult> GetDesiredDeviceStatus(Guid deviceIdentifier)
        {
            var iotDevice = await _iotDeviceService.GetIoTDeviceByDeviceId(deviceIdentifier);
            if (iotDevice == null)
            {
                return NotFound($"Device with identifier {deviceIdentifier} not found.");
            }
            var desiredStatusMapped = _mapper.Map<DesiredStatusDTO>(iotDevice);
            return Ok(desiredStatusMapped);
        }

        [HttpGet]
        public async Task<IActionResult> ChangeActualDeviceStatus(Guid deviceIdentifier, bool status)
        {
            var success = await _iotDeviceService.ChangeActualDeviceStatus(deviceIdentifier, status);

            return Ok(success);
        }
    }
}
