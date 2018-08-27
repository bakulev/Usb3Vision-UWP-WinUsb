using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.Serialization;

namespace Centice.Spectrometry.Base
{
    /* replaced by IParamStorage
	//////////////////////////////////////////////////////////////////////////	
	/// <summary>
	/// Interface that defines a spectrometer calibration.
	/// </summary>
	public interface ICalibration		
	{
		//////////////////////////////////////////////////////////////////////////		
		/// <summary>
		/// Gets/Sets the version of calibration information that is being maintained.
		/// </summary>
		int Schema { get; set; }


		//////////////////////////////////////////////////////////////////////////		
        /// <summary>
        /// The user specified name of the calibration.
        /// </summary>
		string Name { get; set; }


		//////////////////////////////////////////////////////////////////////////		
        /// <summary>
        /// The spectral source used to perform the calibration.
        /// </summary>
		string SpectraSource { get; set; }


		//////////////////////////////////////////////////////////////////////////		
        /// <summary>
        /// The known wavelengths and their corresponding pixel positions.
        /// </summary>
		float[] Wavelengths { get; set; }
		float[] ObservedPixels { get; set; }


		//////////////////////////////////////////////////////////////////////////		
        /// <summary>
        /// The polynomial order of the calibration curve fit.
        /// </summary>
		int PolynomialOrder { get; set; }


		//////////////////////////////////////////////////////////////////////////
        /// <summary>
        /// The number of dead pixels on the left side of the image.
        /// </summary>
		short DeadPixelsLeft { get; set; }


		//////////////////////////////////////////////////////////////////////////
		/// <summary>
        /// The number of dead pixels on the right side of the image.
        /// </summary>
		short DeadPixelsRight { get; set; }


		//////////////////////////////////////////////////////////////////////////
		/// <summary>
        /// A constant used to calculate the reconstruction shift.
        /// </summary>
        short ReconstructionShift { get; set; }


		//////////////////////////////////////////////////////////////////////////
		/// <summary>
        /// An array of shift amounts to shift the binned image.
        /// </summary>
        short[] Shifts              { get; set; }


		//////////////////////////////////////////////////////////////////////////
		/// <summary>
        /// The floating point square gray-scale mask matrix.
        /// </summary>
		float[,] GrayScaleMask { get; set; }


		//////////////////////////////////////////////////////////////////////////
		/// <summary>
        /// The floating point square binary mask matrix.
        /// </summary>
        float[,] BinaryMask { get; set; }


		//////////////////////////////////////////////////////////////////////////
		/// <summary>
        /// The model name of the mask.
        /// </summary>
        string MaskName { get; set; }


		//////////////////////////////////////////////////////////////////////////
		/// <summary>
        /// The mask height in elements.
        /// </summary>
		ushort MaskHeight { get; set; }


		//////////////////////////////////////////////////////////////////////////
		/// <summary>
        /// The mask width in elements.
        /// </summary>
		ushort MaskWidth { get; set; }


		//////////////////////////////////////////////////////////////////////////
		/// <summary>
        /// The number of pixels per mask element in the horizontal direction.
        /// </summary>
		ushort MaskHorizontalRatio { get; set; }


		//////////////////////////////////////////////////////////////////////////
		/// <summary>
        /// The number of pixels per mask element in the vertical direction.
        /// </summary>
		ushort MaskVerticalRatio { get; set; }


		//////////////////////////////////////////////////////////////////////////
		/// <summary>
        /// Indicates if the mask is inverted horizontally.
        /// </summary>
		bool MaskHorzFlipped { get; set; }


		//////////////////////////////////////////////////////////////////////////
		/// <summary>
        /// Indicates if the mask is inverted vertically.
        /// </summary>
        bool MaskVertFlipped { get; set; }


		//////////////////////////////////////////////////////////////////////////
		/// <summary>
        /// Indicates if the CCD is inverted.
        /// </summary>
		bool SpectraFlipped { get; set; }


		//////////////////////////////////////////////////////////////////////////
		/// <summary>
        /// An array of pixel rows which define the positions of the mask indices.
        /// </summary>
		int[] MaskIndices { get; set; }


		//////////////////////////////////////////////////////////////////////////
		/// <summary>
        /// An array of shift values to align the image vertically.
        /// </summary>
		short[] MaskXAlignAdjust { get; set; }


		//////////////////////////////////////////////////////////////////////////
		/// <summary>
        /// An array of shift values to align the image horizontally.
        /// </summary>
        short[] MaskYAlignAdjust { get; set; }


		//////////////////////////////////////////////////////////////////////////
		/// <summary>
        /// The instrument model-serial number
        /// </summary>
		string SerialNumber { get; set; }


		//////////////////////////////////////////////////////////////////////////
		/// <summary>
        /// Is true if the calibration is valid, false otherwise.
        /// </summary>
        bool IsValid { get; }

        /// <summary>
        /// Is true when calibration data disabled by Suspend()
        /// </summary>
        bool Suspended
        {
            get;
        }

        /// <summary>
        /// Invalidates configuration untill resume is called
        /// </summary>
        void Suspend();

        /// <summary>
        /// Reverts effect of Suspend operation
        /// </summary>
        void Resume();
        
		//////////////////////////////////////////////////////////////////////////
		/// <summary>
        /// The order of the polynomial used to fit the row calibrations
        /// </summary>
        byte RowFitPolynomialOrder { get; set; }


		//////////////////////////////////////////////////////////////////////////
		/// <summary>
        /// The coefficients of the polynomials used to fit the row calibrations
        /// </summary>
        float[,] RowFitCoefficients { get; set; }


		//////////////////////////////////////////////////////////////////////////		
		/// <summary>
		/// Gets/sets the minimum wave length supported by the instrument.
		/// </summary>
        float MinimumWavelength { get; set; }

		
		//////////////////////////////////////////////////////////////////////////
		/// <summary>
		/// Gets/Sets the maximum wavelength supported by the instrument.
		/// </summary>
        float MaximumWavelength { get; set; }


		//////////////////////////////////////////////////////////////////////////		
		/// <summary>
		/// Gets/Sets NIST intensity correction factors.
		/// </summary>
        float [] NISTIntensityCorrectionFactors { get; set; }


        //////////////////////////////////////////////////////////////////////////		
        /// <summary>
        /// Gets/Sets CCD row begin and row end pairs for each element of the mask.
        /// </summary>
        ushort[,] MaskRows { get; set; }
	}
    */
}
