using CodaDevices.Devices.BaslerWinUsb.Helpers;
using CodaDevices.Devices.BaslerWinUsb.USB3VisionTypes;
using System;
using System.Threading;
using System.Threading.Tasks;
using Windows.Devices.Enumeration;
using Windows.Devices.Usb;

namespace CodaDevices.Devices.BaslerWinUsb
{
    public class BaslerDevice : IDevice
    {
        #region Constructors
        public BaslerDevice()
        {
            //Create watcher to watch for device connection.
            string aqs = UsbDevice.GetDeviceSelector(VendorId, ProductId);
            var superMuttWatcher = DeviceInformation.CreateWatcher(aqs);
            superMuttWatcher.Added += this.OnDeviceAdded;
            superMuttWatcher.Removed += this.OnDeviceRemoved;

            _cameraHelper = new CameraInterchangeHelper();

            superMuttWatcher.Start();
        }
        #endregion

        #region Fields
        private UsbDevice _targetDevice;
        CameraInterchangeHelper _cameraHelper;

        const UInt32 VendorId = 0x2676;
        const UInt32 ProductId = 0xBA02;
        #endregion

        #region Properties
        public string Name => throw new NotImplementedException();

        public string ModelNumber => throw new NotImplementedException();

        public string SerialNumber => throw new NotImplementedException();

        public int ImageHeight { get; private set; }

        public int ImageWidth { get; private set; }

        public bool IsAttached { get; private set; }

        public bool IsLaserEnabled => throw new NotImplementedException();
        #endregion

        #region Events
        public event EventHandler<EventArgs> Attached;
        public event EventHandler<EventArgs> Detached;
        public event EventHandler<EventArgs> LaserDisabled;
        public event EventHandler<EventArgs> LaserEnabled;
        #endregion

        #region Methods

        private async void OnDeviceAdded(DeviceWatcher watcher, DeviceInformation deviceInformation)
        {
            if (deviceInformation.IsEnabled)
            {
                _targetDevice = await UsbDevice.FromIdAsync(deviceInformation.Id);
                foreach (var interf in _targetDevice.Configuration.UsbInterfaces)
                {
                    if (interf.InterfaceNumber == 0)
                    {
                        _cameraHelper.ControlInPipe = interf.BulkInPipes[0];
                        _cameraHelper.ControlOutPipe = interf.BulkOutPipes[0];
                    }
                    else if (interf.InterfaceNumber == 1)
                    {
                        _cameraHelper.StreamInPipe = interf.BulkInPipes[0];
                    }
                    //else if (interf.InterfaceNumber == 2)
                    //{
                    //    _cameraHelper.StreamInPipe = interf.BulkInPipes[0];
                    //}
                }

                IsAttached = true;
                Attached?.Invoke(this, EventArgs.Empty);
            }
        }

        private void OnDeviceRemoved(DeviceWatcher watcher, DeviceInformationUpdate deviceInformation)
        {
            _targetDevice = null;
            IsAttached = false;
            Detached?.Invoke(this, EventArgs.Empty);
        }

        public Task<bool> GetInterlockState()
        {
            throw new NotImplementedException();
        }

        public Task<float> GetLaserTemperature()
        {
            throw new NotImplementedException();
        }

        public Task<bool> LaserTurnOff(CancellationToken ct)
        {
            throw new NotImplementedException();
        }

        public Task<bool> LaserTurnOn(CancellationToken ct)
        {
            throw new NotImplementedException();
        }

        public Task<bool> SetExposure(float exposureTime, CancellationToken ct)
        {
            throw new NotImplementedException();
        }



        private async Task CorrectGainAsync(TakeParams acquireParams)
        {
            //lower gain limit
            var P1 = (int)acquireParams.MinGain;

            //high gain limit         
            var P2 = acquireParams.MaxGain;

            var gain = (P1 == 226) ? (2048 - 2048 * Math.Exp(-acquireParams.AnalogGain * Math.Log(10) / 20)) : (acquireParams.AnalogGain * 10 + P2);

            var rez = await _cameraHelper.SetConfigRegisterAsync(131108, (int)gain);
            gain = await _cameraHelper.GetBlocksSizeAsync(131108);

        }

        public async Task<ushort[,]> TakeImage(
            TakeParams acquireParams, CancellationToken ct, IProgress<TakeProgressEventArgs> progress = null)
        {

            //Pixel format
            var rez = await _cameraHelper.SetConfigRegisterAsync(196644, 5);

            //set gain
            await CorrectGainAsync(acquireParams);

            //Get SBRM register
            var sbrm = await _cameraHelper.GetRegisterValueAsync(0x001D8);

            //Read SIRM register
            var sirm = await _cameraHelper.GetRegisterValueAsync(sbrm + 0x0020);

            //Get leader size.
            var leaderSize = await _cameraHelper.GetBlocksSizeAsync(sirm + 0x10);

            //Get trailer size
            var trailerSize = await _cameraHelper.GetBlocksSizeAsync(sirm + 0x14);

            var width = await _cameraHelper.GetBlocksSizeAsync(197124);
            var height = await _cameraHelper.GetBlocksSizeAsync(197156);

            ///
            rez = await _cameraHelper.SetConfigRegisterAsync(0x40104, 0);
            rez = await _cameraHelper.SetConfigRegisterAsync(0x40204, 0);
            rez = await _cameraHelper.SetConfigRegisterAsync(0x40004, 0);
            rez = await _cameraHelper.SetConfigRegisterAsync(sirm + 0x0004, 0);
            rez = await _cameraHelper.SetConfigRegisterAsync(sirm + 0x001C, width * height);
            rez = await _cameraHelper.SetConfigRegisterAsync(sirm + 0x0020, 4); 
            rez = await _cameraHelper.SetConfigRegisterAsync(sirm + 0x0024, 206848);
            rez = await _cameraHelper.SetConfigRegisterAsync(sirm + 0x0028, 0);
            rez = await _cameraHelper.SetConfigRegisterAsync(sirm + 0x0018, leaderSize);
            rez = await _cameraHelper.SetConfigRegisterAsync(sirm + 0x002c, trailerSize);
            rez = await _cameraHelper.SetConfigRegisterAsync(0x40024, 0x01);

            //exposure mode
            //EnumEntry_ExposureMode_Timed = 1
            //EnumEntry_ExposureMode_TriggerWidth = 2
            rez = await _cameraHelper.SetConfigRegisterAsync(263172, 1);

            //Exposure time
            rez = await _cameraHelper.SetConfigRegisterAsync(263268, (int)acquireParams.ExposureTime);

            rez = await _cameraHelper.SetConfigRegisterAsync(sirm + 0x0004, 1);
            rez = await _cameraHelper.SetConfigRegisterAsync(0x40024, 0x01);

            byte[] data = new byte[0];
            var leader = await _cameraHelper.GetImageData();

            while (data.Length < width * height * 2)
                data = ArrayHelper.ConcatArrays(data, await _cameraHelper.GetImageData());
            var trailer = await _cameraHelper.GetImageData();

            rez = await _cameraHelper.SetConfigRegisterAsync(0x40044, 0x01);

            ImageHeight = height; ImageWidth = width;
            return ArrayHelper.UnpackImage(data, width, height);
        }

        public async Task<bool> GetEnabled(ushort Laser)
        {
            return await _cameraHelper.GetBlocksSizeAsync(0xc0248) == 1;
        }

        public async Task SetLaserState(ushort Laser, bool Enabled)
        {
            var rez = await _cameraHelper.SetConfigRegisterAsync(0xc0264, 0x1c);
            rez &= await _cameraHelper.SetConfigRegisterAsync(0xc02e4, Enabled ? 1 : 0);
            if (rez != 0)
                throw new Exception("Can't set laser state");
        }
        #endregion
    }
}
