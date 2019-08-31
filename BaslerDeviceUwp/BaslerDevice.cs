using CodaDevices.Devices.BaslerWinUsb.Helpers;
using CodaDevices.Devices.BaslerWinUsb.USB3VisionTypes;
using CodaDevices.Spectrometry.Model;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.Devices.Enumeration;
using Windows.Devices.Usb;
using Windows.Storage;

namespace CodaDevices.Devices.BaslerWinUsb
{
    
    public class BaslerDevice : IBaslerCameraDriver
    {
        #region Constructors
        public BaslerDevice()
        {
            //Create watcher to watch for device connection.

            //var superMuttWatcher = DeviceInformation.CreateWatcher(aqs);
            //superMuttWatcher.Added += this.OnDeviceAdded;
            //superMuttWatcher.Removed += this.OnDeviceRemoved;
            new Task(async () => await CheckDeviceAsync()).Start();
            _cameraHelper = new CameraInterchangeHelper();

            //superMuttWatcher.Start();
        }
        #endregion

        #region Fields
        private UsbDevice _targetDevice;
        CameraInterchangeHelper _cameraHelper;
        private bool _close;
        private bool _isAttached;

        //product info
        const UInt32 VendorId = 0x2676;
        const UInt32 ProductId = 0xBA02;
        #endregion

        #region Properties
        public string Name => throw new NotImplementedException();

        public string ModelNumber => throw new NotImplementedException();

        public string SerialNumber => throw new NotImplementedException();

        public int ImageHeight { get; private set; }

        public int ImageWidth { get; private set; }

        public bool IsAttached 
        {
            get { return _isAttached; }
            private set
            {
                if (_isAttached == value)
                    return;
                _isAttached = value;
                if (_isAttached)
                {
                    Attached?.Invoke(this, EventArgs.Empty);
                }
                else
                    Detached?.Invoke(this, EventArgs.Empty);
            }
        }

        public bool IsLaserEnabled => throw new NotImplementedException();
        #endregion

        #region Events
        public event EventHandler<EventArgs> Attached;
        public event EventHandler<EventArgs> Detached;
        public event EventHandler<EventArgs> LaserDisabled;
        public event EventHandler<EventArgs> LaserEnabled;
        #endregion

        #region Methods

        private async Task CheckDeviceAsync()
        {
            while (!_close)
            {
                Thread.Sleep(10000);
                var aqs = UsbDevice.GetDeviceSelector(VendorId, ProductId);
                var myDevices = await DeviceInformation.FindAllAsync(aqs);
                if (myDevices.Count == 0)
                {
                    IsAttached = false;
                    continue;
                }
                else
                {
                    if (IsAttached)
                        continue;

                    IsAttached = true;

                    _targetDevice = await UsbDevice.FromIdAsync(myDevices[0].Id);
                    if (_targetDevice != null)
                        GetInterfaces();

                    NotifyDeviceAvailabilityObservers();
                }
            }
        }


        private void GetInterfaces()
        {
            if (_targetDevice == null) return;
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
            }
        }

        public async Task Reset()
        {
            var aqs = UsbDevice.GetDeviceSelector(VendorId, ProductId);
            var myDevices = await DeviceInformation.FindAllAsync(aqs);
            if (myDevices.Count > 0)
            {
                _targetDevice = await UsbDevice.FromIdAsync(myDevices[0].Id);
                if (_targetDevice != null)
                    GetInterfaces();
            }
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

        public async Task<bool> SetExposure(float exposureTime, CancellationToken ct)
        {
            int exposure = (int)(exposureTime * 1000000);
            //Exposure time, registers from props.txt
            var rez = await _cameraHelper.SetConfigRegisterAsync(263268, exposure);
            return rez > 0;
        }

        private async Task CorrectGainAsync(TakeParams acquireParams)
        {
            //lower gain limit
            var P1 = (int)acquireParams.MinGain;

            //high gain limit         
            var P2 = acquireParams.MaxGain;

            //from props.txt
            var gain = (P1 == 226) ? (2048 - 2048 * Math.Exp(-acquireParams.AnalogGain * Math.Log(10) / 20)) : (acquireParams.AnalogGain * 10 + P2);

            //set gain, 131108 register from props.txt
            var rez = await _cameraHelper.SetConfigRegisterAsync(131108, (int)gain);
            gain = await _cameraHelper.GetBlocksSizeAsync(131108);

        }

        public async Task<ushort[,]> TakeImage(
            TakeParams acquireParams, CancellationToken ct, IProgress<TakeProgressEventArgs> progress = null)
        {

            if (!IsAttached)
                throw new Exception("The camera has been disconnected");

            try
            {
                bool success = false;
                do
                {
                    try
                    {
                        //Pixel format
                        var r1 = await _cameraHelper.SetConfigRegisterAsync(0x30024, 5);
                        success = true;
                    }
                    catch (Exception e)
                    {
                        Debug.WriteLine(e.Message);
                        throw;
                    }
                } while (!success);

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

                //image width and height, registers from props.txt
                var width = await _cameraHelper.GetBlocksSizeAsync(0x30204);
                var height = await _cameraHelper.GetBlocksSizeAsync(0x30224);

                //trigger_mode_off (on = 1), registers from props.txt
                var rez = await _cameraHelper.SetConfigRegisterAsync(0x40104, 0);

                //unknown register from WireShark dump
                rez = await _cameraHelper.SetConfigRegisterAsync(0x40204, 0);

                //acquisition mode, 0-single, 2-continue
                rez = await _cameraHelper.SetConfigRegisterAsync(0x40004, 0);

                //disable exposure
                rez = await _cameraHelper.SetConfigRegisterAsync(sirm + 0x0004, 0);

                //single payload size
                rez = await _cameraHelper.SetConfigRegisterAsync(sirm + 0x001C, width * height);

                //count of payload transmitions
                rez = await _cameraHelper.SetConfigRegisterAsync(sirm + 0x0020, 4);

                //max final payload transfer 1 size (value from WireShark dump)
                rez = await _cameraHelper.SetConfigRegisterAsync(sirm + 0x0024, 206848);
                //max final payload transfer 1 size (value from WireShark dump)
                rez = await _cameraHelper.SetConfigRegisterAsync(sirm + 0x0028, 0);

                //leader and trailer sizes
                rez = await _cameraHelper.SetConfigRegisterAsync(sirm + 0x0018, leaderSize);
                rez = await _cameraHelper.SetConfigRegisterAsync(sirm + 0x002c, trailerSize);

                //exposure mode,registers from props.txt
                //EnumEntry_ExposureMode_Timed = 1
                //EnumEntry_ExposureMode_TriggerWidth = 2
                rez = await _cameraHelper.SetConfigRegisterAsync(263172, 1);

                //Exposure time, registers from props.txt
                rez = await _cameraHelper.SetConfigRegisterAsync(263268, (int)(acquireParams.ExposureTime * 1000000));

                await SetLaserState(0, acquireParams.ExposureType);

                //enable exposure
                rez = await _cameraHelper.SetConfigRegisterAsync(sirm + 0x0004, 1);

                //start exposure
                rez = await _cameraHelper.SetConfigRegisterAsync(0x40024, 0x01);

                byte[] data = new byte[0];
                var leader = await _cameraHelper.GetImageData();

                while (data.Length < width * height * 2)
                {
                    bool sucess = false;
                    do
                    {
                        try
                        {
                            data = ArrayHelper.ConcatArrays(data, await _cameraHelper.GetImageData());
                            success = true;
                        }
                        catch (Exception e)
                        {
                            Debug.WriteLine(e.Message);
                            throw;
                        }
                    } while (!success);
                }
                var trailer = await _cameraHelper.GetImageData();

                //stop exposure
                rez = await _cameraHelper.SetConfigRegisterAsync(0x40044, 0x01);

                await SetLaserState(0, false);

                ImageHeight = height; ImageWidth = width;
                //return ArrayHelper.UnpackImage(data, width, height);

                var imageU = new ushort[data.Length / 2];
                // Copy single image.
                Buffer.BlockCopy(data, 0, imageU, 0, data.Length);
                // Average and scale.
                for (var i = 0; i < imageU.Length; ++i)
                    if (imageU[i] * 16 <= ushort.MaxValue)
                        imageU[i] = (ushort)(imageU[i] * 16);
                    else
                        imageU[i] = ushort.MaxValue;
                // Prepare visual informaition.
                FindMinMax(imageU);
                InsertHistogram(imageU, width, height);
                InsertSection(imageU, width, height, width * 3 / 8);

                var result = new ushort[height, width];
                var offset = 0;
                for (var i = 0; i < height; ++i)
                    for (var j = 0; j < width; ++j)
                    {
                        result[i, j] = imageU[offset++];
                    }
                return result;
            }
            catch (ArgumentException)
            {
                OnDisconnect();
                return null;
            }
        }

        public async Task<bool> GetEnabled(ushort Laser)
        {
            try
            {
                //check laser, registers from WireShark dump
                return await _cameraHelper.GetBlocksSizeAsync(0xc0248) == 1;
            }
            catch (ArgumentException)
            {
                OnDisconnect();
                return false;
            }
        }

        public async Task SetLaserState(ushort Laser, bool Enabled)
        {
            try
            {
                //set gpio config, registers from WireShark dump
                var rez = await _cameraHelper.SetConfigRegisterAsync(0xc0264, 0x1c);

                //enable/disable laser, registers from WireShark dump
                rez &= await _cameraHelper.SetConfigRegisterAsync(0xc02e4, Enabled ? 1 : 0);
                if (rez != 0)
                    throw new Exception("Can't set laser state");
            }catch(ArgumentException)
            {
                OnDisconnect();
            }
        }

        private void OnDisconnect()
        {
            IsAttached = false;
            NotifyDeviceAvailabilityObservers();
        }
        #endregion

        public async Task<string> GetCalibration()
        {
            string calibrationStr = "";

            var fileName = "Calibration.xml";

            var localFolder = ApplicationData.Current.LocalFolder;
            var item = await localFolder.TryGetItemAsync(fileName);

            if (item == null)
            {
                var installedLocation = Package.Current.InstalledLocation;
                var hasSrc = await installedLocation.TryGetItemAsync($"Assets\\Data\\{fileName}");
                if (hasSrc != null)
                {
                    var srcFile = await installedLocation.GetFileAsync($"Assets\\Data\\{fileName}");
                    await srcFile.CopyAsync(ApplicationData.Current.LocalFolder);
                }
            }

            var file = await localFolder.GetFileAsync(fileName);
            using (var inputStream = await file.OpenReadAsync())
            using (var classicStream = inputStream.AsStreamForRead())
            using (var streamReader = new StreamReader(classicStream))
            {
                calibrationStr = await streamReader.ReadToEndAsync();
            }

            return calibrationStr;
        }

        private List<IDeviceAvailabilityObserver> deviceAvailabilityObserverList = new List<IDeviceAvailabilityObserver>();


        public bool GetDeviceAvailability()
        {
            //Debug.WriteLine("GetDeviceAvailability");
            return true; // for temporary use, for simulation without real device
        }

        public void RegisterDeviceAvailabilityObserver(IDeviceAvailabilityObserver observer)
        {
            //Debug.WriteLine("RegisterDeviceAvailabilityObserver");
            deviceAvailabilityObserverList.Add(observer);
        }

        public void UnregisterDeviceAvailabilityObserver(IDeviceAvailabilityObserver observer)
        {
            //Debug.WriteLine("UnregisterDeviceAvailabilityObserver");
            deviceAvailabilityObserverList.Remove(observer);
        }

        private void NotifyDeviceAvailabilityObservers()
        {
            //Debug.WriteLine("NotifyDeviceAvailabilityObservers");
            foreach (IDeviceAvailabilityObserver observer in deviceAvailabilityObserverList)
            {
                observer.OnDeviceAvailabilityChanged(IsAttached);
            }
        }

        private void FindMinMax(ushort[] img)
        {
            var min = ushort.MaxValue;
            var max = ushort.MinValue;
            for (int i = 0; i < img.Length - 1; i++)
            {
                if (min > img[i]) min = img[i];
                if (max < img[i]) max = img[i];
            }
        }

        private void InsertHistogram(ushort[] img, int width, int height)
        {
            var histLen = 65;
            var hist = new ushort[histLen];
            // count hist values
            for (int i = 0; i < img.Length; i++)
                    hist[img[i] * (histLen - 1) / ushort.MaxValue]++;
            // fill hist
            for (int i = 0; i < hist.Length; i++)
                hist[i] = (ushort)(hist[i] * width / 10000);
            for (int i = 0; i < hist.Length; i++)
                for (uint x = 0; x < hist[i] && x < width; x++)
                    img[width * i + x] = ushort.MaxValue;
        }

        private void InsertSection(ushort[] img, int width, int height, int x)
        {
            var sectLen = 100;
            var sect = new ushort[sectLen];
            // Make section by max values. Skip first setion because there is histogram there.
            var start = 65 * sectLen / height;
            for (int s = start; s < sect.Length; s++)
                for (int i = height * s / sectLen;
                    i < height * (s + 1) / sectLen; i++)
                    if (sect[s] < img[(width * i) + x])
                        sect[s] = img[(width * i) + x];
            // Scale
            for (int s = 0; s < sect.Length; s++)
                sect[s] = (ushort)(sect[s] * width / ushort.MaxValue);
            // Draw x line
            for (int s = 0; s < sect.Length; s++)
                img[width * (height - sect.Length + s) + x] =
                    ushort.MaxValue;
            // Draw section
            for (int s = 0; s < sect.Length; s++)
                for (uint w = 0; w < sect[s] && w < width; w++)
                    img[width * (height - sect.Length + s) + w] =
                        ushort.MaxValue;
        }
    }
}
