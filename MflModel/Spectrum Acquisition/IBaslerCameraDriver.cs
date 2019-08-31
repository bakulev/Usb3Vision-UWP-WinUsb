using System;
using System.Threading;
using System.Threading.Tasks;

namespace CodaDevices.Spectrometry.Model
{
    public class TakeParams
    {
        public bool ExposureType { get; set; }

        public float ExposureTime { get; set; }

        public float AnalogGain { get; set; }

        public float MinGain { get; set; }

        public float MaxGain { get; set; }

        public TakeParams(
            bool exposureType,
            float exposureTime,
            float analogGain,
            float minGain,
            float maxGain
            )
        {
            ExposureType = exposureType;
            ExposureTime = exposureTime;
            AnalogGain = analogGain;
            MinGain = minGain;
            MaxGain = maxGain;
        }
    }

    /// <summary>
    /// Progress EventArgs for all camera awaitable methods.
    /// </summary>
    public class TakeProgressEventArgs : EventArgs
    {
        int _percentage = 0;

        public int Percentage
        {
            get { return _percentage; }
        }

        string _description = "";

        public string Description
        {
            get { return _description; }
        }

        public TakeProgressEventArgs(
            int percentage,
            string description
            )
        {
            _percentage = percentage;
            _description = description;
        }
    }

    /// <summary>
    /// – interface of a driver for spectrometer device
    /// – contains abstract methods describing communication with physical spectrometer
    /// – hides all device-specific details (interface should be device-independent, while concrete implementation should be device-specific)
    /// </summary>
    public interface IBaslerCameraDriver
    {
        // should contain methods related to device operating
        bool GetDeviceAvailability();
        void RegisterDeviceAvailabilityObserver(IDeviceAvailabilityObserver observer);
        void UnregisterDeviceAvailabilityObserver(IDeviceAvailabilityObserver observer);

        #region Fields

        //////////////////////////////////////////////////////////////////////////		
        /// <summary>
        /// Fetches the name of the camera.
        /// </summary>
        string Name { get; }

        //////////////////////////////////////////////////////////////////////////		
        /// <summary>
        /// Gets/sets the camera's model number.
        /// </summary>
        string ModelNumber { get; }

        //////////////////////////////////////////////////////////////////////////		
        /// <summary>
        /// Gets/Sets the camera's serial number.
        /// </summary>
        string SerialNumber { get; }

        //////////////////////////////////////////////////////////////////////////		
        /// <summary>
        /// Gets/Sets the camera's CCD image size.
        /// </summary>
        int ImageHeight { get; }

        int ImageWidth { get; }

        //////////////////////////////////////////////////////////////////////////		
        /// <summary>
        /// Indicates if the camera has an open connection.
        /// </summary>
        bool IsAttached { get; }

        bool IsLaserEnabled { get; }

        #endregion

        event EventHandler<EventArgs> Attached;
        event EventHandler<EventArgs> Detached;
        event EventHandler<EventArgs> LaserDisabled;
        event EventHandler<EventArgs> LaserEnabled;

        Task Reset();
        Task<bool> GetInterlockState();
        Task<float> GetLaserTemperature();
        Task<bool> LaserTurnOff(
            CancellationToken ct);
        Task<bool> LaserTurnOn(
            CancellationToken ct);
        Task<bool> SetExposure(float exposureTime,
            CancellationToken ct);

        Task<ushort[,]> TakeImage(TakeParams acquireParams,
            CancellationToken ct, IProgress<TakeProgressEventArgs> progress = null);

        Task SetLaserState(ushort Laser, bool Enabled);

        Task<bool> GetEnabled(ushort Laser);

        Task<string> GetCalibration();
    }
}
