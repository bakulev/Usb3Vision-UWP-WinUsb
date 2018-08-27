using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.Enumeration;
using Windows.Devices.Usb;
using Windows.Foundation;
using Windows.Storage.Streams;
using System.Threading;
using Windows.ApplicationModel;
using Windows.UI.Core;
using Windows.ApplicationModel.Core;
using Windows.UI.Xaml.Media.Imaging;

namespace CodaDevices.Uwp.Devices
{
    public class ClearShotCamera
    {

        public ClearShotCamera()
        {
            //Register for onsuspending event.
            Windows.UI.Xaml.Application.Current.Suspending += OnAppExiting;
        }

        #region Constants

        //Device stuff.
        const UInt32 VendorId = 0x184C;
        const UInt32 ProductId = 0x0001;

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

        #endregion

        #region Public Funcs

        public void StartWaitForCamera()
        {
            //Create watcher to watch for device connection.
            string aqs = UsbDevice.GetDeviceSelector(VendorId, ProductId);
            var superMuttWatcher = DeviceInformation.CreateWatcher(aqs);
            superMuttWatcher.Added += this.OnDeviceAdded;
            superMuttWatcher.Removed += this.OnDeviceRemoved;
            superMuttWatcher.Start();
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
            if (deviceInformation.Name.StartsWith("ClearShot") && deviceInformation.IsEnabled)
            {
                _targetDevice = await UsbDevice.FromIdAsync(deviceInformation.Id);
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
            UsbBulkOutPipe writePipe = _targetDevice.DefaultInterface.BulkOutPipes[1]; //not sure which one, lets try..
            writePipe.WriteOptions |= UsbWriteOptions.ShortPacketTerminate;

            //Setup the stream.
            var stream = writePipe.OutputStream;
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
        private async Task<byte[]> GetResults(int inPipe = 1)
        {
            //Config
            UsbBulkInPipe readPipe = _targetDevice.DefaultInterface.BulkInPipes[inPipe];
            readPipe.ReadOptions |= UsbReadOptions.IgnoreShortPacket;

            //Setup stream.
            var stream = readPipe.InputStream;
            DataReader reader = new DataReader(stream);

            uint bytesRead = 0;
            try 
            {
                bytesRead = await reader.LoadAsync(readPipe.EndpointDescriptor.MaxPacketSize);
            }
            catch (Exception exception)
            {
                Debug.WriteLine(exception.Message);
            }

            //Get buffer
            IBuffer buffer = reader.ReadBuffer(bytesRead);
            var result = buffer.ToArray();

            //Change result depending on size.
            var size = BitConverter.ToUInt32(result, 1);

            if (size > result.Count())
            {
                //We need to actually get more data.
                while (size >= result.Count())
                {
                    bytesRead = await reader.LoadAsync(readPipe.EndpointDescriptor.MaxPacketSize);
                    buffer = reader.ReadBuffer(bytesRead);
                    var newRes = ConcatArrays(result, buffer.ToArray());
                    result = newRes;
                }
            }

            //Remove padded zeros and return.
            if (size < result.Count())
            {
                return result.Take((int)size).ToArray();
            }

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
            byte[] cmd = new byte[6];
            MemoryStream writer = new MemoryStream(cmd);

            //Write.
            writer.WriteByte(CONST_GetCalibration); //Write cmd type.
            byte[] amountOBytes = BitConverter.GetBytes(Convert.ToUInt32(cmd.Count())).ToArray();
            writer.Write(amountOBytes, 0, amountOBytes.Count()); //Write cmd Size.
            writer.WriteByte(0x01); //Scheme.
            writer.Flush();
            writer.Dispose();

            //Now Send.
            await SendCommand(cmd);

            //Receive result.
            byte[] result = await GetResults();

            //Parse it.
            if (result[0] == CONST_GetCalibration) // first byte is code of command (the same as request)
            {
                uint iCommandSize = BitConverter.ToUInt32(result, 1); // Overall size of command.
                if (result[5] == 0x01) // Schema number must be 0x01. (Same as request)
                {
                    if (result[6] == 0x01) //If success.
                    {
                        return Encoding.UTF8.GetString(result, 64, result.Length - 65); // Returns the string.
                    }
                    else
                        return "";
                }
                else
                    return "";
            }
            else
                return "";
        }

        public async Task<float> GetExposureTime()
        {
            //intiate the cmd.
            byte[] cmd = new byte[6];
            MemoryStream writer = new MemoryStream(cmd);

            //Write.
            writer.WriteByte(CONST_GetExposureTime); //Write cmd type.
            byte[] amountOBytes = BitConverter.GetBytes(Convert.ToUInt32(cmd.Count())).ToArray(); writer.Write(amountOBytes, 0, amountOBytes.Count()); //Write cmd Size.
            writer.WriteByte(0x01); //Scheme.
            writer.Flush();
            writer.Dispose();

            //Now Send.
            await SendCommand(cmd);

            //Receive result.
            byte[] result = await GetResults();

            //Parse it.
            if (result[6] == 0x01) //If success.
            {
                return BitConverter.ToSingle(result, 7); //Return the exposure.
            }
            else
                return -1;
        }

        public async Task<string> GetSerialNumber()
        {
            //intiate the cmd.
            byte[] cmd = new byte[6];
            MemoryStream writer = new MemoryStream(cmd);

            //Write.
            writer.WriteByte(CONST_GetSerialNumber); //Write cmd type.
            byte[] amountOBytes = BitConverter.GetBytes(Convert.ToUInt32(cmd.Count())).ToArray();
            writer.Write(amountOBytes, 0, amountOBytes.Count()); //Write cmd Size.
            writer.WriteByte(0x01); //Scheme.
            writer.Flush();
            writer.Dispose();

            //Now Send.
            await SendCommand(cmd);

            //Receive result.
            byte[] result = await GetResults();

            //Parse it.
            if (result[0] == CONST_GetSerialNumber)
            {
                uint iCommandSize = BitConverter.ToUInt32(result, 1);
                if (result[5] == 0x01) // Schema number must be 0x01.
                { 
                    if (result[6] == 0x01) //If success.
                    {
                        uint length = BitConverter.ToUInt16(result, 7); // length of string.
                        return Encoding.UTF8.GetString(result, 9, (int)(length - 1)); // Returns the string.
                    }
                    else
                        return "";
                }
                else
                    return "";
            } else
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

        public async Task<bool> StartExposure(byte shutterState, byte exposureType)
        {
            //create the cmd.
            byte[] cmd = new byte[8];
            MemoryStream writer = new MemoryStream(cmd);

            //Write the cmd.
            writer.WriteByte(CONST_StartExposure);
            byte[] amountOBytes = BitConverter.GetBytes(Convert.ToUInt32(cmd.Count())).ToArray(); writer.Write(amountOBytes, 0, amountOBytes.Count()); //Write cmd Size.
            writer.WriteByte(0x01); //Scheme.
            writer.WriteByte(shutterState);
            writer.WriteByte(exposureType);
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
        public async Task<ushort[,]> GetExposure()
        {
            //create the cmd.
            byte[] cmd = new byte[6];
            MemoryStream writer = new MemoryStream(cmd);

            //Write the cmd.
            writer.WriteByte(CONST_GetExposure);
            byte[] amountOBytes = BitConverter.GetBytes(Convert.ToUInt32(cmd.Count())).ToArray(); writer.Write(amountOBytes, 0, amountOBytes.Count()); //Write cmd Size.
            writer.WriteByte(0x01); //Scheme.
            writer.Flush();
            writer.Dispose();

            //Now send.
            await SendCommand(cmd);

            //Recieve result.
            byte[] result = await GetResults(0); //Use inpoint 0 for image data.

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
