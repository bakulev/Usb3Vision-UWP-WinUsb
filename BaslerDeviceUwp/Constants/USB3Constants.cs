using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodaDevices.Devices.BaslerWinUsb.Constants
{
    public class USB3Constants
    {
        /* Command Codes */
        public static Int16 READMEM_CMD = 0x0800;
        public static Int16 READMEM_ACK = 0x0801;
        public static Int16 WRITEMEM_CMD = 0x0802;
        public static Int16 WRITEMEM_ACK = 0x0803;
        public static Int16 PENDING_ACK = 0x0805;
        public static Int16 EVENT_CMD = 0x0C00;
    }
}
