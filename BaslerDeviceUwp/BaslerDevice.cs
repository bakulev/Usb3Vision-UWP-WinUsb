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

        public int ImageHeight => throw new NotImplementedException();

        public int ImageWidth => throw new NotImplementedException();

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
                    else if (interf.InterfaceNumber == 2)
                    {
                        _cameraHelper.StreamInPipe = interf.BulkInPipes[0];
                    }
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
            var sirm = await _cameraHelper.GetRegisterValueAsync(sbrm + 0x00020);

            //Get leader size.
            var leaderSize = await _cameraHelper.GetBlocksSizeAsync(sirm + 0x10);

            //Get trailer size
            var trailerSize = await _cameraHelper.GetBlocksSizeAsync(sirm + 0x14);

            var pSize = await _cameraHelper.GetBlocksSizeAsync(sirm + 0x08);

            //Setup Stream Configuration
            //Set max transfer size for the cam
            if (await _cameraHelper.SetConfigRegisterAsync(sirm + 0x1C, 1024) != 0)
                return null;

            var p1Size = await _cameraHelper.GetBlocksSizeAsync(sirm + 0x1C);


            //Set number of transfers of data per frame.
            if (await _cameraHelper.SetConfigRegisterAsync(sirm + 0x20, pSize / 1024) != 0)
                return null;

            var ntrans = await _cameraHelper.GetBlocksSizeAsync(sirm + 0x20);


            //Set max leader and trailing buffer
            //Set number of transfers of data per frame.
            if (await _cameraHelper.SetConfigRegisterAsync(sirm + 0x18, 65536) != 0)
                return null;

            var rez = await _cameraHelper.SetConfigRegisterAsync(0x40004, 0);


            Debug.WriteLine($"Acuisition status before: {await _cameraHelper.GetRegisterValueAsync(0x40608)}");

            rez = await _cameraHelper.SetConfigRegisterAsync(262468, 1);
            rez = await _cameraHelper.SetConfigRegisterAsync(262404, 1);
            rez = await _cameraHelper.SetConfigRegisterAsync(1278500, 36);
            rez = await _cameraHelper.SetConfigRegisterAsync(263172, 1);
            rez = await _cameraHelper.SetConfigRegisterAsync(263268, 1000);

            //acuisition start
            rez = await _cameraHelper.SetConfigRegisterAsync(0x40024, 1);

            //trigger event ???
            rez = await _cameraHelper.SetConfigRegisterAsync(262436, 1);

            Debug.WriteLine($"Acuisition status after: {await _cameraHelper.GetRegisterValueAsync(0x40608)}");

            //var data = await GetImageData();

            Thread.Sleep(1000);

            //acuisition stop
            rez = await _cameraHelper.SetConfigRegisterAsync(0x40044, 1);


            Thread.Sleep(20000);
            return null;
        }
        #endregion
    }
}
