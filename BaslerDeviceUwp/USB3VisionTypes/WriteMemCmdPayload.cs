using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodaDevices.Devices.BaslerWinUsb.USB3VisionTypes
{
    public struct WriteMemCmdPayload
    {
        public Int64 address; // register to write data to
        public Int32 data; // variable size array of actual data to write
    }
}
