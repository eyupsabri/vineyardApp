namespace Entities
{
    public class IoTDevice
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public Guid DeviceIdentifier { get; set; }

        public ICollection<UserDevice> UserDevices { get; set; }

        public Pump Pump { get; set; }
        public TemperatureSensor TemperatureSensor { get; set; }
    }
}
