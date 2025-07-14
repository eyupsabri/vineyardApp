namespace Entities.DTOs
{
    public class PumpStatusDTO
    {
        public Guid DeviceIdentifier { get; set; }
        public bool ActualState { get; set; }
        public bool NeedsAttention { get; set; }
        public DateTime? LastHeartBeat { get; set; }
        public bool IsActive { get; set; }
        public bool Loading { get; set; }

    }
}
