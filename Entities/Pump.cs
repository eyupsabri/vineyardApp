namespace Entities
{
    public class Pump
    {
        public int Id { get; set; }
        public bool DesiredState { get; set; }
        public bool ActualState { get; set; }
        public bool IsManualOverride { get; set; } = false;
        public DateTime? LastHeartbeat { get; set; }
        public DateTime? LastDesiredChange { get; set; }
        public DateTime? LastActualChange { get; set; }
        public int RetryCount { get; set; } = 0;
        public DateTime? LastRetry { get; set; } = null;
        public bool NeedsAttention { get; set; } = false;
        public int IoTDeviceId { get; set; }
        public IoTDevice IoTDevice { get; set; }

        public ICollection<PumpSession> Sessions { get; set; } = new List<PumpSession>();
    }
}
