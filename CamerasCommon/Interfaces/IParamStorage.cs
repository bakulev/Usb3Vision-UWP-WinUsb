using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Centice.Spectrometry.Base
{
    public interface IParamStorage
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
        /// Indicates if the camera has an open connection.
        /// </summary>
        bool IsAttached { get; }

        #endregion

        #region Events

        /// <summary>
        /// Event that is fired when a camera is attached.
        /// </summary>
        event EventHandler<EventArgs> Attached;

        /// <summary>
        /// Event that is fired when a camera is detached.
        /// </summary>
        event EventHandler<EventArgs> Detached;

        #endregion

        #region Methods

        Task<string> GetCalibrationString(CancellationToken ct);

        Task<byte[]> GetCalibrationByteArray(CancellationToken ct);

        Task LoadCalibration(CancellationToken ct);

        float GetLaserWavelength();

        int GetShift();

        void SetShift(int shift);

        double[][] GetNistSpectrum();

        double[,] GetDarkImage(float exposure, float temperature);

        List<Tuple<int, int>> GetBadPixels();

        Calibration Calibration { get; }

        SpectrometerCalibration DeviceCalibration { get; }

        #endregion
    }
}
