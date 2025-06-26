namespace Entities.DTOs
{
    public class IoTPollingDTO
    {
        public Guid DeviceIdentifier { get; set; }
        public bool DesiredState { get; set; }
        public bool IsManualOverride { get; set; }
        public bool ActualState { get; set; }
        public DateTime? LastDesiredChange { get; set; }
        public DateTime? LastActualChange { get; set; }

    }
}
