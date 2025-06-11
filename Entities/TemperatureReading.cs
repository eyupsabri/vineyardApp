using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities
{
    public class TemperatureReading
    {
        public int Id { get; set; }
        public float ValueCelsius { get; set; }
        public DateTime Timestamp { get; set; }

        public int TemperatureSensorId { get; set; }
        public TemperatureSensor TemperatureSensor { get; set; }
    }
}
