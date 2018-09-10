//#define  TimeTests

using System;
using System.Threading;
using System.Text;
using System.IO;

//using Centice.Spectrometry.Base;

//using Centice.PASS.CommonLibrary.Utility;
using System.Threading.Tasks;
using System.Collections.Generic;
using Centice.Spectrometry.Base;

namespace CodaDevices.Devices.ImageFile
{

    public class ImageFileSource : ICamera
    {
        #region Variables

        IUserInterface _ui;

        IDevice _device;

        List<Task> _pendingTasks = new List<Task>();

        #endregion

        #region Fields

        private string _name;
        //////////////////////////////////////////////////////////////////////////		
        /// <summary>
        /// Fetches the name of the camera.
        /// </summary>
        public string Name { get { return _name; } }

        private string _modelNumber;
        //////////////////////////////////////////////////////////////////////////		
        /// <summary>
        /// Gets/sets the camera's model number.
        /// </summary>
        public string ModelNumber { get { return _modelNumber; } }

        private string _serialNumber;
        //////////////////////////////////////////////////////////////////////////		
        /// <summary>
        /// Gets/Sets the camera's serial number.
        /// </summary>
        public string SerialNumber { get { return _serialNumber; } }

        private int _imageHeight = 520;
        //////////////////////////////////////////////////////////////////////////		
        /// <summary>
        /// Gets/Sets the camera's CCD image size.
        /// </summary>
        public int ImageHeight { get { return _imageHeight; } }

        private int _imageWidth = 784;
        public int ImageWidth { get { return _imageWidth; } }

        bool _isAttached;
        //////////////////////////////////////////////////////////////////////////		
        /// <summary>
        /// Indicates if the camera has an open connection.
        /// </summary>
        public bool IsAttached { get { return _isAttached; } }

        #endregion

        #region Events

        public event EventHandler<EventArgs> Attached;

        public event EventHandler<EventArgs> Detached;

        #endregion

        #region Public Constructor

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Await.Warning", "CS4014:Await.Warning")]
        public ImageFileSource(IDevice device)
        {
            _device = device;
            _device.Attached += OnDeviceAttached;
            _device.Detached += OnDeviceDetached;

            _name = _device.Name + device.SerialNumber + "Camera";
            _modelNumber = _device.ModelNumber;
            _serialNumber = _device.SerialNumber;
            _imageHeight = _device.ImageHeight;
            _imageWidth = _device.ImageWidth;
            _isAttached = false;
            //this.AnalogOffset.Value = 0;
            //this.AnalogGain.Value = 1;
            //this.AnalogConfiguration.Value = 8;
            //this.CCDBitsPerPixel = 16;

            // If device already connected then OnConnected should be fired immediately.
            if (_device.IsAttached)
                OnDeviceAttached(this, EventArgs.Empty);
        }

        #endregion // Public Constructor

        #region ICamera

        ///////////////////////////////////////////////////////////////////////
        /// <summary>
        /// Captures an image from the camera.
        /// </summary>
        /// <param name="image">A reference to the CameraImage or DarkCameraImage that is to store the newly taken camera image.</param>
        public async Task<CameraImage> AcquireImageAsync(AcquireParams acquireParams,
            CancellationToken ct, IProgress<CameraProgressEventArgs> progress = null)
        {
            await _device.SetExposure(acquireParams.ExposureTime, ct);

            if (acquireParams.ExposureType)
                await _device.LaserTurnOn(ct);

            var image = await _device.TakeImage(acquireParams, ct);

            if (acquireParams.ExposureType)
                await _device.LaserTurnOff(ct);

            return new CameraImage(image, _imageWidth, _imageHeight, acquireParams.ExposureType, 1, 35f, acquireParams.ExposureTime, false, false);
        }

        //////////////////////////////////////////////////////////////////////////	
        /// <summary>
        /// Get the spectrometer's CCD temperature in degrees C.
        /// </summary>
        public async Task<float> GetCCDTemperature(
            CancellationToken ct, IProgress<CameraProgressEventArgs> progress = null)
        {
            await Task.Delay(0);

            return 25f;
        }

        #endregion

        #region Private methods

        /// <summary>
        /// Should be called without await for fire and forget.
        /// </summary>
        /// <param name="task"></param>
        private async void QueueAsync(Task task)
        {
            // keep failed/cancelled tasks in the list
            // they will be observed outside
            _pendingTasks.Add(task);
            try
            {
                await task;
            }
            catch
            {
                return;
            }
            _pendingTasks.Remove(task);
        }

        #region this class events helpers

        ///////////////////////////////////////////////////////////////////////
        /// <summary>
        /// Handles the notifications from the USB device manager of a new USB device.
        /// </summary>
        /// <param name="usbDevice">Reference the the USB device that has been attached.</param>
        public void OnAttached(object sender, EventArgs e)
        {
            _isAttached = true;
            // Check if anyone has registered for the event.            
            Attached?.Invoke(sender, e);
        }

        ///////////////////////////////////////////////////////////////////////
        /// <summary>
        /// Handles the notifications from the USB device manager of the 
        /// removal of a USB device.
        /// </summary>
        /// <param name="usbDevice">Reference the the USB device that has been detached.</param>
        public void OnDetached(object sender, EventArgs e)
        {
            _isAttached = false;
            // Check if anyone has registered for the event.            
            Detached?.Invoke(sender, e);
        }

        #endregion

        #region Device events processing.

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Await.Warning", "CS4014:Await.Warning")]
        private void OnDeviceAttached(object sender, EventArgs e)
        {
            System.Diagnostics.Debug.WriteLine("ImageFileSource.OnDeviceAttached QueueAsync start");
            // Make apropriate work in background.
            QueueAsync(OnDeviceAttachedTask(sender, e));
            System.Diagnostics.Debug.WriteLine("ImageFileSource.OnDeviceAttached QueueAsync done");
        }

        private async Task OnDeviceAttachedTask(object sender, EventArgs e)
        {
            System.Diagnostics.Debug.WriteLine("ImageFileSource.OnDeviceAttachedTask start");
            await Task.Delay(TimeSpan.FromSeconds(2.0f));
            System.Diagnostics.Debug.WriteLine("ImageFileSource.OnDeviceAttachedTask Delay done");
            if (_device.IsAttached)
                OnAttached(sender, e);
            System.Diagnostics.Debug.WriteLine("ImageFileSource.OnDeviceAttachedTask finish");
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Await.Warning", "CS4014:Await.Warning")]
        private void OnDeviceDetached(object sender, EventArgs e)
        {
            _isAttached = false;
            // Check if anyone has registered for the event.            
            Detached?.Invoke(sender, e);
        }

        private async Task OnDeviceDetachedTask(object sender, EventArgs e)
        {
            await Task.Delay(TimeSpan.FromSeconds(2.0f));
            if (!_device.IsAttached)
                OnDetached(sender, e);
        }

        #endregion

        #endregion

        #region Specific methods for UI

        public interface IUserInterface
        {
            string GetDirectory();
        }

        #endregion
    }
}
