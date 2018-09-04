using BaslerDeviceUwp.Helpers;
using BaslerDeviceUwp.USB3VisionTypes;
using Centice.Spectrometry.Base;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Windows.Devices.Enumeration;
using Windows.Devices.Usb;

namespace BaslerDeviceUwp
{
    public class BaslerDevice : IImageDevice
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

        public int ImageHeight => GetImageHeigth();

        public int ImageWidth => GetImageWidth();

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

        private int GetImageHeigth()
        {
            return 1200;
        }

        private int GetImageWidth()
        {
            return 1920;
        }

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

        public async Task<ushort[,]> TakeImage(
            AcquireParams acquireParams, CancellationToken ct, IProgress<CameraProgressEventArgs> progress = null)
        {
            //Get SBRM register
            var sbrm = await _cameraHelper.GetRegisterValueAsync(0x001D8);

            //Read SIRM register
            var sirm = await _cameraHelper.GetRegisterValueAsync(sbrm + 0x0020);

            //Get leader size.
            var leaderSize = await _cameraHelper.GetBlocksSizeAsync(sirm + 0x10);

            //Get trailer size
            var trailerSize = await _cameraHelper.GetBlocksSizeAsync(sirm + 0x14);

            var width = 1920;//GetImageWidth(); 
            var height = 1200;// GetImageHeigth();
            ///
            var rez = await _cameraHelper.SetConfigRegisterAsync(0x40104, 0);
            rez = await _cameraHelper.SetConfigRegisterAsync(0x40204, 0);
            rez = await _cameraHelper.SetConfigRegisterAsync(0x40004, 0);
            rez = await _cameraHelper.SetConfigRegisterAsync(sirm + 0x0004, 0);
            rez = await _cameraHelper.SetConfigRegisterAsync(sirm + 0x001C, width * height / 2 );
            rez = await _cameraHelper.SetConfigRegisterAsync(sirm + 0x0020, 2); 
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

            //Pixel format
            //rez = await _cameraHelper.SetConfigRegisterAsync(196644, 0x01080001);

            rez = await _cameraHelper.SetConfigRegisterAsync(sirm + 0x0004, 1);
            rez = await _cameraHelper.SetConfigRegisterAsync(0x40024, 0x01);

            byte[] data = new byte[0];
            var leader = await _cameraHelper.GetImageData();
            for (int i = 0; i < 2; i++)
                data = ArrayHelper.ConcatArrays(data, await _cameraHelper.GetImageData());
            var trailer = await _cameraHelper.GetImageData();

            rez = await _cameraHelper.SetConfigRegisterAsync(0x40044, 0x01);

            return ArrayHelper.UnpackImage(data, width, height);
        }
        #endregion
    }
}
