using Centice.Spectrometry.Base;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace CodaDevices.Devices.BaslerWinUsb
{
    public class BaslerCamera : ICamera
    {
        #region Constructors
        public BaslerCamera(IDevice device)
        {
            _device = device;
            _device.Attached += _device_Attached;
            _device.Detached += _device_Detached;
        }
        #endregion

        #region Fields
        IDevice _device;
        #endregion

        #region Properties
        public string Name => _device.Name;

        public string ModelNumber => _device.ModelNumber;

        public string SerialNumber => _device.SerialNumber;

        public int ImageHeight => _device.ImageHeight;

        public int ImageWidth => _device.ImageWidth;

        public bool IsAttached => _device.IsAttached;
        #endregion

        #region Events
        public event EventHandler<EventArgs> Attached;
        public event EventHandler<EventArgs> Detached;
        #endregion

        #region Methods
        public async Task<CameraImage> AcquireImageAsync(AcquireParams acquireParams, 
            CancellationToken ct, IProgress<CameraProgressEventArgs> progress = null)
        {
            var takeParams = new TakeParams(
                acquireParams.ExposureType,
                acquireParams.ExposureTime,
                acquireParams.AnalogGain,
                acquireParams.MinGain,
                acquireParams.MaxGain
            );
            var takeProgress = new Progress<TakeProgressEventArgs>();
            takeProgress.ProgressChanged += (s, e) =>
            {
                progress.Report(
                    new CameraProgressEventArgs(e.Percentage, e.Description)
                    );
            };

            return new CameraImage()
            {
                Image = await _device.TakeImage(takeParams, ct, takeProgress),
                ImageWidth = ImageWidth, ImageHeight = ImageHeight
            };
        }

        public Task<float> GetCCDTemperature(CancellationToken ct, IProgress<CameraProgressEventArgs> progress = null)
        {
            throw new NotImplementedException();
        }

        private void _device_Detached(object sender, EventArgs e)
        {
            Detached?.Invoke(sender, e);
        }

        private void _device_Attached(object sender, EventArgs e)
        {
            Attached?.Invoke(sender, e);    
        }
        #endregion
    }
}
