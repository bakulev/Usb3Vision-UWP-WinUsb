using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BaslerDeviceUwp.USB3VisionTypes
{
    public struct ReadMemCmdPayload
    {
        public Int64 address; // register address to read data from
        public Int16 reserved; // must be 0
        public Int16 byte_count; // how much data to read
    };
}
