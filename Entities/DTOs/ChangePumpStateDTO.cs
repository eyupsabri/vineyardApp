namespace Entities.DTOs
{
    public class ChangePumpStateDTO
    {
        public required Guid DeviceIdentifier { get; set; }
        public required bool DesiredState { get; set; } // true for ON, false for OFF

    }
}
