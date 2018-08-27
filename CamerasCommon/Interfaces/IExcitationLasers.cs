using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Centice.Spectrometry.Base
{
	//////////////////////////////////////////////////////////////////////////	
	/// <summary>
	/// Interface that defines the functionality of excitation lasers.
	/// </summary>
    public interface IExcitationLasers
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

        /// <summary>
        /// Event that is fired when a camera is attached.
        /// </summary>
        event EventHandler<EventArgs> Enabled;

        /// <summary>
        /// Event that is fired when a camera is detached.
        /// </summary>
        event EventHandler<EventArgs> Disabled;

        #endregion

        #region Methods

        //////////////////////////////////////////////////////////////////////////		
        /// <summary>
        /// Gets the temperature setting of the indexed laser.
        /// </summary>
        /// <param name="Laser">The laser to index (1 to N where N is the number of supported lasers).</param>
        /// <param name="Temperature">The current temperature of the index laser.</param>
        Task<float> GetLaserTemperature(ushort Laser);

		//////////////////////////////////////////////////////////////////////////		
		/// <summary>
		/// Fetches the operational state of the indexed excitation laser.
		/// </summary>
		/// <param name="Laser">The laser to index (1 to N where N is the number of supported lasers)</param>
		/// <param name="TemperatureRegulationEnabled">True if the laser is enabled.</param>
		Task<bool> GetEnabled(ushort Laser);

		//////////////////////////////////////////////////////////////////////////		
		/// <summary>
		/// Turn on/off the index excitation laser.
		/// </summary>
		/// <param name="Laser">The laser to index (1 to N where N is the number of supported lasers)</param>
		/// <param name="TemperatureRegulationEnabled">True if the laser is enabled.</param>		
		Task SetLaserState(ushort Laser, bool Enabled);

        //////////////////////////////////////////////////////////////////////////		
        /// <summary>
        /// Fetches the current configuration of the indexed excitation laser.
        /// </summary>
        /// <param name="Laser">The laser to index (1 to N where N is the number of supported lasers).</param>
        /// <param name="config">The excitation laser configuration of the indexed laser.</param>
        //void GetLaserConfiguration(ushort Laser, out ExcitationLaserConfiguration config);

        //////////////////////////////////////////////////////////////////////////		
        /// <summary>
        /// Assigns an excitation laser configuration to the indexed laser.
        /// </summary>
        /// <param name="Laser">The laser to index (1 to N where N is the number of supported lasers).</param>
        /// <param name="config">The excitation laser configuration to be 
        /// assigned to the indexed laser.</param>
        //void SetLaserConfiguration(ushort Laser, ExcitationLaserConfiguration config);

        //////////////////////////////////////////////////////////////////////////		
        /// <summary>
        /// Fetches the index of the active excitation laser.
        /// </summary>
        /// <returns>The index (1 to N where N is the number of supported lasers) 
        /// of the active excitation laser.</returns>
        //ushort GetActiveLaser();

        //////////////////////////////////////////////////////////////////////////		
        /// <summary>
        /// Set the index excitation laser to be active.
        /// </summary>
        /// <param name="Laser">Index (1 to N where N is the number of supported 
        /// lasers) of the excitation laser to be activated.</param>
        //void SetActiveLaser(ushort Laser);

        #endregion
    }
}
