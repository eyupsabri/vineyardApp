using static Entities.Enums.Enums;

namespace Entities.DTOs
{
    public class UpdateStatusRequestDTO
    {
        public Guid DeviceIdentifier { get; set; }
        public bool ActualState { get; set; }
        public DeviceTriggerSource TriggeredBy { get; set; } = DeviceTriggerSource.Unknown;

    }
}
