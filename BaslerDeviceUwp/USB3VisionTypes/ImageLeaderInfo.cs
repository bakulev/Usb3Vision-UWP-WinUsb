using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace CodaDevices.Devices.BaslerWinUsb.USB3VisionTypes
{
    public struct ImageLeaderInfo
    {
            public long timestamp;
            public int pixel_format;
            public int size_x;
            public int size_y;
            public int offset_x;
            public int offset_y;
            public short padding_x;
            public short reserved;

        public static ImageLeaderInfo ConvertBytes(byte[] data)
        {
            GCHandle handle = GCHandle.Alloc(data, GCHandleType.Pinned);
            try
            {
                ImageLeaderInfo stuff = (ImageLeaderInfo)Marshal.PtrToStructure(handle.AddrOfPinnedObject(), typeof(ImageLeaderInfo));
                return stuff;
            }
            finally
            {
                handle.Free();
            }   
        }
    }
}
