using System;
using System.Collections.Generic;
using System.Text;
using Centice.Spectrometry.Base;


namespace Centice.Spectrometry.Base
{
	//////////////////////////////////////////////////////////////////////////	
	/// <summary>
	/// Object that defines a configuration for an excitation laser.
	/// </summary>
	public class ExcitationLaserConfiguration : Object, IExcitationLaserConfiguration
	{
		#region Variables

		/// <summary>
		/// Initialization flag.
		/// </summary>
		private bool m_bInitializing = false;

		/// <summary>
		/// The system laser index.
		/// </summary>
		private ushort m_Laser = 0;

		/// <summary>
		/// Specifies if the indexed laser is remote enabled or not.
		/// </summary>
		private bool m_RemoteEnabled = false;

		/// <summary>
		/// The wavelength in nanometers of the laser.
		/// </summary>
		private float m_Wavelength = float.NaN;

		/// <summary>
		/// The current power level of the laser (0=off)
		/// </summary>
		private ushort m_PowerLevel = 0;

		/// <summary>
		/// The set point temperature of the laser in degrees C.
		/// </summary>
		private float m_SetpointTemperature = float.NaN;

		/// <summary>
		/// Specifies if the laser is enabled by its safety key.
		/// </summary>
		private bool m_KeyOn = false;

		/// <summary>
		/// Specifies if the index laser is a valid device.
		/// </summary>
		private bool m_DevicePresent = false;

		/// <summary>
		/// Specifies if the indexed laser has an over current fault.
		/// </summary>
		private bool m_CurrentFault = false;

		/// <summary>
		/// Specifies if the index laser is within its temperature lot-in range.
		/// </summary>
		private bool m_TemperatureLock = false;

		/// <summary>
		/// Specifies if the indexed laser is enable for operation.
		/// </summary>
		private bool m_CurrentDrive = false;

		/// <summary>
		/// Delegate that is to be called when class data is modified.
		/// </summary>
		/// <param name="obj">Object sending the notification.</param>		
		public delegate void ModifyDelegate(ExcitationLaserConfiguration obj);

		/// <summary>
		/// Event that is fired when class data is changed.
		/// </summary>
		public event ModifyDelegate ModifyEvent;
		
		#endregion


		//////////////////////////////////////////////////////////////////////////		
		/// <summary>
		/// Constructs an excitation laser configuration.
		/// </summary>
		public ExcitationLaserConfiguration()
		{
		}


		//////////////////////////////////////////////////////////////////////////		
		/// <summary>
		/// Constructs an excitation laser configuration.
		/// </summary>
		/// <param name="source">The source excitation laser configuration to copy.</param>
		public ExcitationLaserConfiguration(ExcitationLaserConfiguration source)
		{
			// Assign the excitation laser configuration.
			Assign(source);
		}


		//////////////////////////////////////////////////////////////////////////		
		/// <summary>
		/// Assigns the excitation laser configuration of the source to the current object.
		/// </summary>
		/// <param name="that">The source range information to be copied.</param>		
		public void Assign(ExcitationLaserConfiguration that)
		{
			if (that != null)
			{
				this.Laser = that.Laser;
				this.RemoteEnabled = that.RemoteEnabled;
				this.Wavelength = that.Wavelength;
				this.PowerLevel = that.PowerLevel;
				this.SetpointTemperature = that.SetpointTemperature;
				this.KeyOn = that.KeyOn;
				this.DevicePresent = that.DevicePresent;
				this.CurrentFault = that.CurrentFault;
				this.TemperatureLock = that.TemperatureLock;
				this.CurrentDrive = that.CurrentDrive;
			}
		}


		//////////////////////////////////////////////////////////////////////////		
		/// <summary>
		/// Converts the excitation laser configuration to a string.
		/// </summary>
		/// <returns>A string representation of class data.</returns>
		public override string ToString()
		{
			System.Text.StringBuilder sb = new System.Text.StringBuilder();

			sb.AppendFormat("[Laser:{0}]", this.Laser);
			sb.AppendFormat("[RemoteEnabled:{0}]", this.RemoteEnabled);
			sb.AppendFormat("[Wavelength:{0}]", this.Wavelength);
			sb.AppendFormat("[PowerLevel:{0}]", this.PowerLevel);
			sb.AppendFormat("[SetpointTemperature:{0}]", this.SetpointTemperature);
			sb.AppendFormat("[KeyOn:{0}]", this.KeyOn);
			sb.AppendFormat("[DevicePresent:{0}]", this.DevicePresent);
			sb.AppendFormat("[CurrentFault:{0}]", this.CurrentFault);
			sb.AppendFormat("[TemperatureLock:{0}]", this.TemperatureLock);
			sb.AppendFormat("[CurrentDrive:{0}]", this.CurrentDrive);
			return sb.ToString();
		}


		//////////////////////////////////////////////////////////////////////////		
		/// <summary>
		/// Handles notifications of modifications to class data.
		/// </summary>
		protected void HandleModifications()
		{
			// Check if anyone has registered for the event.
			if (ModifyEvent != null)
			{
				// Check if we are not initializing.
				if (!Initializing)
				{
					// Tell whomever that a new value has been stored.
					ModifyEvent(this);
				}
			}
		}

		#region IExcitationLaserConfiguration Members

		//////////////////////////////////////////////////////////////////////////		
		/// <summary>
		/// Gets/Sets a flag indicating if the laser configuration is being initialized.  
		/// </summary>
		public bool Initializing
		{
			get
			{
				return m_bInitializing;
			}
			set
			{
				m_bInitializing = value;
			}
		}


		//////////////////////////////////////////////////////////////////////////		
		/// <summary>
		/// Gets/Sets the system index of the laser.
		/// </summary>
		public ushort Laser
		{
			get
			{
				return this.m_Laser;
			}
			set
			{
				m_Laser = value;

				// Handle data modifications.
				HandleModifications();
			}
		}


		
		//////////////////////////////////////////////////////////////////////////		
		/// <summary>
		/// Gets/Sets if the laser is remote enabled or not.</param>
		/// </summary>
		public bool RemoteEnabled
		{
			get
			{
				return m_RemoteEnabled;
			}
			set
			{
				m_RemoteEnabled = value;

				// Handle data modifications.
				HandleModifications();
			}
		}
		
		
		//////////////////////////////////////////////////////////////////////////		
		/// <summary>
		/// Gets/Sets the wavelength in nanometers of laser.
		/// </summary>
		public float Wavelength
		{
			get
			{
				return m_Wavelength;
			}
			set
			{
				m_Wavelength = value;

				// Handle data modifications.
				HandleModifications();
			}
		}
		
		
		//////////////////////////////////////////////////////////////////////////		
		/// <summary>
		/// Gets/Sets the current power level of the laser (0=off).
		/// </summary>
		public ushort PowerLevel
		{
			get
			{
				return m_PowerLevel;
			}
			set
			{
				m_PowerLevel = value;

				// Handle data modifications.
				HandleModifications();
			}
		}

		
		//////////////////////////////////////////////////////////////////////////		
		/// <summary>
		/// Gets/Sets the set point temperature of the laser in degrees C.
		/// </summary>
		public float SetpointTemperature
		{
			get
			{
				return m_SetpointTemperature;
			}
			set
			{
				m_SetpointTemperature = value;

				// Handle data modifications.
				HandleModifications();
			}
		}
		
		
		//////////////////////////////////////////////////////////////////////////		
		/// <summary>
		/// Gets/Sets if the laser is enabled by its safety key.
		/// </summary>
		public bool KeyOn
		{
			get
			{
				return m_KeyOn;
			}
			set
			{
				m_KeyOn = value;

				// Handle data modifications.
				HandleModifications();
			}
		}
		
		
		//////////////////////////////////////////////////////////////////////////		
		/// <summary>
		/// Gets/Sets if the laser is a valid device (physically present or not).
		/// </summary>
		public bool DevicePresent
		{
			get
			{
				return m_DevicePresent;
			}
			set
			{
				m_DevicePresent = value;

				// Handle data modifications.
				HandleModifications();
			}
		}
		
		
		//////////////////////////////////////////////////////////////////////////		
		/// <summary>
		/// Gets/Sets if the indexed laser has an over current fault.
		/// </summary>
		public bool CurrentFault
		{
			get
			{
				return m_CurrentFault;
			}
			set
			{
				m_CurrentFault = value;

				// Handle data modifications.
				HandleModifications();
			}
		}
		
		
		//////////////////////////////////////////////////////////////////////////		
		/// <summary>
		/// Gets/Sets if the index lase is within its temperature lot-in range.
		/// </summary>
		public bool TemperatureLock
		{
			get
			{
				return m_TemperatureLock;
			}
			set
			{
				m_TemperatureLock = value;

				// Handle data modifications.
				HandleModifications();
			}
		}

		
		
		//////////////////////////////////////////////////////////////////////////		
		/// <summary>
		/// Gets/Sets if the indexed laser is enable for operation.
		/// </summary>
		public bool CurrentDrive
		{
			get
			{
				return m_CurrentDrive;
			}
			set
			{
				m_CurrentDrive = value;

				// Handle data modifications.
				HandleModifications();
			}
		}


        #endregion
    }
}
