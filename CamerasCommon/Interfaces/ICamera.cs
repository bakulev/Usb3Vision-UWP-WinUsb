using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
//using Centice.Spectrometry.Utility; // For ArryaUtil Pack
namespace Centice.Spectrometry.Base
{
    //////////////////////////////////////////////////////////////////////////	
    /// <summary>
    /// Interface that defines the functionality of a camera.
    /// </summary>
    public interface ICamera
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

        //////////////////////////////////////////////////////////////////////////
        /// <summary>
        /// Synchronously acquires an image of the required type.
        /// </summary>
        /// <param name="exposureType">The type of image to acquire (light, dark, or reference).</param>
        Task<CameraImage> AcquireImageAsync(AcquireParams acquireParams, 
            CancellationToken ct, IProgress<CameraProgressEventArgs> progress = null);

        //////////////////////////////////////////////////////////////////////////	
        /// <summary>
        /// Fetches the spectrometer's CCD temperature in degrees C, whether there
        /// is a temperature regulation support from the CCD, or not.
        /// </summary>
        Task<float> GetCCDTemperature(CancellationToken ct, IProgress<CameraProgressEventArgs> progress = null);

        #endregion

#if false

        //////////////////////////////////////////////////////////////////////////
        /// <summary>
        /// Assigns a new Analog Front End (AFE) configuration.
        /// </summary>
        void AssignAnalogConfiguration(short value);

		//////////////////////////////////////////////////////////////////////////		
		/// <summary>
		/// Assigns a new Analog Front End (AFE) offset.
		/// </summary>
		void AssignAnalogOffset(short value);

		//////////////////////////////////////////////////////////////////////////		
		/// <summary>
		/// Assigns a new Analog Front End (AFE) gain.
		/// </summary>
		void AssignAnalogGain(short value);

        //////////////////////////////////////////////////////////////////////////		
        /// <summary>
        /// Gets the last acquired image.
        /// </summary>
        ushort[] Image { get; }

		//////////////////////////////////////////////////////////////////////////		
		/// <summary>
		/// Gets/Sets the last camera error ID number.
		/// </summary>
		uint LastError { get; }

		//////////////////////////////////////////////////////////////////////////		
		/// <summary>
		/// Gets/Sets the last camera error string.
		/// </summary>
		string LastErrorText { get; }


		//////////////////////////////////////////////////////////////////////////		
		/// <summary>
		/// Open a connection to the first camera found on the USB bus.
		/// </summary>
		/// <returns>The USB serial number of the camera that was connected to or 
		/// an empty string on failure.</returns>
		string Open();

		//////////////////////////////////////////////////////////////////////////		
		/// <summary>
		/// Open a connection to the camera with the specified USB serial number.
		/// </summary>
		/// <param name="usbSerialNumber">The USB serial number of the spectrometer to open.</param>
		void Open(string usbSerialNumber);

		//////////////////////////////////////////////////////////////////////////		
		/// <summary>
		/// Closes the connection to the camera.
		/// </summary>
		void Close();

        #region Get Spectrometer Info USB Command

		//////////////////////////////////////////////////////////////////////////		
		/// <summary>
		/// Gets/Sets the type of CCD used by the camera.
		/// </summary>
		SupportedCCDTypes CCDType { get; set; }


		//////////////////////////////////////////////////////////////////////////		
		/// <summary>
		/// Fetches the camera's exposure configuration.
		/// </summary>
		RangeInfo ExposureTime { get; }

	    //////////////////////////////////////////////////////////////////////////		
	    /// <summary>
	    /// Fetches the camera's gain configuration.
	    /// </summary>
	    int GainTime { get; set; }

        #endregion


        #region Get AFE Parameters USB Command


		///////////////////////////////////////////////////////////////////
		/// <summary>
		/// Gets the current exposure AFE configuration and it's range information.
		/// </summary>
		RangeInfo AnalogConfiguration { get; }


		///////////////////////////////////////////////////////////////////
		/// <summary>
		/// Gets the current exposure AFE offset and it's range information.
		/// </summary>
		RangeInfo AnalogOffset { get; }


		///////////////////////////////////////////////////////////////////
		/// <summary>
		/// Gets the current exposure AFE gain and it's range information.
		/// </summary>
		 AnalogGain { get; }

        #endregion

        #endif
    }

    /// <summary>
    /// Progress EventArgs for all camera awaitable methods.
    /// </summary>
    public class CameraProgressEventArgs : EventArgs
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

        public CameraProgressEventArgs(int percentage, string description)
        {
            _percentage = percentage;
            _description = description;
        }
    }

    //////////////////////////////////////////////////////////////////////////	
    /// <summary>
    /// Class that defines a camera image object.
    /// </summary>
    public class CameraImage : Object
    {

        #region Variables

        /// <summary>
        /// The 1-D image data array.
        /// </summary>       
        private ushort[] m_Image;

        /// <summary>
        /// The size of the image being stored.
        /// </summary>
        private int m_ImageHeight;
        private int m_ImageWidth;

        /// <summary>
        /// The CCD temperature when the image was acquired.
        /// </summary>
		private float m_CCDTemperature;


        /// <summary>
        /// The duration of the acquisition in seconds.
        /// </summary>
        private float m_ExposureTime;

        /// <summary>
        /// The exposure type of the image.
        /// </summary>
        private bool m_ExposureType;

        /// <summary>
        /// Indicates if the background was subtracted for this image.
        /// </summary>
		private bool m_BackgroundSubtracted;

        /// <summary>
        /// The number of actual CCD images that were averaged that represent the
        /// CCD image.
        /// </summary>
        private uint m_SamplesAveraged = 1;

        /// <summary>
        /// Indicates if the CCD image has been flagged as being saturated.
        /// </summary>
        private bool m_Saturated = false;

        #endregion


        #region Public Constructors


        //////////////////////////////////////////////////////////////////////////		
        /// <summary>
        /// Constructs a camera image object.
        /// </summary>
        public CameraImage()
        {
        }


        //////////////////////////////////////////////////////////////////////////
        /// <summary>
        /// Constructs a camera image object.
        /// </summary>
        /// <param name="image">The CCD image to be stored.</param>
        /// <param name="exposureType">The type of CCD exposure that was acquired.</param>
        /// <param name="samplesAveraged">The number of exposures that have been average 
        /// together and represent the CCD image being stored.</param>
        /// <param name="ccdTemperature">The CCD temperature when the image was acquired.</param>
        /// <param name="exposureTime">The exposure time of the CCD image.</param>
        /// <param name="backgroundSubtracted">Indicates if a dark CCD image was subtracted from the CCD image.</param>
        /// <param name="saturated">Indicate if the CCD image was saturated.</param>
        public CameraImage(
            ushort[,] image,
            int imageWidth,
            int imageHeight,
            bool exposureType,
            uint samplesAveraged,
            float ccdTemperature,
            float exposureTime,
            bool backgroundSubtracted,
            bool saturated)
        {
            Image = image;
            m_ImageWidth = imageWidth;
            m_ImageHeight = imageHeight;

            ExposureType = exposureType;
            SamplesAveraged = samplesAveraged;
            CCDTemperature = ccdTemperature;
            ExposureTime = exposureTime;
            BackgroundSubtracted = backgroundSubtracted;
            Saturated = saturated;
        }

        #endregion // Public Constructors


        #region Properties

        //////////////////////////////////////////////////////////////////////////		
        /// <summary>
        /// Generates a packed 1-D image from a 2-D image.
        /// </summary>
        /// <param name="m_Image">The source 2-D image.</param>
        /// <returns>A packed 1-D image based on the source 2-D image.</returns>
        public static ushort[] PackImage(ushort[,] Image)
        {
            return null; //TODO fixit ArrayUtil.PackArray(Image);
        }


        //////////////////////////////////////////////////////////////////////////		
        /// <summary>
        /// Generates an unpacked 2-D image from a 1-D image.
        /// </summary>
        /// <param name="PackedImage">The source packed 1-D image.</param>
        /// <param name="iWidth">The width of the source image.</param>
        /// <param name="iHeight">The height of the source image.</param>
        /// <returns>A packed 2-D image based on the source 1-D image.</returns>
        public static ushort[,] UnpackImage(ushort[] PackedImage, int iWidth, int iHeight)
        {
            return null; //TODO fixit ArrayUtil.UnpackArray(PackedImage, iWidth, iHeight);
        }


        //////////////////////////////////////////////////////////////////////////		
        /// <summary>
        /// Generates an unpacked 2-D image from a 1-D image.
        /// </summary>
        /// <param name="image">The camera image to be unpacked.</param>
        /// <returns>A packed 2-D image based on the CameraImage source.</returns>
        public static ushort[,] UnpackImage(CameraImage image)
        {
            return UnpackImage(image.PackedImage, image.ImageWidth, image.ImageHeight);
        }


        //////////////////////////////////////////////////////////////////////////		
        /// <summary>
        /// Fetches a unpacked camera image.
        /// </summary>		
        public ushort[,] Image
        {
            get;set;
        }


        //////////////////////////////////////////////////////////////////////////		
        /// <summary>
        /// Fetches a packed camera image.
        /// </summary>
        public ushort[] PackedImage
        {
            get
            {
                return m_Image;
            }
            set
            {
                m_Image = value;
            }
        }


        //////////////////////////////////////////////////////////////////////////		
        /// <summary>
        /// Gets/Sets the pixel size of the packed camera image.
        /// </summary>
        public int ImageHeight
        {
            get
            {
                return m_ImageHeight;
            }
            set
            {
                m_ImageHeight = value;
            }
        }

        public int ImageWidth
        {
            get
            {
                return m_ImageWidth;
            }
            set
            {
                m_ImageWidth = value;
            }
        }

        //////////////////////////////////////////////////////////////////////////		
        /// <summary>
        /// Fetches the CCD temperature when the image was acquired.
        /// </summary>
        public float CCDTemperature
        {
            get
            {
                return m_CCDTemperature;
            }
            set
            {
                m_CCDTemperature = value;
            }
        }



        //////////////////////////////////////////////////////////////////////////		
        /// <summary>
        /// Fetches the duration of the acquisition in seconds.
        /// </summary>
        public float ExposureTime
        {
            get
            {
                return m_ExposureTime;
            }
            set
            {
                m_ExposureTime = value;
            }
        }


        //////////////////////////////////////////////////////////////////////////		
        /// <summary>
        /// Fetches if the background (dark) was subtracted for this image.
        /// </summary>
        public bool BackgroundSubtracted
        {
            get
            {
                return m_BackgroundSubtracted;
            }
            set
            {
                m_BackgroundSubtracted = value;
            }
        }


        //////////////////////////////////////////////////////////////////////////		
        /// <summary>
        /// Gets/Sets the exposure type of the image.
        /// </summary>
        public bool ExposureType
        {
            get
            {
                return m_ExposureType;
            }
            set
            {
                m_ExposureType = value;
            }
        }


        //////////////////////////////////////////////////////////////////////////		
        /// <summary>
        /// Gets/Sets the number of CCD images that have been averaged together 
        /// and represent the CCD image.
        /// </summary>
        public uint SamplesAveraged
        {
            get
            {
                return m_SamplesAveraged;
            }
            set
            {
                m_SamplesAveraged = value;
            }
        }


        //////////////////////////////////////////////////////////////////////////		
        /// <summary>
        /// Gets/Sets if the CCD image is saturated.
        /// </summary>
        public bool Saturated
        {
            get
            {
                return m_Saturated;
            }
            set
            {
                m_Saturated = value;
            }
        }


        #endregion
    }

    public class AcquireParams
    {
        public bool ExposureType { get; set; }

        public float ExposureTime { get; set; }

        public float AnalogGain { get; set; }

        public float MinGain { get; set; }

        public float MaxGain { get; set; }
    }
}
