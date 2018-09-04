//#define  TimeTests

using System;
using System.Threading;
using System.Text;
using System.IO;

using System.Collections.Generic;
using System.Threading.Tasks;
using Centice.Spectrometry.Base;

namespace Centice.Spectrometry.Spectrometers.Cameras
{

    public class ImageFileDevice
    {
        #region Variables

        IUserInterface _ui;

        //private Timer _timer;

        List<Task> _pendingTasks = new List<Task>();

        bool _isLaserTurnedOn = false;

        float _exposureTime = 0.1f;

        #endregion

        #region Fields

        private string _name = "ImageFileDevice";
        public string Name { get { return _name; } }

        private string _modelNumber = "1.0";
        //////////////////////////////////////////////////////////////////////////		
        /// <summary>
        /// Gets/sets the camera's model number.
        /// </summary>
        public string ModelNumber { get { return _modelNumber; } }

        private string _serialNumber = "000001";
        //////////////////////////////////////////////////////////////////////////		
        /// <summary>
        /// Gets/Sets the camera's serial number.
        /// </summary>
        public string SerialNumber { get { return _serialNumber; } }

        private int _imageHeight = 10; //1200; //520;
        //////////////////////////////////////////////////////////////////////////		
        /// <summary>
        /// Gets/Sets the camera's CCD image size.
        /// </summary>
        public int ImageHeight { get { return _imageHeight; } }

        private int _imageWidth = 25; //1920; //784;
        public int ImageWidth { get { return _imageWidth; } }

        private bool _isAttached = false;
        //////////////////////////////////////////////////////////////////////////		
        /// <summary>
        /// Indicates if the camera has an open connection.
        /// </summary>
        public bool IsAttached { get { return _isAttached; } }

        private bool _isLaserEnabled = true;
        //////////////////////////////////////////////////////////////////////////		
        /// <summary>
        /// Indicates if the camera has an open connection.
        /// </summary>
        public bool IsLaserEnabled { get { return _isLaserEnabled; } }

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

        public event EventHandler<EventArgs> LaserEnabled;

        ///////////////////////////////////////////////////////////////////////
        /// <summary>
        /// Handles the notifications from the USB device manager of a new USB device.
        /// </summary>
        /// <param name="usbDevice">Reference the the USB device that has been attached.</param>
        public void OnLaserEnabled(object sender, EventArgs e)
        {
            _isLaserEnabled = true;
            // Check if anyone has registered for the event.            
            LaserEnabled?.Invoke(sender, e);
        }

        public event EventHandler<EventArgs> LaserDisabled;

        ///////////////////////////////////////////////////////////////////////
        /// <summary>
        /// Handles the notifications from the USB device manager of the 
        /// removal of a USB device.
        /// </summary>
        /// <param name="usbDevice">Reference the the USB device that has been detached.</param>
        public void OnLaserDisabled(object sender, EventArgs e)
        {
            _isLaserEnabled = false;
            // Check if anyone has registered for the event.            
            LaserDisabled?.Invoke(sender, e);
        }

        #endregion

        #region Public Constructor

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Await.Warning", "CS4014:Await.Warning")]
        public ImageFileDevice(IUserInterface ui)
        {
            _ui = ui;

            //_timer = new Timer(TimerCallback);
            //_timer.Change(10000, 10000);

            // Start background process of device connection searching.
            System.Diagnostics.Debug.WriteLine("ImageFileDevice.ImageFileDevice QueueAsync start");
            QueueAsync(PrepareToAttachTask(this, EventArgs.Empty));
            System.Diagnostics.Debug.WriteLine("ImageFileDevice.ImageFileDevice QueueAsync done");
        }

        #endregion

        #region Fire and forget event handlers.

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

        async Task PrepareToAttachTask(object sender, EventArgs e)
        {
            System.Diagnostics.Debug.WriteLine("ImageFileDevice.PrepareToAttachTask start");
            await Task.Delay(TimeSpan.FromSeconds(2.0f));
            System.Diagnostics.Debug.WriteLine("ImageFileDevice.PrepareToAttachTask Delay done");
            OnAttached(sender, e);
            System.Diagnostics.Debug.WriteLine("ImageFileDevice.PrepareToAttachTask finish");
        }

        #endregion

        //private void TimerCallback(object state)
        //{
        //    _timer.Dispose();
        //    _timer = null;
        //    OnAttached(this, EventArgs.Empty);
        //}

        #region Main methods of the device

        public async Task<bool> LaserTurnOn(
            CancellationToken ct, IProgress<CameraProgressEventArgs> progress = null)
        {
            await Task.Delay(0); // temporary for eliminating warning message.
            bool isSucess = false;
            if (!_isLaserTurnedOn)
            {
                _isLaserTurnedOn = true;
                isSucess = true;
            } 
            return isSucess;
        }

        public async Task<bool> LaserTurnOff(
            CancellationToken ct, IProgress<CameraProgressEventArgs> progress = null)
        {
            await Task.Delay(0); // temporary for eliminating warning message.
            bool isSucess = false;
            if (_isLaserTurnedOn)
            {
                _isLaserTurnedOn = false;
                isSucess = true;
            }
            return isSucess;
        }

        public async Task<bool> GetInterlockState()
        {
            await Task.Delay(0); // temporary for eliminating warning message.
            return true;
        }

        public async Task<float> GetLaserTemperature()
        {
            await Task.Delay(0); // temporary for eliminating warning message.
            return 25.0f;
        }

        public async Task<float> GetExposure()
        {
            await Task.Delay(0); // temporary for eliminating warning message.

            return _exposureTime;
        }

        public async Task<bool> SetExposure(float exposureTime,
            CancellationToken ct, IProgress<CameraProgressEventArgs> progress = null)
        {
            await Task.Delay(0); // temporary for eliminating warning message.
            bool isSucess = false;

            _exposureTime = exposureTime;
            isSucess = true;

            return isSucess;
        }

        public async Task<ushort[]> TakeImage(
            CancellationToken ct, IProgress<CameraProgressEventArgs> progress = null)
        {
            await Task.Delay(0); // temporary for eliminating warning message.
            ushort[] image;

            //return MakeSimulateImage(ct, progress);
            return MakeSimulateImage2(ct, progress);

            var measurementPath = _ui.GetDirectory();

            /* // test convertion
            var imageUshort = PASS.CommonLibrary.Utility.Imaging.Png16LoadImageData(
                Path.Combine(measurementPath,
                _exposureTime.ToString("000.000",
                    System.Globalization.CultureInfo.InvariantCulture).Replace(".", "") + "L",
                "00000.png"));
            //var imageDouble = new double[imageUshort.GetLength(0), imageUshort.GetLength(1)];
            var imageDouble = new double[_imageHeight, _imageWidth];
            for (int i = 0; i < imageUshort.GetLength(0); i++)
                for (int j = 0; j < imageUshort.GetLength(1); j++)
                    imageDouble[i, j] = imageUshort[i, j];
            imageDouble = DrawBorders(imageDouble);
            return ConvertDoubleToUshort(imageDouble);
            */

            if (_isLaserTurnedOn)
                image = PASS.CommonLibrary.Utility.Imaging.Png16LoadImagePacked(
                    Path.Combine(measurementPath,
                    _exposureTime.ToString("000.000",
                        System.Globalization.CultureInfo.InvariantCulture).Replace(".", "") + "L",
                    "00000.png"), ct);
            else
                image = PASS.CommonLibrary.Utility.Imaging.Png16LoadImagePacked(
                    Path.Combine(measurementPath,
                    _exposureTime.ToString("000.000",
                        System.Globalization.CultureInfo.InvariantCulture).Replace(".", "") + "D",
                    "00000.png"), ct);

            return image;
        }

        #endregion

        #region Private methods

        private ushort[] MakeSimulateImage(
            CancellationToken ct, IProgress<CameraProgressEventArgs> progress = null)
        {
            double[,] mask = new double[64,64];
            //mask = DrawBorders(mask);
            mask = GetMaskBinary();

            var imageDouble = SimulateImage(mask);

            return ConvertDoubleToUshort(imageDouble);
        }

        private static ushort[] ConvertDoubleToUshort(double[,] imageDouble)
        {
            int imageHeight = imageDouble.GetLength(0);
            int imageWidth = imageDouble.GetLength(1);
            double imageDoubleMin = double.MaxValue;
            double imageDoubleMax = double.MinValue;
            for (int h = 0; h < imageHeight; h++)
                for (int w = 0; w < imageWidth; w++)               
                {
                    if (imageDoubleMin > imageDouble[h, w]) imageDoubleMin = imageDouble[h, w];
                    if (imageDoubleMax < imageDouble[h, w]) imageDoubleMax = imageDouble[h, w];
                }

            ushort[] imageUshort = new ushort[imageHeight * imageWidth];
            for (int h = 0; h < imageHeight; h++)
                for (int w = 0; w < imageWidth; w++)                
                    imageUshort[h * imageWidth + w] = (ushort)((imageDouble[h, w] - imageDoubleMin) *
                        ushort.MaxValue / (imageDoubleMax - imageDoubleMin));
            return imageUshort;
        }

        private double[,] GetMaskBinary()
        {
            var maskList = new List<double[]>();
            var minLen = int.MaxValue;
            var lines = File.ReadAllLines(@"C:\Users\abaku\Pictures\Tilt_MFL\mask.txt");
            foreach (var line in lines)
            {
                var value = line.Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries);
                var len = value.GetLength(0);
                if (minLen > len) minLen = len;
                var bin = new double[len];
                for (int i = 0; i < len; i++)
                    bin[i] = double.Parse(value[i]);
                maskList.Add(bin);
            }
            var maskBinary = new double[minLen, maskList.Count];
            for (int i = 0; i < minLen; i++)
                for (int j = 0; j < maskList.Count; j++)
                    maskBinary[i, j] = maskList[i][j];
            return maskBinary;
        }

        private double[,] SimulateImage(double[,] maskBinary)
        {
            double maskExpantionRatio = 2d; // 4.17d;
            double curvature = 2d; // 1.2d;

            double[] spectrumSimulated = new double[_imageWidth + ((int)(maskBinary.GetLength(0) / 2 * maskExpantionRatio) - 1) * 2 + 1];
            spectrumSimulated[0] = 1;
            spectrumSimulated[(spectrumSimulated.GetLength(0) - 1) / 2] = 1;
            //spectrumSimulated[(spectrumSimulated.GetLength(0) - 1) / 2 + (int)(maskExpantionRatio * 5 + 1)] = 1;
            spectrumSimulated[spectrumSimulated.GetLength(0) - 1] = 1;
            //for (int i = 0; i < spectrumSimulated.GetLength(0); i++)
            //    if (i % 30 == 0) spectrumSimulated[i] = 0.5;
            //FileHelpers.SaveDoubleArrToCsv(spectrumSimulated, await FileHelpers.CreateFile(folder, "Results\\00SpectrumSimulated.csv"));

            double[,] imageBinned = new double[64, _imageWidth]; // 379 on both sides, 380 - center

            // Simulate binned image by given spectrum. Center of spectrum on center of image.
            // 443 px spectrumSimulated to 379 px of _imageBinned
            double shift = (maskBinary.GetLength(0) / 2d * maskExpantionRatio) - 1;
            double a2 = -curvature / (32d * 32d); // (32d - 43d) / (32d * 32d);
            double a1 = 1d - a2 * 64d;
            for (int i = 0; i < spectrumSimulated.GetLength(0); i++)
            {
                if (spectrumSimulated[i] > 0)
                    CodedApertureSimulation.SimulateLineCurvature(ref imageBinned, maskBinary, i - shift, spectrumSimulated[i], maskExpantionRatio, a2, a1);
                //CodedApertureHelper.SimulateLine(ref _imageBinned, _maskBinary, i - shift, 1, maskExpantionRatio);
            }

            var image = new double[_imageHeight, _imageWidth];
            for (int w = 0; w < _imageWidth; w++)
                for (int b = 0; b < imageBinned.GetLength(0); b++)
                    for (int c = 0; c < 4; c++)
                        image[150 + b * 5 + c, w] = imageBinned[b, w];

            return image;
        }

        private ushort[] MakeSimulateImage2(
            CancellationToken ct, IProgress<CameraProgressEventArgs> progress = null)
        {
            var image = new double[_imageHeight, _imageWidth];

            //image = DrawTriangle(image, 645.5, 80.2, 461.4, 257.9, 769.3, 447.7);
            //image = DrawTriangle(image, 645.5, 80.2, 461.4, 83.9, 769.3, 447.7);
            image = DrawTriangle(image, 7.0 - 0.25 + 5.0 * 2.0, 3.0 + 0.05 - 1.0 * 2.0, 2.0 - 0.25, 4.0 + 0.05, 25.75, 4.05);
            //image = DrawBorders(image);

            return ConvertDoubleToUshort(image);
        }

        private double[,] DrawTriangle(double[,] image, double x1, double y1, double x2, double y2, double x3, double y3)
        {
            double yMax = image.GetLength(0);
            double xMax = image.GetLength(1);

            //image = DrawPoint(image, x1, y1);
            //image = DrawPoint(image, x2, y2);
            image = DrawPoint(image, x3, y3);
            // x direction is along image width, lower x is on the left.
            // proceed only for ordered vertices
            // y1 on top y3 on bottom. y directrion is along image heiht, lower y is on the top.
            if (y1 <= y2 && y2 <= y3) {
                // find intersection line
                double x4 = x1 + ((y2 - y1) / (y3 - y1)) * (x3 - x1);
                //image = DrawPoint(image, x4, y2);

                // draw top flat triangle
                int l = (int)(y1 + - y1 % 1);
                image = DrawLineBot(image, l, x2, y2, x1, y1, x4);
                image = DrawLineBot(image, l + 1, x2, y2, x1, y1, x4);
                image = DrawLineBot(image, l + 2, x2, y2, x1, y1, x4);
                image = DrawLineBot(image, l + 3, x2, y2, x1, y1, x4);
                //image = DrawPoint(image, x1i, y1b);
                // draw bottom line
                for (int y = (int)Math.Ceiling(y1); y <= y2; y++)
                {
                    //for (int x = (int)Math.Floor(x1); x < x4; x++)
                    //    image[x, y] = 1;
                }
                // draw top flat triangle
                for (int y = (int)Math.Floor(y3); y > y2; y--)
                {
                    //for (int x = (int)Math.Floor(x1); x < x4; x++)
                    //    image[x, y] = 1;
                }
            }
            return image;
        }

        private double[,] DrawLineBot(double[,] image, int l, double x1, double y1, double x2, double y2,
            double x3)
        {
            double yMax = image.GetLength(0);
            double xMax = image.GetLength(1);

            double i1x = LineIntersectionX(x1, y1, x2, y2, 0, l + 1, xMax, l + 1);
            double i2x = LineIntersectionX(x1, y1, x2, y2, 0, l, xMax, l);
            double i3x = LineIntersectionX(x2, y2, x3, y1, 0, l, xMax, l);
            double i4x = LineIntersectionX(x2, y2, x3, y1, 0, l + 1, xMax, l + 1);
            double k1 = (y1 - y2) / (x2 - x1);
            double k2 = (y2 - y1) / (x3 - x2);
            int s1x = (int)(i1x - i1x % 1);
            int s2x = (int)(i2x - i2x % 1);
            int s3x = (int)(i3x - i3x % 1);
            int s4x = (int)(i4x - i4x % 1);
            double y1s = k1 * (1 - i2x % 1);
            double y2s = k2 * (1 - i3x % 1);
            if (l > 0 && l < yMax && s1x > 0 && s1x < xMax)
                image[l, s1x] = y1s / 2;
            for (int x = s1x + 1; x < s2x; x++)
            {
                double intencity = y1s + k1 * (x - s1x - 1) + k1 / 2;
                if (intencity > 0)
                    if (l > 0 && l < yMax && x > 0 && x < xMax)
                        if (intencity > 1)
                            image[l, x] = 1;
                        else
                            image[l, x] = intencity;
            }
            //TODO make connection between edges if no gap ( s2x = s3 )
            if (s2x + 1 > s3x)
            {
                double intencity =
                    (1 - k1 * (i2x % 1) / 2) * (x1 % 1) +
                    (1 + y2s / 2) * (1 - x1 % 1);
                if (l > 0 && l < yMax && s2x > 0 && s2x < xMax)
                    image[l, s2x] = intencity;
            }
            else
            {
                if (l > 0 && l < yMax && s2x > 0 && s2x < xMax)
                    image[l, s2x] = 1 - k1 * (i2x % 1) / 2;
                for (int x = s2x + 1; x < s3x; x++)
                {
                    if (l > 0 && l < yMax && x > 0 && x < xMax)
                        image[l, x] = 1;
                }
                if (l > 0 && l < yMax && s3x > 0 && s3x < xMax)
                    image[l, s3x] = 1 + y2s / 2;
            }
            for (int x = s3x + 1; x < s4x; x++)
            {
                double intencity = 1 + y2s + k2 * (x - s3x - 1) + k2 / 2;
                if (intencity > 0)
                    if (l > 0 && l < yMax && x > 0 && x < xMax)
                        if (intencity > 1)
                            image[l, x] = 1;
                        else
                            image[l, x] = intencity; 
            }
            if (l > 0 && l < yMax && s4x > 0 && s4x < xMax)
                image[l, s4x] = - k2 * (1 - i4x % 1) / 2;

            return image;
        }

        private double LineIntersectionX(double x1, double y1, double x2, double y2, 
            double x3, double y3, double x4, double y4)
        {
            return ((x1 * y2 - y1 * x2) * (x3 - x4) - (x1 - x2) * (x3 * y4 - y3 * x4)) / 
                ((x1 - x2) * (y3 - y4) - (y1 - y2) * (x3 - x4));
        }

        private double LineIntersectionY(double x1, double y1, double x2, double y2,
            double x3, double y3, double x4, double y4)
        {
            return ((x1 * y2 - y1 * x2) * (y3 - y4) - (y1 - y2) * (x3 * y4 - y3 * x4)) / 
                ((x1 - x2) * (y3 - y4) - (y1 - y2) * (x3 - x4));
        }


        private double[,] DrawBorders(double[,] image)
        {
            for (int h = 0; h < image.GetLength(0); h++)
            {
                image[h, 0] = 1;
                image[h, image.GetLength(1) - 1] = 1;
            }
            for (int w = 0; w < image.GetLength(1); w++)
            {
                image[0, w] = 1;
                image[image.GetLength(0) - 1, w] = 1;
            }
            return image;
        }

        private double[,] DrawPoint(double[,] image, double x1, double y1)
        {
            double yMax = image.GetLength(0);
            double xMax = image.GetLength(1);

            double x1p = x1 % 1;
            double y1p = y1 % 1;
            double x1b = (1 - x1p);
            double y1b = (1 - y1p);
            int x1f = (int)(x1 - x1p);
            int x1c = (int)(x1 + x1b);
            int y1f = (int)(y1 - y1p);
            int y1c = (int)(y1 + y1b);
            if (y1f > 0 && y1f < yMax && x1f > 0 && x1f < xMax) image[y1f, x1f] = x1b * y1b;
            if (y1f > 0 && y1f < yMax && x1c > 0 && x1c < xMax) image[y1f, x1c] = x1p * y1b;
            if (y1c > 0 && y1c < yMax && x1f > 0 && x1f < xMax) image[y1c, x1f] = x1b * y1p;
            if (y1c > 0 && y1c < yMax && x1c > 0 && x1c < xMax) image[y1c, x1c] = x1p * y1p;
            //image[(int)Math.Round(y1), (int)Math.Round(x1)] = 1;
            return image;
        }

        #endregion

        #region Specific methods for UI

        public interface IUserInterface
        {
            string GetDirectory();
        }

        #endregion
    }
}
