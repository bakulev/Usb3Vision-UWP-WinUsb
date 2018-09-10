//using Centice.PASS.Calibration;
//using Centice.PASS.CommonLibrary.Utility;
using Centice.Spectrometry.Base;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
//using Centice.Util;
using System.Windows;
using System.Threading;
using CodaDevices.Devices.BaslerWinUsb;

namespace Centice.Spectrometry.Spectrometers.Cameras
{

    public class ImageFileParams : IParamStorage
    {
        #region Variables

        IDevice _device;

        List<Task> _pendingTasks = new List<Task>();

        IUserInterface _ui;

        CalibrationParameters calibrationParameters = new CalibrationParameters();

        string _strCalibration;

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
        /// Indicates if the camera has an open connection.
        /// </summary>
        public bool IsAttached { get { return _isAttached; } }

        private SpectrometerCalibration _deviceCalibration;
        public SpectrometerCalibration DeviceCalibration
        {
            get { return _deviceCalibration; }
        }

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

        #endregion

        #region Public Constructor

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Await.Warning", "CS4014:Await.Warning")]
        public ImageFileParams (IDevice device, IUserInterface ui)
        {
            _device = device;
            _device.Attached += OnDeviceAttached;
            _device.Detached += OnDeviceDetached;
            _ui = ui;

            // If device already connected then OnConnected should be fired immediately.
            if (_device.IsAttached)
                OnDeviceAttached(this, EventArgs.Empty);
        }

        #endregion

        #region IParamStorage members

        public async Task<string> GetCalibrationString(CancellationToken ct)
        {
            await Task.Delay(0); // temporary

            if (_device != null)
            {
                try
                {
                    _strCalibration = await UploadCalibration();
                }
                catch (Exception ex)
                {
                    //MessageBox.Show(ex.Message);
                    return null;
                }
                return _strCalibration;
            }
            else
            {
                return null;
            }
        }

        public async Task<byte[]> GetCalibrationByteArray(CancellationToken ct)
        {
            await Task.Delay(0); // temporary
            string strCalibration;
            if (_device != null)
            {
                try
                {
                    strCalibration = await UploadCalibration();
                    //_deviceCalibration = XmlSerializationUtil.FromXml<SpectrometerCalibration>(strCalibration);
                    System.Diagnostics.Trace.WriteLine("InspectRxMeasurementSource:LoadCalibration DeviceCalibration");
                    _serialNumber = "serial"; //  _device.SerialNumber;
                }
                catch (Exception ex)
                {
                    //MessageBox.Show(ex.Message);
                    return null;
                }
                return _deviceCalibration.ReconstructionCalibrationByteArray;
            }
            else
            {
                return null;
            }
        }

        public async Task LoadCalibration(CancellationToken ct)
        {
            try
            {
                _strCalibration = await UploadCalibration();
                //_deviceCalibration = XmlSerializationUtil.FromXml<SpectrometerCalibration>(_strCalibration);
                System.Diagnostics.Trace.WriteLine("InspectRxMeasurementSource:LoadCalibration DeviceCalibration");
                _calibrationBinary = new Calibration(DeviceCalibration.ReconstructionCalibrationByteArray);
            }
            catch (Exception ex)
            {
                //MessageBox.Show(ex.Message);
            }
        }

        public float GetLaserWavelength()
        {
            return _deviceCalibration.LaserWavelength;
        }

        private int _shift;

        public int GetShift()
        {
            return _shift;
        }

        public void SetShift(int shift)
        {
            _shift = shift;
        }

        public double[][] GetNistSpectrum()
        {
            return DeviceCalibration.NISTSpectra.Spectrum;
        }

        public double[,] GetDarkImage(float exposure, float temperature)
        {
            var logDirectory = _ui.GetDirectory();
            var setName = _ui.GetSetName();

            string binDirectory = Path.Combine(Path.GetDirectoryName(logDirectory),
                   "Bin", _serialNumber, setName);
            temperature = 30.0f;
            int weight = 30;
            //var binDirectory = Path.Combine(parametersPath, "Bin");

            /*
            var imagesFileNames = Director.EnumerateFiles(
                binDirectory,
                "*.bin", SearchOption.TopDirectoryOnly);

            foreach (var imagesFileName in imagesFileNames)
            {
                var image = LoadDoubleArrToBinary(imagesFileName, badPixels);
                image = Centice.Util.ImageUtils.ImageMedianFilterPoints(image, calibrationParameters.badPixels);
                var values = Path.GetFileNameWithoutExtension(imagesFileName).Split(new char[] { '-' });
                double exposure = int.Parse(values[0]) / 1000d;
                double temperature = int.Parse(values[1]) / 10d;
                int weight = int.Parse(values[2])
                    */

            string exposureStr = exposure.ToString("000.000",
                System.Globalization.CultureInfo.InvariantCulture).Replace(".", "");
            string temperatureStr = temperature.ToString("00.0",
                System.Globalization.CultureInfo.InvariantCulture).Replace(".", "");
            string weightStr = weight.ToString("000",
                System.Globalization.CultureInfo.InvariantCulture).Replace(".", "");
            var imageFileName = exposureStr + "-" + temperatureStr + "-" + weightStr;

            double[,] image = null;//FileUtils.LoadDoubleArrToBinary(Path.Combine(binDirectory, imageFileName + ".bin"));

            return image; // new ushort[ImageSize.Height, ImageSize.Width];
        }

        public System.Collections.Generic.List<Tuple<int, int>> GetBadPixels()
        {
            var calibrationParameters = new CalibrationParameters();
            return calibrationParameters.badPixels;
        }

        private Calibration _calibrationBinary;

        public Calibration Calibration
        {
            get
            {
                return _calibrationBinary;
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

        #region methods from BaseCamera

        ///////////////////////////////////////////////////////////////////////
        /// <summary>
        /// Performs the upload of the camera's active calibration.
        /// </summary>	
        /// <param name="szCalibrationName">The name associated with the calibration.</param>
        /// <param name="calibration">The calibration that was uploaded.</param>	
        /// <returns>Returns true if the calibration upload from the camera succeeds.</returns>
        public async Task<string> UploadCalibration()
        {
            string calibration = null;

            var measurementPath = _ui.GetMeasurementDirectory();

            //if (!string.IsNullOrEmpty(measurementPath))
            //    calibration = await Util.FileUtils.ReadAllTextAsync(Path.Combine(measurementPath, "Calibration.xml"));

            return calibration;
        }

        #endregion

        #region Specific methods for UI

        public interface IUserInterface
        {
            string GetMeasurementDirectory();

            string GetDirectory();

            string GetSetName();
        }

        #endregion
    }
}
