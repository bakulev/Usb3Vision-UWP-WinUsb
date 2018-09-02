
using Centice.Spectrometry.Base;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;

namespace Centice.Spectrometry.Spectrometers.Cameras
{

    public class ClearShotCamera : ICamera
    {
        #region Variables

        private ClearShotDevice _device;

        private float _exposureTime;

        private float _temperature;

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

        private int _imageHeight;
        //////////////////////////////////////////////////////////////////////////		
        /// <summary>
        /// Gets/Sets the camera's CCD image size.
        /// </summary>
        public int ImageHeight { get { return _imageHeight; } }

        private int _imageWidth;
        public int ImageWidth { get { return _imageWidth; } }

        bool _isAttached;
        //////////////////////////////////////////////////////////////////////////		
        /// <summary>
        /// Indicates if the camera has an open connection.
        /// </summary>
        public bool IsAttached { get { return _isAttached; } }

        #endregion

        #region Events

        // For async events.
        //public Func<object, EventArgs, Task> Attached;

        public event EventHandler<EventArgs> Attached;

        ///////////////////////////////////////////////////////////////////////
        /// <summary>
        /// Handles the notifications from the USB device manager of a new USB device.
        /// </summary>
        /// <param name="usbDevice">Reference the the USB device that has been attached.</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Await.Warning", "CS4014:Await.Warning")]
        public void OnAttached(object sender, EventArgs e)
        {
            QueueAsync(ProceedOnConnected(this, EventArgs.Empty));
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

        #endregion

        #region Public Constructor

        // Async interaction from http://blog.stephencleary.com/2013/01/async-oop-2-constructors.html
        //var myInstance = new MyFundamentalType();
        //// Danger: the instance is not initialized here!
        //await myInstance.Initialization;
        //// OK: the instance is initialized now.
        // also https://blogs.msdn.microsoft.com/pfxteam/2011/01/13/await-and-ui-and-deadlocks-oh-my/
        //public Task Initialization { get; private set; }

        /// <summary>
        /// Async interaction from http://blog.stephencleary.com/2013/01/async-oop-2-constructors.html
        /// </summary>
        /// <param name="device"></param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Await.Warning", "CS4014:Await.Warning")]
        public ClearShotCamera(ClearShotDevice device)
        {
            _device = device;

            _name = "ClearShotWinUsb";
            _modelNumber = "Unknown";
            _serialNumber = "Unknown";
            // Set default image size.
            _imageHeight = 784;
            _imageWidth = 520;
            // Attached
            _isAttached = false;

            //Register events.
            _device.Attached += OnAttached;
            _device.Detached += OnDetached;

            // If camera already connected them OnCameraConnected will not be fired.
            // We need to call it manually.
            if (_device.IsAttached)
                QueueAsync(ProceedOnConnected(this, EventArgs.Empty));
        }

        List<Task> _pendingTasks = new List<Task>();
        /// <summary>
        /// Should be called without await for fire and forget.
        /// </summary>
        /// <param name="task"></param>
        async void QueueAsync(Task task)
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

        async Task ProceedOnConnected(object sender, EventArgs e)
        {
            if (await UpdateCameraInfo())
            {
                _isAttached = true;
                // Check if anyone has registered for the event.            
                Attached?.Invoke(sender, e);
            }
        }

        #endregion

        #region Enumerations

        //////////////////////////////////////////////////////////////////////////	
        /// <summary>
        /// Enumeration of clearshot camera error codes.
        /// </summary>
        public enum CLEARSHOT_CAMERA_ERROR : uint
        {
            NO_ERROR,                   // ""
            NO_CAMERAS_FOUND,           // "No spectrometers found."
            TARGET_CAMERA_NOT_FOUND,    // "Target spectrometer not located."
            CANT_OPEN_CONNECTION,       // "Can't open spectrometer connection."
            FORMAT_7_NOT_SUPPORTED,     // "Spectrometer does not support format 7."
            WRONG_CAMERA_FORMAT,        // "Spectrometer does not support necessary video format."
            CONNECTION_CLOSE_ERROR,     // "Error closing spectrometer's connection."
            SHUTTER_OPEN_ERROR,         // "Error opening spectrometer's shutter."
            SUTTTER_CLOSE_ERROR,        // "Error closing spectrometer's shutter."
            EXPOSURE_START_ERROR,       // "Error starting spectrometer's exposure."
            VIDEO_CAPTURE_ERROR,        // "Error capturing spectrometer's video."
            EXPOSURE_ERROR,             // "Exposure query error."
            EXPOSURE_STOP_ERROR,        // "Error ending spectrometer's exposure."
            START_IMAGE_EXPOSURE_ERROR, // "Error configuring spectrometer."
            VIDEO_STREAM_CLOSE_ERROR,   // "Error closing spectrometer's video stream."
            ACQUIRE_CAMERA_INFO_ERROR,  // "Error acquiring spectrometer information."
            EXPOSURE_PROCESSING_ERROR,  // "Error processing spectrometer's exposure."
            TEMP_SUPPORT_QUERY_ERROR,   // "Error querying if spectrometer supports m_LaserTemperature regulation."
            TEMP_REGULATION_ERROR,      // "Error regulating spectrometer's m_LaserTemperature."
            TEMP_QUERY_ERROR,           // "Error acquiring spectrometer’s CCD m_LaserTemperature."
        }

        #endregion // Enumerations

        #region ICamera

        ///////////////////////////////////////////////////////////////////////
        /// <summary>
        /// Captures an image from the camera.
        /// </summary>
        /// <param name="image">A reference to the CameraImage or DarkCameraImage that is to store the newly taken camera image.</param>
        public async Task<CameraImage> AcquireImageAsync(AcquireParams acquireParams,
            CancellationToken ct, IProgress<CameraProgressEventArgs> progress = null)
        {
            await Task.Delay(0); // temporary for eliminating warning message.

            ushort[] image = await GetImageFromCamera(acquireParams.ExposureType, acquireParams.ExposureTime);

            return new CameraImage(image, _imageWidth, _imageHeight, acquireParams.ExposureType, 1, _temperature, acquireParams.ExposureTime, false, false);
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

        #region Methods

        private async Task<ushort[]> GetImageFromCamera(bool IsLaserChecked, float ExposureTime)
        {
            bool isLaserAvailable = await _device.IsLaserAvailable();
            if (isLaserAvailable && IsLaserChecked) // && _isLaserEnabled == false)
            {
                await _device.SetLaserEnabled(1, true);
            }

            await _device.SetExposureTime(ExposureTime);

            //Start taking image. [shutterState = open, exposureType = light]
            if (await _device.StartExposure(0x01, 0x01))
            {
                var currentTime = DateTime.Now;
                Await:
                //Await the exposure time.
                Thread.Sleep(100); //Just let it wait a second before a 

                //Make sure image is ready.
                if (await _device.QueryExposure())
                {
                    //Get the image.
                    ushort[] img = await _device.GetExposure(_imageHeight, _imageWidth);

                    //Stop the exposure.
                    await _device.EndExposure(0x00); //Close shutter.

                    //Now stop the laser
                    //if (isLaserAvailable && IsLaserChecked && _isLaserEnabled)
                    {
                        await _device.SetLaserEnabled(1, false);
                    }

                    //Quickly set the textbox data.
                    //var maxVal = UInt16.MinValue;
                    //var minVal = UInt16.MaxValue;
                    //for (int i = 10; i < imgHeight; i++)
                    //{
                    //    for (int j = 10; j < imgWidth; j++)
                    //    {
                    //        if (maxVal < img[i, j]) maxVal = img[i, j];
                    //        if (minVal > img[i, j]) minVal = img[i, j];
                    //    }
                    //}
                    
                    //Now convert the image and return.
                    //_lastImageArray = img;
                    //return Imaging.GetImageFromUShort(img, IsScaledChecked, false);
                    return img;
                }
                else
                {
                    goto Await;
                }
            }
            return null;
        }

        ///////////////////////////////////////////////////////////////////////
        /// <summary>
        /// Fetches information about the camera.
        /// </summary>
        private async Task<bool> UpdateCameraInfo()
        {
            bool isSuccess = false;

            if (_device.IsAttached)
            {
                StringBuilder sb = new StringBuilder();

                _modelNumber = await _device.GetModelNumber();
                sb.AppendFormat("\tModel Number: {0}\n", _modelNumber);
                _serialNumber = await _device.GetSerialNumber();
                sb.AppendFormat("\tSerial Number: {0}\n", _serialNumber);
                /*string strCalibration = await camera.GetCalibration();
                // bakulev сохранить калибровку в файл.
                string fileName = DateTime.Now.ToString("yyyyMMdd-HHmmss");
                fileName += "-Calibration" + strSerialNumber + ".xml";
                System.IO.File.WriteAllText(fileName, strCalibration);*/

                //Set default.
                _exposureTime = await _device.GetExposureTime();
                sb.AppendFormat("\tCurrent Exposure: {0} sec\n", _exposureTime);
                //sb.AppendFormat("\tExposure Time Range: {0} s min to {1} s max\n", this.ExposureTime.Min, this.ExposureTime.Max);
                var gain = await _device.GetAfeParameters();
                sb.AppendFormat("\tAnalog Offset: {0}\n", gain.Item1);
                sb.AppendFormat("\tAnalog Gain:   {0}\n", gain.Item2);
                sb.AppendFormat("\tAnalog Config: {0}\n", gain.Item3);
                int[] dims = await _device.GetExposureWidthHeightBits();
                if (dims != null)
                {
                    _imageHeight = dims[0];
                    _imageWidth = dims[1];
                }
                sb.AppendFormat("\tImage Size: {0} width x {1} height\n", _imageWidth, _imageHeight);
                _temperature = await _device.GetCCDTemperature();
                sb.AppendFormat("\tCurrent Temperature: {0}\n", _temperature);

                System.Diagnostics.Trace.WriteLine(sb.ToString());

                isSuccess = true;
            }

            return isSuccess;
        }

        #endregion // BaseCamera
    }
}