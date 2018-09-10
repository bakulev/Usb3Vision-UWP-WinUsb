using CodaDevices.Devices.BaslerWinUsb.USB3VisionTypes;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.Usb;
using Windows.Storage.Streams;
using static CodaDevices.Devices.BaslerWinUsb.Constants.USB3Constants;

namespace CodaDevices.Devices.BaslerWinUsb.Helpers
{
    public class CameraInterchangeHelper
    {
        #region Constructors
        public CameraInterchangeHelper()
        {
            //answer required, in another case should be 0
            _header.flags = 0x4000;

            // must be 0x43563355, magic number
            _header.prefix = 0x43563355;  
            _header.length = (short)System.Runtime.InteropServices.Marshal.SizeOf(
                typeof(ReadMemCmdPayload));
        }
        #endregion

        #region Fields
        CommandHeader _header;
        private static short _commandId;
        #endregion
        
        #region Properties

        public UsbBulkInPipe StreamInPipe { get; set; }
        public UsbBulkInPipe ControlInPipe { get; set; }
        public UsbBulkOutPipe ControlOutPipe { get; set; }
        #endregion

        #region Methods
        public async Task<uint> SendCommand(byte[] cmd)
        {
            //Setup the pipe.
            //ControlOutPipe.WriteOptions |= UsbWriteOptions.ShortPacketTerminate;

            //Setup the stream.
            var stream = ControlOutPipe.OutputStream;
            using (DataWriter writer = new DataWriter(stream))
            {

                //WriteAllData.
                writer.WriteBytes(cmd);

                //Push.
                UInt32 bytesWritten = 0;
                try
                {
                    bytesWritten = await writer.StoreAsync();
                }
                catch (Exception exception)
                {
                    Debug.WriteLine(exception.Message.ToString());
                }
                return bytesWritten;
            }
        }

        public uint MaxBufferSize => StreamInPipe.MaxTransferSizeBytes;

        public async Task<Int64> GetRegisterValueAsync(long address)
        {
            byte[] result = await SendReadCommandWithResult(address, 8);
            return BitConverter.ToInt64(result, result.Length - 8);
        }

        public async Task<Int32> GetBlocksSizeAsync(long address)
        {
            byte[] result = await SendReadCommandWithResult(address, 4);
            return BitConverter.ToInt32(result, result.Length - 4);
        }

        public async Task<Int16> SetConfigRegisterAsync(long address, Int32 value)
        {
            byte[] result = await SendWriteCommandWithResult(address, value);
            return BitConverter.ToInt16(ArrayHelper.SubArray(result, 4, 2), 0);

        }

        public async Task<byte[]> GetImageData()
        {
            if ((StreamInPipe.ReadOptions & UsbReadOptions.AllowPartialReads) != 0)
                StreamInPipe.ReadOptions ^= UsbReadOptions.AllowPartialReads;
            StreamInPipe.ReadOptions |= UsbReadOptions.OverrideAutomaticBufferManagement;

            //Setup stream.
            var stream = StreamInPipe.InputStream;
            using (DataReader reader = new DataReader(stream))
            {

                uint bytesRead = 0;
                try
                {
                    bytesRead = await reader.LoadAsync(StreamInPipe.MaxTransferSizeBytes);
                }
                catch (Exception exception)
                {
                    Debug.WriteLine(exception.Message);
                }

                //Get buffer
                IBuffer buffer = reader.ReadBuffer(bytesRead);
                var result = buffer.ToArray();

                return result;
            }
        }

        private async Task<byte[]> GetResults()
        {
            //Config
            // pipe.ReadOptions |= UsbReadOptions.IgnoreShortPacket;

            //Setup stream.
            var stream = ControlInPipe.InputStream;
            using (DataReader reader = new DataReader(stream))
            {

                uint bytesRead = 0;
                try
                {
                    bytesRead = await reader.LoadAsync(ControlInPipe.EndpointDescriptor.MaxPacketSize);
                }
                catch (Exception exception)
                {
                    Debug.WriteLine(exception.Message);
                }

                //Get buffer
                IBuffer buffer = reader.ReadBuffer(bytesRead);
                var result = buffer.ToArray();

                return result;
            }
        }

        private async Task<byte[]> SendReadCommandWithResult(long address, short bytesToRead)
        {
            ReadMemCmdPayload payload = new ReadMemCmdPayload();
            payload.address = address; //Get SBRM register
            payload.byte_count = bytesToRead;

            _header.request_id = ++_commandId;
            _header.cmd = READMEM_CMD;

            //create the cmd.
            byte[] cmd = ArrayHelper.ConcatArrays(ArrayHelper.getBytes(_header),
                ArrayHelper.SubArray(ArrayHelper.getBytes(payload), 0, 12));

            //Now send.
            await SendCommand(cmd);

            //Recieve result.
            byte[] result = await GetResults(); 
            return result;
        }

        private async Task<byte[]> SendWriteCommandWithResult(long address, Int32 data)
        {
            var payload = new WriteMemCmdPayload();
            payload.address = address; //Get SBRM register
            payload.data = data;

            _header.request_id = ++_commandId;
            _header.cmd = WRITEMEM_CMD;
            _header.length = 12;

            //create the cmd.
            byte[] cmd = ArrayHelper.ConcatArrays(ArrayHelper.getBytes(_header), 
                ArrayHelper.getBytes(payload));

            //Now send.
            await SendCommand(cmd);

            //Recieve result.
            byte[] result = await GetResults(); 
            return result;
        }
        #endregion
    }
}
