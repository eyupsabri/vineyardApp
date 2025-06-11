using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities
{
    public class PumpSession
    {
        public int Id { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime? EndTime { get; set; }

        public int PumpId { get; set; }
        public Pump Pump { get; set; }

        public TimeSpan? Duration => EndTime.HasValue ? EndTime - StartTime : null;
    }
}
