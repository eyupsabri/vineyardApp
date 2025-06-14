namespace Entities.DTOs
{
    public class IoTPollingDTO
    {
        public Guid DeviceIdentifier { get; set; }
        public bool DesiredState { get; set; }
        public bool IsManualOverride { get; set; }

    }
}
