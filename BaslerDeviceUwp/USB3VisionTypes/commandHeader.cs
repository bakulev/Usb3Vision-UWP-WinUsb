using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodaDevices.Devices.BaslerWinUsb.USB3VisionTypes
{
    public struct CommandHeader
    {
        public Int32 prefix;  // must be 0x43563355
        public Int16 flags;   // specify U3V_REQUEST_ACK 0x4000 if response is needed
        public Int16 cmd;    // command id 
        public Int16 length; // size of payload
        public Int16 request_id; // unique id of the request (used to identify the response)
    };
}
