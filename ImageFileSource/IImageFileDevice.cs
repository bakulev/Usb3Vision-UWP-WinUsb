using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CodaDevices.Devices.ImageFile
{ 
    public class TakeParams
    {
        public bool ExposureType { get; set; }

        public float ExposureTime { get; set; }

        public float AnalogGain { get; set; }

        public float MinGain { get; set; }

        public float MaxGain { get; set; }
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

        public TakeProgressEventArgs(int percentage, string description)
        {
            _percentage = percentage;
            _description = description;
        }
    }

    public interface IDevice
    {

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

    }
}
