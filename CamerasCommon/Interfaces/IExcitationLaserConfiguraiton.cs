using System;
using System.Collections.Generic;
using System.Text;

namespace Centice.Spectrometry.Base
{
	//////////////////////////////////////////////////////////////////////////	
	/// <summary>
	/// Interface that defines the functionality associated with an excitation 
	/// lasers operational configuration. 
	/// </summary>
	public interface IExcitationLaserConfiguration
	{

		//////////////////////////////////////////////////////////////////////////		
		/// <summary>
		/// Assigns the excitation laser configuration of the source to the current object.
		/// </summary>
		/// <param name="that">The source range information to be copied.</param>		
		void Assign(ExcitationLaserConfiguration that);


		//////////////////////////////////////////////////////////////////////////		
		/// <summary>
		/// Gets/Sets the system index of the laser.
		/// </summary>
		ushort Laser { get; set; }

		//////////////////////////////////////////////////////////////////////////		
		/// <summary>
		/// Gets/Sets if the laser is remote enabled or not.</param>
		/// </summary>
		bool RemoteEnabled  { get; set; }

		//////////////////////////////////////////////////////////////////////////		
		/// <summary>
		/// Gets/Sets the wavelength in nanometers of laser.</param>
		/// </summary>
		float Wavelength  { get; set; }

		//////////////////////////////////////////////////////////////////////////		
		/// <summary>
		/// Gets/Sets the current power level of the laser (0=off).</param>
		/// </summary>
		ushort PowerLevel  { get; set; }

		//////////////////////////////////////////////////////////////////////////		
		/// <summary>
		/// Gets/Sets the set point temperature of the laser in degrees C.
		/// </summary>
		float SetpointTemperature  { get; set; }

		//////////////////////////////////////////////////////////////////////////		
		/// <summary>
		/// Gets/Sets if the laser is enabled by its safety key.
		/// </summary>
		bool KeyOn  { get; set; }

		//////////////////////////////////////////////////////////////////////////		
		/// <summary>
		/// Gets/Sets if the laser is a valid device (physically present or not).
		/// </summary>
		bool DevicePresent  { get; set; }

		//////////////////////////////////////////////////////////////////////////		
		/// <summary>
		/// Gets/Sets if the indexed laser has an over current fault.
		/// </summary>
		bool CurrentFault  { get; set; }

		//////////////////////////////////////////////////////////////////////////		
		/// <summary>
		/// Gets/Sets if the index laser is within its temperature lot-in range.
		/// </summary>
		bool TemperatureLock  { get; set; }

		//////////////////////////////////////////////////////////////////////////		
		/// <summary>
		/// Gets/Sets if the indexed laser is enable for operation.
		/// </summary>
		bool CurrentDrive  { get; set; }
	}
}
