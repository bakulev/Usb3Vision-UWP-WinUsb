using Centice.Spectrometry.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Centice.Spectrometry.Spectrometers.Cameras
{
    public class ImageFileLasers : IExcitationLasers
    {
        #region Variables

        IImageFileDevice _device;

        bool _isEnabled = true;

        List<Task> _pendingTasks = new List<Task>();

        #endregion

        #region Fields

        private string _name = "uninitialized";
        //////////////////////////////////////////////////////////////////////////		
        /// <summary>
        /// Fetches the name of the camera.
        /// </summary>
        public string Name { get { return _name; } }

        private string _modelNumber = "uninitialized";
        //////////////////////////////////////////////////////////////////////////		
        /// <summary>
        /// Gets/sets the camera's model number.
        /// </summary>
        public string ModelNumber { get { return _modelNumber; } }

        private string _serialNumber = "uninitialized";
        //////////////////////////////////////////////////////////////////////////		
        /// <summary>
        /// Gets/Sets the camera's serial number.
        /// </summary>
        public string SerialNumber { get { return _serialNumber; } }

        bool _isAttached = false;
        //////////////////////////////////////////////////////////////////////////		
        /// <summary>
        /// Indicates if the laser is attached.
        /// </summary>
        public bool IsAttached { get { return _isAttached; } }

        #endregion

        #region Events

        public event EventHandler<EventArgs> Attached;

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

        public event EventHandler<EventArgs> Detached;

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

        public event EventHandler<EventArgs> Enabled;

        ///////////////////////////////////////////////////////////////////////
        /// <summary>
        /// Handles the notifications from the USB device manager of a new USB device.
        /// </summary>
        /// <param name="usbDevice">Reference the the USB device that has been attached.</param>
        public void OnEnabled(object sender, EventArgs e)
        {
            _isEnabled = true;
            // Check if anyone has registered for the event.            
            Enabled?.Invoke(sender, e);
        }

        public event EventHandler<EventArgs> Disabled;

        ///////////////////////////////////////////////////////////////////////
        /// <summary>
        /// Handles the notifications from the USB device manager of the 
        /// removal of a USB device.
        /// </summary>
        /// <param name="usbDevice">Reference the the USB device that has been detached.</param>
        public void OnDisabled(object sender, EventArgs e)
        {
            _isEnabled = false;
            // Check if anyone has registered for the event.            
            Disabled?.Invoke(sender, e);
        }

        #endregion

        #region Public ctor

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Await.Warning", "CS4014:Await.Warning")]
        public ImageFileLasers(IImageFileDevice device)
        {
            _device = device;
            _device.Attached += OnDeviceAttached;
            _device.Detached += OnDeviceDetached;
            _device.LaserEnabled += OnDeviceLaserEnabled;
            _device.LaserDisabled += OnDeviceLaserDisabled;

            // If device already connected then OnConnected should be fired immediately.
            if (_device.IsAttached)
                OnDeviceAttached(this, EventArgs.Empty);
            if (_device.IsLaserEnabled)
                OnDeviceLaserEnabled(this, EventArgs.Empty);
        }

        #endregion

        #region IExcitationLasers Members

        public async Task<bool> GetEnabled(ushort laserNum)
        {
            bool interlockState = false;
            if (laserNum == 0)
                interlockState = await _device.GetInterlockState();
            else
                throw new Exception("Wrong laserNum");

            return interlockState;
        }

        public async Task<float> GetLaserTemperature(ushort laserNum)
        {
            float laserTemperature = 25.0f;
            if (laserNum == 0)
                laserTemperature = await _device.GetLaserTemperature();
            else
                throw new Exception("Wrong laserNum");

            return laserTemperature;
        }

        public async Task SetLaserState(ushort laserNum, bool isEnabled)
        {
            try
            {
                // TODO fix it
                var ct = new System.Threading.CancellationToken();
                if (laserNum == 0)
                    if (isEnabled)
                        await _device.LaserTurnOn(ct);
                    else
                        await _device.LaserTurnOff(ct);
                else
                    throw new Exception("Wrong laserNum");
            }
            catch (Exception e)
            {
                throw new Exception("Can't set laser state:" + e.Message);
            }
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

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Await.Warning", "CS4014:Await.Warning")]
        private void OnDeviceAttached(object sender, EventArgs e)
        {
            System.Diagnostics.Debug.WriteLine("ImageFileLasers.OnDeviceAttached QueueAsync start");
            // Make apropriate work in background.
            QueueAsync(OnDeviceAttachedTask(sender, e));
            System.Diagnostics.Debug.WriteLine("ImageFileLasers.OnDeviceAttached QueueAsync done");
        }

        private async Task OnDeviceAttachedTask(object sender, EventArgs e)
        {
            System.Diagnostics.Debug.WriteLine("ImageFileLasers.OnDeviceAttachedTask start");
            await Task.Delay(TimeSpan.FromSeconds(2.0f));
            System.Diagnostics.Debug.WriteLine("ImageFileLasers.OnDeviceAttachedTask Delay done");
            if (_device.IsAttached)
                OnAttached(sender, e);
            System.Diagnostics.Debug.WriteLine("ImageFileLasers.OnDeviceAttachedTask finish");
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Await.Warning", "CS4014:Await.Warning")]
        private void OnDeviceDetached(object sender, EventArgs e)
        {
            _isAttached = false;
            // Check if anyone has registered for the event.            
            Detached?.Invoke(sender, e);
        }

        async Task OnDeviceDetachedTask(object sender, EventArgs e)
        {
            await Task.Delay(TimeSpan.FromSeconds(2.0f));
            if (!_device.IsAttached)
                OnDetached(sender, e);
        }

        private void OnDeviceLaserEnabled(object sender, EventArgs e)
        {
            if (_device.IsLaserEnabled)
                OnEnabled(sender, e);
        }

        private void OnDeviceLaserDisabled(object sender, EventArgs e)
        {
            if (!_device.IsLaserEnabled)
                OnDisabled(sender, e);
        }

        #endregion
    }
}
