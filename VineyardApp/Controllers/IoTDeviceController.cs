using AutoMapper;
using Business.FilterAndSort;
using Business.Filters;
using Business.PagedList;
using Business.Results;
using Business.Services;
using DAL;
using Entities;
using Entities.DTOs;
using Microsoft.AspNetCore.Mvc;
using VineyardApp.ActionFilters;

namespace VineyardApp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class IoTDeviceController : ControllerBase
    {
        private IIoTDevicesService _iotDeviceService;
        private readonly IMapper _mapper;
        private readonly IUnitOfWork _unitOfWork;

        public IoTDeviceController(IIoTDevicesService iotDeviceService, IMapper mapper, IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
            _iotDeviceService = iotDeviceService;
            _mapper = mapper;
        }


        [HttpPost("SetDesiredState")]
        [ServiceFilter(typeof(AuthActionFilter))]
        public async Task<IActionResult> SetDesiredState(ChangePumpStateDTO dto)
        {
            OperationResult<Pump> result = OperationResult<Pump>.NotFound();
            try
            {
                result = await _iotDeviceService.SetDesiredState(dto);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }


            if (result.IsNotFound) return NotFound();
            if (result.IsConflict) return Conflict(new { message = result.ErrorMessage });
            if (result.IsFailure) return BadRequest(new { message = result.ErrorMessage });
            var pumpMapped = _mapper.Map<PumpResponseDTO>(result.Value);
            return Ok(pumpMapped);
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

        [HttpGet("[action]")]
        [ServiceFilter(typeof(AuthActionFilter))]
        public async Task<IActionResult> GetPumps([FromQuery] PumpFilter filter, int pagedIndex = 0)
        {
            var pumps = _unitOfWork.PumpRepo.QueryWithDevice();
            var filteredPumps = pumps.FilterAndSort(filter);
            var pagedPumps = new PagedList<Pump>(pagedIndex);
            pagedPumps.ToPagedList(filteredPumps);
            var mapped = pagedPumps.FinalList.Select(p => _mapper.Map<PumpResponseDTO>(p));

            return Ok(new
            {
                PageIndex = pagedPumps.PageIndex,
                PageCount = pagedPumps.PageCount,
                data = mapped
            });
        }

        [HttpGet("[action]")]
        [ServiceFilter(typeof(AuthActionFilter))]
        public async Task<IActionResult> GetPumpsByIds([FromQuery] List<Guid> ids)
        {
            var pump = await _iotDeviceService.GetPumpsWithId(ids);
            if (pump == null)
            {
                return NotFound($"Pump with IDs not found.");
            }
            var mapped = pump.Select(p => _mapper.Map<PumpResponseDTO>(p));
            return Ok(mapped);

        }
    }
}
