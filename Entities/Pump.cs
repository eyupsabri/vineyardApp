namespace Entities
{
    public class Pump
    {
        public int Id { get; set; }
        public bool DesiredState { get; set; }
        public bool ActualState { get; set; }
        public bool IsManualOverride { get; set; } = false;
        public DateTime? LastHeartbeat { get; set; }
        public int IoTDeviceId { get; set; }
        public IoTDevice IoTDevice { get; set; }

        public ICollection<PumpSession> Sessions { get; set; }
    }
}
