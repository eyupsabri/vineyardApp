using static Entities.Enums.Enums;

namespace Entities.DTOs
{
    public class StatusPayload
    {
        public bool ActualState { get; set; }
        public DeviceTriggerSource TriggeredBy { get; set; } = DeviceTriggerSource.Unknown;
    }
}
