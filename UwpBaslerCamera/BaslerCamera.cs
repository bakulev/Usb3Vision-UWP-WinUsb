using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.Devices.Enumeration;
using Windows.Devices.Usb;
using Windows.Storage.Streams;



namespace UwpBaslerCamera
{


    struct command_header
    {
        public Int32 prefix;  // must be 0x43563355
        public Int16 flags;   // specify U3V_REQUEST_ACK 0x4000 if response is needed
        public Int16 cmd;    // command id 
        public Int16 length; // size of payload
        public Int16 request_id; // unique id of the request (used to identify the response)
    };

    struct read_mem_cmd_payload
    {
        public Int64 address; // register address to read data from
        public Int16 reserved; // must be 0
        public Int16 byte_count; // how much data to read
    };

    public class BaslerCamera
    {
        public BaslerCamera()
        {
            _header.flags = 0x4000;
            _header.prefix = 0x43563355;  // must be 0x43563355
            _header.length = (short)System.Runtime.InteropServices.Marshal.SizeOf(typeof(read_mem_cmd_payload));
            //Register for onsuspending event.
            //Windows.UI.Xaml.Application.Current.Suspending += OnAppExiting;
        }

        #region Constants

        /* Command Codes */
        private Int16 READMEM_CMD = 0x0800;
        private Int16 READMEM_ACK = 0x0801;
        private Int16 WRITEMEM_CMD = 0x0802;
        private Int16 WRITEMEM_ACK = 0x0803;
        private Int16 PENDING_ACK = 0x0805;
        private Int16 EVENT_CMD = 0x0C00;

        //Device stuff.
        const UInt32 VendorId = 0x2676;
        const UInt32 ProductId = 0xBA02;

        const byte CONST_GetCCDTemperatureInfo = 0x03;
        const byte CONST_SetCCDTemperatureInfo = 0x04;
        const byte CONST_GetCalibration = 0x05;
        const byte CONST_SetCalibration = 0x06;
        const byte CONST_OpenShutter = 0x07;
        const byte CONST_CloseShutter = 0x08;
        const byte CONST_GetExposureTime = 0x09;
        const byte CONST_SetExposureTime = 0x0A;
        const byte CONST_StartExposure = 0x0B;
        const byte CONST_QueryExposure = 0x0C;
        const byte CONST_EndExposure = 0x0D;
        const byte CONST_GetExposure = 0x0E;
        const byte CONST_GetReconstruction = 0x0F;
        const byte CONST_SetReconstruction = 0x10;
        const byte CONST_GetLastError = 0x11;
        const byte CONST_GetModelNumber = 0x12;
        const byte CONST_SetModelNumber = 0x13;
        const byte CONST_GetSerialNumber = 0x14;
        const byte CONST_SetSerialNumber = 0x15;
        const byte CONST_GetSpectrometerInformation = 0x16;
        const byte CONST_UpdateFX2 = 0x17;
        const byte CONST_UpdateDSP = 0x18;
        const byte CONST_UpdateFPGA = 0x19;
        const byte CONST_DumpRAM = 0x1A;
        const byte CONST_GetClockRate = 0x1B;
        const byte CONST_SetClockRate = 0x1C;
        const byte CONST_GetPixelMode = 0x1D;
        const byte CONST_SetPixelMode = 0x1E;
        const byte CONST_Reset = 0x1F;
        const byte CONST_GetLaser = 0x20;
        const byte CONST_SetLaser = 0x21;
        const byte CONST_GetLaserEnabled = 0x22;
        const byte CONST_SetLaserEnabled = 0x23;
        const byte CONST_GetLaserTemperature = 0x24;
        const byte CONST_SetLaserTemperature = 0x25;
        const byte CONST_GetGeneralIOConfiguration = 0x26;
        const byte CONST_SetGeneralIOConfiguration = 0x27;
        const byte CONST_GetGeneralIOValue = 0x28;
        const byte CONST_SetGeneralIOValue = 0x29;
        const byte CONST_GetAFEParameters = 0x2A;
        const byte CONST_SetAFEParameters = 0x2B;
        const byte CONST_StartAveragedExposure = 0x2C;
        const byte CONST_MoveStepperAxisToStopPoint = 0x30;
        const byte CONST_MoveStepperAxisToAbsolutePosition = 0x31;
        const byte CONST_MoveStepperAxisToRelativePosition = 0x32;
        const byte CONST_QueryStepperAxisPosition = 0x33;
        const byte CONST_GetStepperAxisConfiguration = 0x34;
        const byte CONST_SetStepperAxisConfiguration = 0x35;
        const byte CONST_GetIllumination = 0x36;
        const byte CONST_SetIllumination = 0x37;
        const byte CONST_UpdateStepperAxisPosition = 0x38;
        const byte CONST_GetDiagnosticRecord = 0xFF;

        #endregion

        #region Private Variables

        private UsbDevice _targetDevice;
        private UsbBulkInPipe _streamInPipe;
        private UsbBulkInPipe _controlInPipe;
        private UsbBulkOutPipe _controlOutPipe;
        command_header _header;


        #endregion

        #region Public Funcs

        public void StartWaitForCamera()
        {

            //Create watcher to watch for device connection.
            string aqs = UsbDevice.GetDeviceSelector(VendorId, ProductId);
            var superMuttWatcher = DeviceInformation.CreateWatcher(aqs);
            superMuttWatcher.Added += this.OnDeviceAdded;
            superMuttWatcher.Removed += this.OnDeviceRemoved;
            superMuttWatcher.Updated += SuperMuttWatcher_Updated;
            superMuttWatcher.Start();
        }


        private void SuperMuttWatcher_Updated(DeviceWatcher sender, DeviceInformationUpdate args)
        {
            throw new NotImplementedException();
        }

        public void CloseCameraHandle()
        {
            if (_targetDevice != null)
            {
                _targetDevice.Dispose();
                _targetDevice = null;
            }
        }

        #endregion

        #region MyEvents

        public event EventHandler<EventArgs> Connected;

        protected virtual void OnConnected(EventArgs e)
        {
            Connected?.Invoke(this, e);
        }

        public event EventHandler<EventArgs> Disconnected;

        protected virtual void OnDisconnected(EventArgs e)
        {
            Disconnected?.Invoke(this, e);
        }

        #endregion

        #region Callbacks

        private async void OnDeviceAdded(DeviceWatcher watcher, DeviceInformation deviceInformation)
        {
            if (deviceInformation.Name.StartsWith("acA1920-40um") && deviceInformation.IsEnabled)
            {
                _targetDevice = await UsbDevice.FromIdAsync(deviceInformation.Id);
                foreach (var interf in _targetDevice.Configuration.UsbInterfaces)
                {
                    if (interf.InterfaceNumber == 0)
                    {
                        _controlInPipe = interf.BulkInPipes[0];
                        _controlOutPipe = interf.BulkOutPipes[0];
                    }
                    else if (interf.InterfaceNumber == 2)
                        _streamInPipe = interf.BulkInPipes[0];
                }

                OnConnected(EventArgs.Empty);
            }
        }

        private void OnDeviceRemoved(DeviceWatcher watcher, DeviceInformationUpdate deviceInformation)
        {
            _targetDevice = null;
            OnDisconnected(EventArgs.Empty);
        }

        private void OnAppExiting(object sender, SuspendingEventArgs e)
        {
            _targetDevice?.Dispose();
        }
        #endregion

        #region PrivateFuncs

        private async Task<uint> SendCommand(byte[] cmd)
        {
            //Setup the pipe.
            _controlOutPipe.WriteOptions |= UsbWriteOptions.ShortPacketTerminate;

            //Setup the stream.
            var stream = _controlOutPipe.OutputStream;
            DataWriter writer = new DataWriter(stream);

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

        /// <summary>
        /// Get Result Of Latest CMD.
        /// </summary>
        /// <param name="inPipe">1 For CMD Response and 0 for Image Data.</param>
        /// <returns></returns>
        private async Task<byte[]> GetResults()
        {
            //Config
            //_controlInPipe.ReadOptions |= UsbReadOptions.IgnoreShortPacket;

            //Setup stream.
            var stream = _controlInPipe.InputStream;
            DataReader reader = new DataReader(stream);

            uint bytesRead = 0;
            try
            {
                bytesRead = await reader.LoadAsync(_controlInPipe.EndpointDescriptor.MaxPacketSize);
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

        /// <summary>
        /// Concatinates arrays.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list"></param>
        /// <returns></returns>
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

        #endregion

        #region CameraFuncs

        public async Task<string> GetCalibration()
        {
            //intiate the cmd.

                return "";
        }

        public async Task<float> GetExposureTime()
        {

                return -1;
        }

        public async Task<string> GetSerialNumber()
        {
            //intiate the cmd.
            return "";
        }

        public async Task<int> SetExposureTime(float value)
        {
            //create the cmd.
            byte[] cmd = new byte[10];
            MemoryStream writer = new MemoryStream(cmd);

            //Write the cmd.
            writer.WriteByte(CONST_SetExposureTime);
            byte[] amountOBytes = BitConverter.GetBytes(Convert.ToUInt32(cmd.Count())).ToArray(); writer.Write(amountOBytes, 0, amountOBytes.Count()); //Write cmd Size.
            writer.WriteByte(0x01); //Scheme.
            byte[] valueBytes = BitConverter.GetBytes(value); writer.Write(valueBytes, 0, valueBytes.Count());
            writer.Flush();
            writer.Dispose();

            //Now send.
            await SendCommand(cmd);

            //Recieve result.
            byte[] result = await GetResults();
            if (result[6] == 0x01) //If success.
            {
                return 1;
            }
            else
                return 0;
        }

        public async Task<bool> IsLaserAvailable()
        {
            //create the cmd.
            byte[] cmd = new byte[6];
            MemoryStream writer = new MemoryStream(cmd);

            //Write the cmd.
            writer.WriteByte(CONST_GetSpectrometerInformation);
            byte[] amountOBytes = BitConverter.GetBytes(Convert.ToUInt32(cmd.Count())).ToArray(); writer.Write(amountOBytes, 0, amountOBytes.Count()); //Write cmd Size.
            writer.WriteByte(0x01); //Scheme.
            writer.Flush();
            writer.Dispose();

            //Now send.
            await SendCommand(cmd);

            //Recieve result.
            byte[] result = await GetResults();
            if ((uint)result[29] > 0) //If success.
            {
                return true;
            }
            else
                return false;
        }

        public async Task<bool> SetLaserEnabled(byte laser, bool state)
        {
            //create the cmd.
            byte[] cmd = new byte[8];
            MemoryStream writer = new MemoryStream(cmd);

            //Write the cmd.
            writer.WriteByte(CONST_SetLaserEnabled);
            byte[] amountOBytes = BitConverter.GetBytes(Convert.ToUInt32(cmd.Count())).ToArray(); writer.Write(amountOBytes, 0, amountOBytes.Count()); //Write cmd Size.
            writer.WriteByte(0x01); //Scheme.
            writer.WriteByte(laser);
            writer.WriteByte(Convert.ToByte(state));
            writer.Flush();
            writer.Dispose();

            //Now send.
            await SendCommand(cmd);

            //Recieve result.
            byte[] result = await GetResults();
            if (result[6] == 0x01) //If success.
            {
                return true;
            }
            else
                return false;
        }

        public async Task<bool> StartExposure()
        {
            //create the cmd.
            //byte[] cmd = new byte[8];
            //MemoryStream writer = new MemoryStream(cmd);

            ////Write the cmd.
            //writer.WriteByte(CONST_StartExposure);
            //byte[] amountOBytes = BitConverter.GetBytes(Convert.ToUInt32(cmd.Count())).ToArray(); writer.Write(amountOBytes, 0, amountOBytes.Count()); //Write cmd Size.
            //writer.WriteByte(0x01); //Scheme.
            //writer.WriteByte(shutterState);
            //writer.WriteByte(exposureType);
            //writer.Flush();
            //writer.Dispose();

            ////Now send.
            //await SendCommand(cmd);

            ////Recieve result.
            //byte[] result = await GetResults();
            //if (result[6] == 0x01) //If success.
            //{
            //    return true;
            //}
            //else
            return false;
        }

        public async Task<bool> QueryExposure()
        {
            //create the cmd.
            byte[] cmd = new byte[6];
            MemoryStream writer = new MemoryStream(cmd);

            //Write the cmd.
            writer.WriteByte(CONST_QueryExposure);
            byte[] amountOBytes = BitConverter.GetBytes(Convert.ToUInt32(cmd.Count())).ToArray(); writer.Write(amountOBytes, 0, amountOBytes.Count()); //Write cmd Size.
            writer.WriteByte(0x01); //Scheme.
            writer.Flush();
            writer.Dispose();

            //Now send.
            await SendCommand(cmd);

            //Recieve result.
            byte[] result = await GetResults();
            if (result[7] == 0x01) //If exposure available.
            {
                return true;
            }
            else
                return false;
        }

        public async Task<bool> EndExposure(byte shutterState)
        {
            //create the cmd.
            byte[] cmd = new byte[7];
            MemoryStream writer = new MemoryStream(cmd);

            //Write the cmd.
            writer.WriteByte(CONST_EndExposure);
            byte[] amountOBytes = BitConverter.GetBytes(Convert.ToUInt32(cmd.Count())).ToArray(); writer.Write(amountOBytes, 0, amountOBytes.Count()); //Write cmd Size.
            writer.WriteByte(0x01); //Scheme.
            writer.WriteByte(shutterState);
            writer.Flush();
            writer.Dispose();

            //Now send.
            await SendCommand(cmd);

            //Recieve result.
            byte[] result = await GetResults();
            if (result[6] == 0x01) //If correct.
            {
                return true;
            }
            else
                return false;
        }

        public async Task<int[]> GetExposureWidthHeightBits()
        {
            //create the cmd.
            byte[] cmd = new byte[6];
            MemoryStream writer = new MemoryStream(cmd);

            //Write the cmd.
            writer.WriteByte(CONST_GetSpectrometerInformation);
            byte[] amountOBytes = BitConverter.GetBytes(Convert.ToUInt32(cmd.Count())).ToArray(); writer.Write(amountOBytes, 0, amountOBytes.Count()); //Write cmd Size.
            writer.WriteByte(0x01); //Scheme.
            writer.Flush();
            writer.Dispose();

            //Now send.
            await SendCommand(cmd);

            //Recieve result.
            byte[] result = await GetResults();
            if ((uint)result[6] == 0x01) //If success.
            {
                int width = BitConverter.ToUInt16(result, 7);
                int height = BitConverter.ToUInt16(result, 9);
                return new int[] { height, width };
            }
            else
                return null;
        }

        private byte[] getBytes(object aux)
        {
            int length = Marshal.SizeOf(aux);
            IntPtr ptr = Marshal.AllocHGlobal(length);
            byte[] myBuffer = new byte[length];

            Marshal.StructureToPtr(aux, ptr, true);
            Marshal.Copy(ptr, myBuffer, 0, length);
            Marshal.FreeHGlobal(ptr);

            return myBuffer;
        }

        public async Task<ushort[,]> GetExposure()
        {
            read_mem_cmd_payload payload = new read_mem_cmd_payload();
            payload.address = 0x001D8; //Get SBRM register
            payload.byte_count = 8;

            _header.request_id = 0;
            _header.cmd = READMEM_CMD;

            //create the cmd.
            byte[] cmd = ConcatArrays(getBytes(_header), getBytes(payload));

            //Now send.
            await SendCommand(cmd);

            //Recieve result.
            byte[] result = await GetResults(); //Use inpoint 0 for image data.

            //Make sure it successded.
            if (result[6] == 0x01)
            {
                uint imageSize = BitConverter.ToUInt32(result, 8);
                if (imageSize > 0)
                {
                    //Extract the image into an array by itself.
                    int offset = 64;

                    //get size.
                    int[] dims = await GetExposureWidthHeightBits();
                    int height = dims[0];
                    int width = dims[1];

                    //The array to be returned.
                    ushort[,] imageData = new ushort[height, width];

                    //loop and fill new array
                    for (int i = 0; i < height; i++)
                    {
                        for (int j = 0; j < width; j++)
                        {
                            imageData[i, j] = BitConverter.ToUInt16(result, offset);
                            offset += 2;
                        }
                    }

                    //Return.
                    return imageData;
                }
                else
                    return null;
            }
            else
                return null;
        }
        #endregion
    }
}
