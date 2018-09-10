using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace CodaDevices.Devices.BaslerWinUsb.Helpers
{
    public class ArrayHelper
    {
        public static byte[] getBytes(object aux)
        {
            int length = Marshal.SizeOf(aux);
            IntPtr ptr = Marshal.AllocHGlobal(length);
            byte[] myBuffer = new byte[length];

            Marshal.StructureToPtr(aux, ptr, true);
            Marshal.Copy(ptr, myBuffer, 0, length);
            Marshal.FreeHGlobal(ptr);

            return myBuffer;
        }

        public static ushort[,] UnpackImage(byte[] data, int iWidth, int iHeight)
        {
            var result = new ushort[iHeight, iWidth];
            var offset = 0;
            for (var i = 0; i < iHeight; ++i)
                for (var j = 0; j < iWidth; ++j)
                {
                    result[i, j] = BitConverter.ToUInt16(new byte[2] { data[offset + 1], data[offset] }, 0);
                    offset += 2;
                }
            return result;
        }

        public static T[] SubArray<T>(T[] data, int index, int length)
        {
            T[] result = new T[length];
            Array.Copy(data, index, result, 0, length);
            return result;
        }

        public static T[] ConcatArrays<T>(params T[][] list)
        {
            var result = new T[list.Sum(a => a.Length)];
            int offset = 0;
            for (int x = 0; x < list.Length; x++)
            {
                list[x].CopyTo(result, offset);
                offset += list[x].Length;
            }
            return result;
        }
    }
}
