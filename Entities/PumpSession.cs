using static Entities.Enums.DbEnums;

namespace Entities
{
    public class PumpSession
    {
        public int Id { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public bool WasInterrupted { get; set; }
        public PumpStateChangeSource StartSource { get; set; }
        public PumpStateChangeSource? EndSource { get; set; }
        public int PumpId { get; set; }
        public Pump Pump { get; set; }

        public TimeSpan? Duration => EndTime.HasValue ? EndTime - StartTime : null;
    }
}
