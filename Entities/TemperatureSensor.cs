using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities
{
    public class TemperatureSensor
    {
        public int Id { get; set; }

        public int IoTDeviceId { get; set; }
        public IoTDevice IoTDevice { get; set; }

        public ICollection<TemperatureReading> Readings { get; set; }
    }
}
