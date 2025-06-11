using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities
{
    public class UserDevice
    {
        public int UserId { get; set; }
        public User User { get; set; }

        public int IoTDeviceId { get; set; }
        public IoTDevice IoTDevice { get; set; }
    }
}
