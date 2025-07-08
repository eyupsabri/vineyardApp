namespace Entities.DTOs
{
    public class PumpResponseDTO
    {
        public int Id { get; set; }
        public Guid DeviceIdentifier { get; set; }
        public string Name { get; set; }
        public bool ActualState { get; set; }
        public DateTime? LastHeartbeat { get; set; }
        public bool NeedsAttention { get; set; }
        public bool IsManualOverride { get; set; }
    }
}
