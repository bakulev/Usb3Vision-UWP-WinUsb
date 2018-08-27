using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using LibUsbDotNet.Main;

namespace Centice.Spectrometry.Spectrometers.Cameras
{
    /// <summary>
    /// Contains camera usb protocol interaction. Implementation indifferent.
    /// OS specific content should be in ClearShotWinUsbService.
    /// if mutex also platform dependent then should be moved to ClearShotWinUsbService, too.
    /// </summary>
    public class ClearShotDevice
    {
        #region Constants

        //Device stuff.
        public const UInt32 VendorId = 0x184C;
        public const UInt32 ProductId = 0x0001;
        public const string GUID = "{dee824ef-729b-4a0e-9c14-b7117d33a817}";

        public const WriteEndpointID SEND_COMMAND_ENDPOINT = WriteEndpointID.Ep04;
        public const ReadEndpointID GET_COMMAND_RESULT_INPOINT = ReadEndpointID.Ep08;
        public const ReadEndpointID GET_IMAGE_RESULT_INPOINT = ReadEndpointID.Ep06;

        public const byte CONST_GetCCDTemperatureInfo = 0x03;
        public const byte CONST_SetCCDTemperatureInfo = 0x04;
        public const byte CONST_GetCalibration = 0x05;
        public const byte CONST_SetCalibration = 0x06;
        public const byte CONST_OpenShutter = 0x07;
        public const byte CONST_CloseShutter = 0x08;
        public const byte CONST_GetExposureTime = 0x09;
        public const byte CONST_SetExposureTime = 0x0A;
        public const byte CONST_StartExposure = 0x0B;
        public const byte CONST_QueryExposure = 0x0C;
        public const byte CONST_EndExposure = 0x0D;
        public const byte CONST_GetExposure = 0x0E;
        public const byte CONST_GetReconstruction = 0x0F;
        public const byte CONST_SetReconstruction = 0x10;
        public const byte CONST_GetLastError = 0x11;
        public const byte CONST_GetModelNumber = 0x12;
        public const byte CONST_SetModelNumber = 0x13;
        public const byte CONST_GetSerialNumber = 0x14;
        public const byte CONST_SetSerialNumber = 0x15;
        public const byte CONST_GetSpectrometerInformation = 0x16;
        public const byte CONST_UpdateFX2 = 0x17;
        public const byte CONST_UpdateDSP = 0x18;
        public const byte CONST_UpdateFPGA = 0x19;
        public const byte CONST_DumpRAM = 0x1A;
        public const byte CONST_GetClockRate = 0x1B;
        public const byte CONST_SetClockRate = 0x1C;
        public const byte CONST_GetPixelMode = 0x1D;
        public const byte CONST_SetPixelMode = 0x1E;
        public const byte CONST_Reset = 0x1F;
        public const byte CONST_GetLaser = 0x20;
        public const byte CONST_SetLaser = 0x21;
        public const byte CONST_GetLaserEnabled = 0x22;
        public const byte CONST_SetLaserEnabled = 0x23;
        public const byte CONST_GetLaserTemperature = 0x24;
        public const byte CONST_SetLaserTemperature = 0x25;
        public const byte CONST_GetGeneralIOConfiguration = 0x26;
        public const byte CONST_SetGeneralIOConfiguration = 0x27;
        public const byte CONST_GetGeneralIOValue = 0x28;
        public const byte CONST_SetGeneralIOValue = 0x29;
        public const byte CONST_GetAFEParameters = 0x2A;
        public const byte CONST_SetAFEParameters = 0x2B;
        public const byte CONST_StartAveragedExposure = 0x2C;
        public const byte CONST_MoveStepperAxisToStopPoint = 0x30;
        public const byte CONST_MoveStepperAxisToAbsolutePosition = 0x31;
        public const byte CONST_MoveStepperAxisToRelativePosition = 0x32;
        public const byte CONST_QueryStepperAxisPosition = 0x33;
        public const byte CONST_GetStepperAxisConfiguration = 0x34;
        public const byte CONST_SetStepperAxisConfiguration = 0x35;
        public const byte CONST_GetIllumination = 0x36;
        public const byte CONST_SetIllumination = 0x37;
        public const byte CONST_UpdateStepperAxisPosition = 0x38;
        public const byte CONST_GetDiagnosticRecord = 0xFF;

        #endregion

        #region Variables

        //Solution found on https://blog.cdemi.io/async-waiting-inside-c-sharp-locks/
        //Instantiate a Singleton of the Semaphore with a value of 1. This means that only 1 thread can be granted access at a time.
        static SemaphoreSlim semaphoreSlim = new SemaphoreSlim(1, 1);

        private readonly IClearShotWinUsbService _service;

        #endregion

        public bool IsAttached {
            get {
                return (_service != null) && _service.IsConnected;
            }
        }

        #region Events

        public event EventHandler<EventArgs> Attached;

        private void OnAttached(object sender, EventArgs e)
        {
            Attached?.Invoke(sender, e);
        }

        public event EventHandler<EventArgs> Detached;

        private void OnDetached(object sender, EventArgs e)
        {
            Detached?.Invoke(sender, e);
        }

        #endregion

        public ClearShotDevice()
        {
            _service = new ClearShotWinUsbService();
            _service.Connected += OnServiceConnected;
            _service.Disconnected += OnServiceDisconnected;
        }

        /*public ClearShotDevice(IClearShotWinUsbService service)
        {
            _service = service;
        }*/

        private void OnServiceConnected(object sender, EventArgs e)
        {
            OnAttached(sender, e);
        }

        private void OnServiceDisconnected(object sender, EventArgs e)
        {
            OnDetached(sender, e);
        }

        public async Task<KeyValuePair<ushort, ushort>> GetLastError()
        {
            //Asynchronously wait to enter the Semaphore. If no-one has been granted access to the Semaphore, code execution will proceed, otherwise this thread waits here until the semaphore is released 
            await semaphoreSlim.WaitAsync();
            try
            {

            //intiate the cmd.
            byte[] cmd = new byte[6];
            MemoryStream writer = new MemoryStream(cmd);

            //Write.
            writer.WriteByte(CONST_GetLastError); //Write cmd type.
            byte[] amountOBytes = BitConverter.GetBytes(Convert.ToUInt32(cmd.Count())).ToArray();
            writer.Write(amountOBytes, 0, amountOBytes.Count()); //Write cmd Size.
            writer.WriteByte(0x01); //Scheme.
            writer.Flush();
            writer.Dispose();

            //Now Send.
            await _service.SendCommand(cmd, nameof(CONST_GetLastError));

            //Receive result.
            byte[] result = await _service.GetResults(CONST_GetLastError, nameof(CONST_GetLastError));

            //Parse it.
            if (result.GetLength(0) > 6
                && result[0] == CONST_GetLastError
                && result[6] == 0x01) //If success.
            {
                return new KeyValuePair<ushort, ushort>(BitConverter.ToUInt16(result, 7), BitConverter.ToUInt16(result, 9));
            }

            }
            finally
            {
                //When the task is ready, release the semaphore. It is vital to ALWAYS release the semaphore when we are ready, or else we will end up with a Semaphore that is forever locked.
                //This is why it is important to do the Release within a try...finally clause; program execution may crash or take a different path, this way you are guaranteed execution
                semaphoreSlim.Release();
            }
            return new KeyValuePair<ushort, ushort>(0, 0);
        }

        public async Task<string> GetCalibration()
        {
            await semaphoreSlim.WaitAsync();
            try
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
            await _service.SendCommand(cmd, nameof(CONST_GetCalibration));

            //Receive result.
            byte[] result = await _service.GetResults(CONST_GetCalibration, nameof(CONST_GetCalibration));

            //Parse it.
            if (result.GetLength(0) > 6
                && result[0] == CONST_GetCalibration
                && result[6] == 0x01) //If success.
            {
                return Encoding.UTF8.GetString(result, 64, result.Length - 64);
                    //Return the calibration.
            }
            var lasterror = await GetLastError();
            return $"ERROR: ({lasterror.Key}) ({lasterror.Value})";

            }
            finally
            {
                semaphoreSlim.Release();
            }
            return "";
        }

        public async Task<float> GetExposureTime()
        {
            await semaphoreSlim.WaitAsync();
            try
            {

            //intiate the cmd.
            byte[] cmd = new byte[6];
            MemoryStream writer = new MemoryStream(cmd);

            //Write.
            writer.WriteByte(CONST_GetExposureTime); //Write cmd type.
            byte[] amountOBytes = BitConverter.GetBytes(Convert.ToUInt32(cmd.Count())).ToArray();
            writer.Write(amountOBytes, 0, amountOBytes.Count()); //Write cmd Size.
            writer.WriteByte(0x01); //Scheme.
            writer.Flush();
            writer.Dispose();

            //Now Send.
            await _service.SendCommand(cmd, nameof(CONST_GetExposureTime));

            //Receive result.
            byte[] result = await _service.GetResults(CONST_GetExposureTime, nameof(CONST_GetExposureTime));

            //Parse it.
            if (result.GetLength(0) > 6
                && result[0] == CONST_GetExposureTime
                && result[6] == 0x01) //If success.
            {
                return BitConverter.ToSingle(result, 7); //Return the exposure.
            }

            }
            finally
            {
                semaphoreSlim.Release();
            }
            return -1;
        }

        public async Task<Tuple<short, short, short>> GetAfeParameters()
        {
            await semaphoreSlim.WaitAsync();
            try
            {

            //intiate the cmd.
            byte[] cmd = new byte[6];
            MemoryStream writer = new MemoryStream(cmd);

            //Write.
            writer.WriteByte(CONST_GetAFEParameters); //Write cmd type.
            byte[] amountOBytes = BitConverter.GetBytes(Convert.ToUInt32(cmd.Count())).ToArray();
            writer.Write(amountOBytes, 0, amountOBytes.Count()); //Write cmd Size.
            writer.WriteByte(0x01); //Scheme.
            writer.Flush();
            writer.Dispose();

            //Now Send.
            await _service.SendCommand(cmd, nameof(CONST_GetAFEParameters));

            //Receive result.
            byte[] result = await _service.GetResults(CONST_GetAFEParameters, nameof(CONST_GetAFEParameters));

            //Parse it.
            if (result.GetLength(0) > 6
                && result[0] == CONST_GetAFEParameters
                && result[6] == 0x01) //If success.
            {
                // The value of the AFE configuration register.
                short configuration = BitConverter.ToInt16(result, 7);

                // The value of the AFE offset register.
                short offset = BitConverter.ToInt16(result, 9);

                /// The value of the AFE gain register.
                short gain = BitConverter.ToInt16(result, 11);

                return new Tuple<short, short, short>(configuration, offset, gain);
            }

            }
            finally
            {
                semaphoreSlim.Release();
            }
            return new Tuple<short, short, short>(0, 0, 0);
        }

        public async Task<bool> SetAfeParameters(short value)
        {
            await semaphoreSlim.WaitAsync();
            try
            {

            //create the cmd.
            byte[] cmd = new byte[13];
            MemoryStream writer = new MemoryStream(cmd);

            //Write the cmd.
            writer.WriteByte(CONST_SetAFEParameters);
            byte[] amountOBytes = BitConverter.GetBytes(Convert.ToUInt32(cmd.Count())).ToArray();
            writer.Write(amountOBytes, 0, amountOBytes.Count()); //Write cmd Size.
            writer.WriteByte(0x01); //Scheme.
            byte[] confBytes = BitConverter.GetBytes((short)0); // Configuration (16-bit signed integer) – not currently used
            writer.Write(confBytes, 0, confBytes.Count());
            byte[] offsetBytes = BitConverter.GetBytes((short)0); // Offset (16-bit signed integer) – Specifies the current dark current compensation (0 to 1024).
            writer.Write(offsetBytes, 0, offsetBytes.Count());
            byte[] gainBytes = BitConverter.GetBytes(value); // Gain (16-bit signed integer) – Specifies the current input sensitivity to the full scale output of the CCD (0 to 63).
            writer.Write(gainBytes, 0, gainBytes.Count());
            writer.Flush();
            writer.Dispose();

            //Now send.
            await _service.SendCommand(cmd, nameof(CONST_SetAFEParameters));

            //Recieve result.
            byte[] result = await _service.GetResults(CONST_SetAFEParameters, nameof(CONST_SetAFEParameters));
            if (result[6] == 0x01) //If success.
            {
                return true;
            }

            }
            finally
            {
                semaphoreSlim.Release();
            }
            return false;
        }

        public async Task<string> GetSerialNumber()
        {
            await semaphoreSlim.WaitAsync();
            try
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
            await _service.SendCommand(cmd, nameof(CONST_GetSerialNumber));

            //Receive result.
            byte[] result = await _service.GetResults(CONST_GetSerialNumber, nameof(CONST_GetSerialNumber));

            //Parse it.
            if (result.GetLength(0) > 6
                && result[0] == CONST_GetSerialNumber
                && result[6] == 0x01) //If success.
            {
                return Encoding.UTF8.GetString(result, 9, BitConverter.ToUInt16(result, 7) - 1 /* We dont want the end-of-string */); //Starting from 9 and until the length in (7-8)
            }
            var lasterror = await GetLastError();
            return $"ERROR: ({lasterror.Key}) ({lasterror.Value})";

            }
            finally
            {
                semaphoreSlim.Release();
            }
            return "";
        }

        public async Task<int> SetExposureTime(float value)
        {
            await semaphoreSlim.WaitAsync();
            try
            {

            //create the cmd.
            byte[] cmd = new byte[10];
            MemoryStream writer = new MemoryStream(cmd);

            //Write the cmd.
            writer.WriteByte(CONST_SetExposureTime);
            byte[] amountOBytes = BitConverter.GetBytes(Convert.ToUInt32(cmd.Count())).ToArray();
            writer.Write(amountOBytes, 0, amountOBytes.Count()); //Write cmd Size.
            writer.WriteByte(0x01); //Scheme.
            byte[] valueBytes = BitConverter.GetBytes(value);
            writer.Write(valueBytes, 0, valueBytes.Count());
            writer.Flush();
            writer.Dispose();

            //Now send.
            await _service.SendCommand(cmd, nameof(CONST_SetExposureTime));

            //Recieve result.
            byte[] result = await _service.GetResults(CONST_SetExposureTime, nameof(CONST_SetExposureTime));
            if (result.GetLength(0) > 6
                && result[0] == CONST_SetExposureTime
                && result[6] == 0x01) //If success.
            {
                return 1;
            }

            }
            finally
            {
                semaphoreSlim.Release();
            }
            return 0;
        }

        public async Task<bool> IsLaserAvailable()
        {
            await semaphoreSlim.WaitAsync();
            try
            {

            //create the cmd.
            byte[] cmd = new byte[6];
            MemoryStream writer = new MemoryStream(cmd);

            //Write the cmd.
            writer.WriteByte(CONST_GetSpectrometerInformation);
            byte[] amountOBytes = BitConverter.GetBytes(Convert.ToUInt32(cmd.Count())).ToArray();
            writer.Write(amountOBytes, 0, amountOBytes.Count()); //Write cmd Size.
            writer.WriteByte(0x01); //Scheme.
            writer.Flush();
            writer.Dispose();

            //Now send.
            await _service.SendCommand(cmd, nameof(CONST_GetSpectrometerInformation));

            //Recieve result.
            byte[] result = await _service.GetResults(CONST_GetSpectrometerInformation, nameof(CONST_GetSpectrometerInformation));
            if (result.GetLength(0) > 6
                && result[0] == CONST_GetSpectrometerInformation
                && (uint)result[29] > 0) //If success.
            {
                return true;
            }

            }
            finally
            {
                semaphoreSlim.Release();
            }
            return false;
        }

        public async Task<bool> SetLaserEnabled(byte laser, bool state)
        {
            await semaphoreSlim.WaitAsync();
            try
            {

            //create the cmd.
            byte[] cmd = new byte[8];
            MemoryStream writer = new MemoryStream(cmd);

            //Write the cmd.
            writer.WriteByte(CONST_SetLaserEnabled);
            byte[] amountOBytes = BitConverter.GetBytes(Convert.ToUInt32(cmd.Count())).ToArray();
            writer.Write(amountOBytes, 0, amountOBytes.Count()); //Write cmd Size.
            writer.WriteByte(0x01); //Scheme.
            writer.WriteByte(laser);
            writer.WriteByte(Convert.ToByte(state));
            writer.Flush();
            writer.Dispose();

            //Now send.
            await _service.SendCommand(cmd, nameof(CONST_SetLaserEnabled));

            //Recieve result.
            byte[] result = await _service.GetResults(CONST_SetLaserEnabled, nameof(CONST_SetLaserEnabled));
            if (result.GetLength(0) > 6
                && result[0] == CONST_SetLaserEnabled
                && result[6] == 0x01) //If success.
            {
                return true;
            }

            }
            finally
            {
                semaphoreSlim.Release();
            }
            return false;
        }

        public async Task<bool> GetLaserEnabled(byte laser)
        {
            await semaphoreSlim.WaitAsync();
            try
            {

            //create the cmd.
            byte[] cmd = new byte[7];
            MemoryStream writer = new MemoryStream(cmd);

            //Write the cmd.
            writer.WriteByte(CONST_GetLaserEnabled);
            byte[] amountOBytes = BitConverter.GetBytes(Convert.ToUInt32(cmd.Count())).ToArray();
            writer.Write(amountOBytes, 0, amountOBytes.Count()); //Write cmd Size.
            writer.WriteByte(0x01); //Scheme.
            writer.WriteByte(laser);
            writer.Flush();
            writer.Dispose();

            //Now send.
            await _service.SendCommand(cmd, nameof(CONST_GetLaserEnabled));

            //Recieve result.
            byte[] result = await _service.GetResults(CONST_GetLaserEnabled, nameof(CONST_GetLaserEnabled));
            if (result.GetLength(0) > 6
                && result[0] == CONST_GetLaserEnabled
                && result[6] == 0x01) //If success and enabled.
            {
                return (result[7] == 0x01);
            }

            }
            finally
            {
                semaphoreSlim.Release();
            }
            return false;
        }

        public async Task<bool> StartExposure(byte shutterState, byte exposureType)
        {
            await semaphoreSlim.WaitAsync();
            try
            {

            //create the cmd.
            byte[] cmd = new byte[8];
            MemoryStream writer = new MemoryStream(cmd);

            //Write the cmd.
            writer.WriteByte(CONST_StartExposure);
            byte[] amountOBytes = BitConverter.GetBytes(Convert.ToUInt32(cmd.Count())).ToArray();
            writer.Write(amountOBytes, 0, amountOBytes.Count()); //Write cmd Size.
            writer.WriteByte(0x01); //Scheme.
            writer.WriteByte(shutterState);
            writer.WriteByte(exposureType);
            writer.Flush();
            writer.Dispose();

            //Now send.
            await _service.SendCommand(cmd, nameof(CONST_StartExposure));

            //Recieve result.
            byte[] result = await _service.GetResults(CONST_StartExposure, nameof(CONST_StartExposure));
            if (result.GetLength(0) > 6
                && result[0] == CONST_StartExposure
                && result[6] == 0x01) //If success.
            {
                return true;
            }

            }
            finally
            {
                semaphoreSlim.Release();
            }
            return false;
        }

        public async Task<bool> QueryExposure()
        {
            await semaphoreSlim.WaitAsync();
            try
            {

            //create the cmd.
            byte[] cmd = new byte[6];
            MemoryStream writer = new MemoryStream(cmd);

            //Write the cmd.
            writer.WriteByte(CONST_QueryExposure);
            byte[] amountOBytes = BitConverter.GetBytes(Convert.ToUInt32(cmd.Count())).ToArray();
            writer.Write(amountOBytes, 0, amountOBytes.Count()); //Write cmd Size.
            writer.WriteByte(0x01); //Scheme.
            writer.Flush();
            writer.Dispose();

            //Now send.
            await _service.SendCommand(cmd, nameof(CONST_QueryExposure));

            //Recieve result.
            byte[] result = await _service.GetResults(CONST_QueryExposure, nameof(CONST_QueryExposure));
            if (result.GetLength(0) > 6
                && result[0] == CONST_QueryExposure
                && result[6] == 0x01) //If exposure available.
            {
                return result[7] == 0x01;
            }

            }
            finally
            {
                semaphoreSlim.Release();
            }
            return false;
        }

        public async Task<bool> EndExposure(byte shutterState)
        {
            await semaphoreSlim.WaitAsync();
            try
            {

            //create the cmd.
            byte[] cmd = new byte[7];
            MemoryStream writer = new MemoryStream(cmd);

            //Write the cmd.
            writer.WriteByte(CONST_EndExposure);
            byte[] amountOBytes = BitConverter.GetBytes(Convert.ToUInt32(cmd.Count())).ToArray();
            writer.Write(amountOBytes, 0, amountOBytes.Count()); //Write cmd Size.
            writer.WriteByte(0x01); //Scheme.
            writer.WriteByte(shutterState);
            writer.Flush();
            writer.Dispose();

            //Now send.
            await _service.SendCommand(cmd, nameof(CONST_EndExposure));

            //Recieve result.
            byte[] result = await _service.GetResults(CONST_EndExposure, nameof(CONST_EndExposure));
            if (result.GetLength(0) > 6
                && result[0] == CONST_EndExposure
                && result[6] == 0x01) //If correct.
            {
                return true;
            }

            }
            finally
            {
                semaphoreSlim.Release();
            }
            return false;
        }

        public async Task<int[]> GetExposureWidthHeightBits()
        {
            await semaphoreSlim.WaitAsync();
            try
            {

            //create the cmd.
            byte[] cmd = new byte[6];
            MemoryStream writer = new MemoryStream(cmd);

            //Write the cmd.
            writer.WriteByte(CONST_GetSpectrometerInformation);
            byte[] amountOBytes = BitConverter.GetBytes(Convert.ToUInt32(cmd.Count())).ToArray();
            writer.Write(amountOBytes, 0, amountOBytes.Count()); //Write cmd Size.
            writer.WriteByte(0x01); //Scheme.
            writer.Flush();
            writer.Dispose();

            //Now send.
            await _service.SendCommand(cmd, nameof(CONST_GetSpectrometerInformation));

            //Recieve result.
            byte[] result = await _service.GetResults(CONST_GetSpectrometerInformation, nameof(CONST_GetSpectrometerInformation));
            if (result.GetLength(0) > 9
                && result[0] == CONST_GetSpectrometerInformation
                && result[6] == 0x01) //If success.
            {
                int width = BitConverter.ToUInt16(result, 7);
                int height = BitConverter.ToUInt16(result, 9);
                return new[] { height, width };
            }

            }
            finally
            {
                semaphoreSlim.Release();
            }
            return null;
        }

        /*
                    //get size.
                    int[] dims = await GetExposureWidthHeightBits();
                    if (dims != null)
                    {
                        int height = dims[0];
                        int width = dims[1];
         */
        public async Task<ushort[]> GetExposure(int height, int width)
        {
            await semaphoreSlim.WaitAsync();
            try
            {

            //create the cmd.
            byte[] cmd = new byte[6];
            MemoryStream writer = new MemoryStream(cmd);

            //Write the cmd.
            writer.WriteByte(CONST_GetExposure);
            byte[] amountOBytes = BitConverter.GetBytes(Convert.ToUInt32(cmd.Count())).ToArray();
            writer.Write(amountOBytes, 0, amountOBytes.Count()); //Write cmd Size.
            writer.WriteByte(0x01); //Scheme.
            writer.Flush();
            writer.Dispose();

            //Now send.
            await _service.SendCommand(cmd, nameof(CONST_GetExposure));

            //Recieve result.
            byte[] result = await _service.GetResults(CONST_GetExposure, nameof(CONST_GetExposure), GET_IMAGE_RESULT_INPOINT); //Use inpoint 0 for image data.

            //Make sure it successded.
            if (result.GetLength(0) > 6
                && result[0] == CONST_GetExposure
                && result[6] == 0x01)
            {
                uint imageSize = BitConverter.ToUInt32(result, 8);
                if (imageSize > 0)
                {
                    //Extract the image into an array by itself.
                    int offset = 64;


                        //The array to be returned.
                        int imageLen = height * width;
                        ushort[] imageData = new ushort[imageLen];
                        int length = result.Length - offset;
                        if (length > imageLen * sizeof(ushort)) length = imageLen * sizeof(ushort);
                        //Array.Copy(result, offset, imageData, 0, 10); // Wrong method.
                        // TODO Replace by more efficient copy.
                        for (int i = 0; i < imageLen; i++)
                            imageData[i] = BitConverter.ToUInt16(result, (int)(offset + i * 2));
                        //Buffer.BlockCopy(result, offset, imageData, 0, length);
                        //loop and fill new array
                        //for (int i = 0; i < height; i++)
                        //{
                        //    for (int j = 0; j < width; j++)
                        //    {
                        //        imageData[i, j] = BitConverter.ToUInt16(result, offset);
                        //        offset += 2;
                        //    }
                        //}

                        //Return.
                        return imageData;
                }
                return null;
            }

            }
            finally
            {
                semaphoreSlim.Release();
            }
            return null;
        }

        public async Task<string> GetModelNumber()
        {
            await semaphoreSlim.WaitAsync();
            try
            {

            //intiate the cmd.
            byte[] cmd = new byte[6];
            MemoryStream writer = new MemoryStream(cmd);

            //Write.
            writer.WriteByte(CONST_GetModelNumber); //Write cmd type.
            byte[] amountOBytes = BitConverter.GetBytes(Convert.ToUInt32(cmd.Count())).ToArray();
            writer.Write(amountOBytes, 0, amountOBytes.Count()); //Write cmd Size.
            writer.WriteByte(0x01); //Scheme.
            writer.Flush();
            writer.Dispose();

            //Now Send.
            await _service.SendCommand(cmd, nameof(CONST_GetModelNumber));

            //Receive result.
            byte[] result = await _service.GetResults(CONST_GetModelNumber, nameof(CONST_GetModelNumber));

            //Parse it.
            if (result.GetLength(0) > 6
                && result[0] == CONST_GetModelNumber
                && result[6] == 0x01) //If success.
            {
                return Encoding.UTF8.GetString(result, 9, BitConverter.ToUInt16(result, 7) - 1 /* We dont want the end-of-string */); //Starting from 9 and until the length in (7-8)
            }
            var lasterror = await GetLastError();
            return $"ERROR: ({lasterror.Key}) ({lasterror.Value})";

            }
            finally
            {
                semaphoreSlim.Release();
            }
            return "";
        }

        public async Task<float> GetCCDTemperature()
        {
            await semaphoreSlim.WaitAsync();
            try
            {

            //intiate the cmd.
            byte[] cmd = new byte[6];
            MemoryStream writer = new MemoryStream(cmd);

            //Write.
            writer.WriteByte(CONST_GetCCDTemperatureInfo); //Write cmd type.
            byte[] amountOBytes = BitConverter.GetBytes(Convert.ToUInt32(cmd.Count())).ToArray();
            writer.Write(amountOBytes, 0, amountOBytes.Count()); //Write cmd Size.
            writer.WriteByte(0x01); //Scheme.
            writer.Flush();
            writer.Dispose();

            //Now Send.
            await _service.SendCommand(cmd, nameof(CONST_GetCCDTemperatureInfo));

            //Receive result.
            byte[] result = await _service.GetResults(CONST_GetCCDTemperatureInfo, nameof(CONST_GetCCDTemperatureInfo));

            //Parse it.
            if (result.GetLength(0) > 6 
                && result[0] == CONST_GetCCDTemperatureInfo
                && result[6] == 0x01) //If success.
            {
                // Indicates is m_LaserTemperature regulation is m_bEnabled or disabled.
                bool temperatureRegulation = BitConverter.ToBoolean(result, 7);

                // The last configured or default CCD m_LaserTemperature set point in degrees C.
                float ccdSetpoint = BitConverter.ToSingle(result, 8);

                // The current m_LaserTemperature of the CCD in degrees C.
                float ccdThermistor = BitConverter.ToSingle(result, 12);

                // Indicates an Open or Short-Circuit Condition from the CCD Thermistor.
                bool ccdThermFault = BitConverter.ToBoolean(result, 16);

                // Indicates if the CCD Thermistor m_LaserTemperature is within +/- 0.1 degrees C of CCD set point.
                bool ccdTempLock = BitConverter.ToBoolean(result, 17);

                // The temperature is really a signed 8-bit value, but is converted to a float as unsigned.  Fix it here.
                if (ccdThermistor >= 128) ccdThermistor -= 256;

                return ccdThermistor;
            }

            }
            finally
            {
                semaphoreSlim.Release();
            }
            return -1;
        }
    }
}
