using static Entities.Enums.Enums;

namespace Entities.DTOs
{
    public class UpdateStatusRequestDTO
    {
        public required Guid DeviceIdentifier { get; set; }
        public required bool ActualState { get; set; }
        public DeviceTriggerSource TriggeredBy { get; set; }

    }
}
