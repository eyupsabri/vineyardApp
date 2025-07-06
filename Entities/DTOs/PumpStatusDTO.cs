namespace Entities.DTOs
{
    public class PumpStatusDTO
    {
        public Guid DeviceIdentifier { get; set; }
        public bool DesiredState { get; set; }
        public bool ActualState { get; set; }
        public DateTime? LastHeartBeat { get; set; }

    }
}
