using System;
using System.IO;
using System.ComponentModel;
using System.Diagnostics;
using System.Collections.Generic;
using System.Collections;
using System.Text;
using System.Security.Permissions;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Reflection;
using Centice.Spectrometry.Base;

namespace Centice.Spectrometry.Base
{

    #region CalibrationBinderClass
    ///////////////////////////////////////////////////////////////////////////
    /// <summary>
    /// This binders class allows older calibration objects to be bound 
    /// to the newer Spectrometer.Calibration object.
    /// </summary>
    sealed public class CalibrationBinder : SerializationBinder
    {
        ///////////////////////////////////////////////////////////////////////
        /// <summary>
        /// Controls the binding of a serialized object to a type.
        /// </summary>
        /// <param name="assemblyName">Specifies the Assembly name of the serialized object. </param>
        /// <param name="typeName">Specifies the Type name of the serialized object. </param>
        /// <returns>The type of the object the formatter creates a new instance of.</returns>
        public override Type BindToType(string assemblyName, string typeName)
        {
            Type typeToDeserialize = null;

            // For each assemblyName/typeName that you want to deserialize to
            // a different type, set typeToDeserialize to the desired type.
            String szThisAssemblyName = Assembly.GetExecutingAssembly().FullName;

            if ((assemblyName.IndexOf("ClearVu") == 0)
                || (assemblyName.IndexOf("Newport") == 0)
                || (assemblyName.IndexOf("Matrix") == 0)
                || (assemblyName.IndexOf("Spectrometer") == 0)
                || (assemblyName.EndsWith("null"))
                || (assemblyName.StartsWith("SpectraData"))
                || (assemblyName.StartsWith("Centice"))
                || (assemblyName.StartsWith("CRE"))
                )
            {
                // To use a type from a different assembly version, 
                // change the version number.
                // To do this, uncomment the following line of code.
                assemblyName = szThisAssemblyName;

                // To use a different type from the same assembly, 
                // change the type name.
                //typeName = "Centice.SpectraData.Calibration";
                typeName = "Centice.Spectrometry.Base.Calibration";
            }

            // The following line of code returns the type.
            // Note: Exceptions will occur here because the assembiles are now strong named.  Since 
            // .NET serialization stores the assembly manifest information in the serialized data, 
            // Calibrations saved with a previous build will no longer be readable.
            typeToDeserialize = Type.GetType(String.Format("{0}, {1}", typeName, assemblyName));

            return typeToDeserialize;
        }
    }
    #endregion

    [Serializable]
    public class Calibration : ISerializable // ICalibration, IComparable<ICalibration> 
    {

        /// <summary>
        /// Maximum number of number of entries in observed pixels and actual wavelengths.
        /// </summary>
        public const int MAX_ENTRIES = 10;
        public const byte VERT_MASK_FLIP = 0x01;
        public const byte HORZ_MASK_FLIP = 0x02;

        /// <summary>
        /// The default schema version.
        /// </summary>
        public const int DEFAULT_SCHEMA = 9;
        /// <summary>
        /// Synchronization object
        /// </summary>
        private object oSyncRoot = new object();

        /// <summary>
        /// Synchronization object
        /// </summary>
        public object SyncRoot
        {
            get { return oSyncRoot; }
        }

        private static readonly byte[] m_FileHeader = { 0xfe, 0xfc, 0x4c, 0x01, 0x10, 0xc4, 0xcf, 0xef };
        #region Constructors

        /*
        /// <summary>
        /// This constructor should never be called explicitly by clients as it does not initialize
        /// the object with sane values. A public default constructor is required for .Net serialization
        /// which we currently are not using, but are not yet ready to throw away either. Hence this ctor may
        /// be changed to public at some later date, when we have some default initialization.
        /// 
        /// In order to create a Calibration object that is properly initialized with the appropriate
        /// default values it should be done by way of the Reconstruction Engine.
        /// 
        /// EXAMPLE: 
        /// Centice.Spectrometry.Base.ICalibration cal = ReconstructionEngine.FromDefaultConfiguration(
        ///            ModelNumber, SerialNumber,
        ///            Camera.ImageSize.Width, Camera.ImageSize.Height) as ICalibration;
        /// 
        /// </summary>
        private Calibration() { }
        */

        /*
        public Calibration(ICalibration calibration)
        {

            this.Schema = calibration.Schema;

            this.Name = calibration.Name;
            this.SpectraSource = calibration.SpectraSource;

            this.Wavelengths = calibration.Wavelengths;
            this.ObservedPixels = calibration.ObservedPixels;
            this.SourcePeaks = (ushort)(m_fObservedPixels.Length);
            this.PolynomialOrder = calibration.PolynomialOrder;

            this.DeadPixelsLeft = calibration.DeadPixelsLeft;
            this.DeadPixelsRight = calibration.DeadPixelsRight;

            this.ReconstructionShift = calibration.ReconstructionShift;
            this.Shifts = calibration.Shifts;

            this.GrayScaleMask = calibration.GrayScaleMask;
            this.BinaryMask = calibration.BinaryMask;

            this.MaskName = calibration.MaskName;

            this.MaskHeight = calibration.MaskHeight;
            this.MaskWidth = calibration.MaskWidth;

            this.MaskHorizontalRatio = calibration.MaskHorizontalRatio;
            this.MaskVerticalRatio = calibration.MaskVerticalRatio;
            this.MaskHorzFlipped = calibration.MaskHorzFlipped;
            this.MaskVertFlipped = calibration.MaskVertFlipped;
            this.SpectraFlipped = calibration.SpectraFlipped;

            this.MaskIndices = calibration.MaskIndices;
            this.MaskXAlignAdjust = calibration.MaskXAlignAdjust;
            this.MaskYAlignAdjust = calibration.MaskYAlignAdjust;

            this.SerialNumber = calibration.SerialNumber;

            this.RowFitPolynomialOrder = calibration.RowFitPolynomialOrder;
            this.RowFitCoefficients = calibration.RowFitCoefficients;

            this.MinimumWavelength = calibration.MinimumWavelength;
            this.MaximumWavelength = calibration.MaximumWavelength;


            this.NISTIntensityCorrectionFactors = calibration.NISTIntensityCorrectionFactors;
            this.MaskRows = calibration.MaskRows;
        }
        */

        public Calibration(byte[] data)
        {
            oSyncRoot = new object();
            this.FromByteArray(data);
        }

        #endregion // Constructors

        #region Private Members

        private int m_iSchema = DEFAULT_SCHEMA;

        private string m_Name;

        /// <summary>
        /// Holds information on the calibration source.
        /// </summary>
        private string m_spectraSource;

        /// <summary>
        /// Holds the spectrometers serial number.
        /// </summary>
        private string m_serialNumber;

        /// <summary>
        /// Holds observed pixels.
        /// </summary>
        private float[] m_fObservedPixels = null;

        /// <summary>
        /// Holds actual wavelengths.
        /// </summary>
        private float[] m_fActualWavelengths = null;

        /// <summary>
        /// Holds number of entries in observed pixels and actual wavelengths are valid.
        /// </summary>
        private int m_iEntries = 0;

        /// <summary>
        /// Holds the polynomial order that should be used in conjunction 
        /// with a polynomial regression algorithm.  This value should be a odd number.
        /// </summary>
        private int m_iPolynomialOrder = 3;

        /// <summary>
        /// The mask to be used.
        /// </summary>
        private string m_szMaskName;

        /// <summary>
        /// True if the mask is flipped along the Y axis.
        /// </summary>
        private bool m_bMaskVertFlipped;

        /// <summary>
        /// True if the spectra data is flipped along the X axis due to optics layout.
        /// </summary>
        private bool m_bSpectraFlipped;

        /// <summary>
        /// An array of Y pixel indices that have been identified as being 
        /// associated with rows of the mask.  This tells us where the mask 
        /// falls on the CCD.
        /// </summary>
        private int[] m_nMaskIndices;


        /// <summary>
        /// An array of offsets that should be applied to the mask indices rows to 
        /// align all the pixels of the mask along the X-axis.  This will allow 
        /// us to correct for curvature of the mask image due to optics.
        /// </summary>
        private short[] m_nMaskXAlignAdjust;

        /// <summary>
        /// An array of offsets that should be applied to the columns of the 
        /// CCD data to align all the pixels of the mask along the Y-axis.  This will 
        /// allow us to adjust the mask image due to a physical miss-alignment of the CCD.
        /// </summary>
        private short[] m_nMaskYAlignAdjust;

        /// <summary>
        /// The number of pixels to shift reconstructed data.
        /// </summary>
        private ushort m_uSourcePeaks = 1;

        /// <summary>
        /// An array of the number of pixels binned spectra data will have to 
        /// be shifted along the X-axis in order to align rows of spectra data.
        /// This array will be filled out based during the calibration process.
        /// Note: Shifts should be linear from top to bottom according to Prasant.
        /// </summary>
        private short[] m_nPeakShifts;

        /// <summary>
        /// The number of dead (unusable) pixels to the left side of the CCD.
        /// </summary>
        private short m_nDeadPixelsLeft;

        /// <summary>
        /// The number of dead (unusable) pixels to the right side of the CCD.
        /// </summary>
        private short m_nDeadPixelsRight;

        /// <summary>
        /// True if the mask is flipped along the X axis.
        /// </summary>
        private bool m_bMaskHorzFlipped;

        /// <summary>
        /// The number of pixels to shift during reconstruction.
        /// </summary>
        //[NotSerialized]
        private short m_nReconShift = 1;

        /// <summary>
        /// The binary mask to use for reconstruction.
        /// </summary>
        private float[,] m_BinaryMask;

        /// <summary>
        /// The gray scale mask to use for reconstruction.
        /// </summary>
        private float[,] m_GrayScaleMask;

        /// <summary>
        /// The pixel width of the mask.
        /// </summary>
        private ushort m_MaskWidth;

        /// <summary>
        /// The pixel height of the mask.
        /// </summary>
        private ushort m_MaskHeight;

        /// <summary>
        /// The number of CCD columns that comprise a mask element.
        /// </summary>
        private ushort m_MaskHorizontalRatio;

        /// <summary>		
        /// The number of CCD rows that comprise a mask element.
        /// </summary>
        private ushort m_MaskVerticalRatio;

        /// <summary>
        /// The order of the row fit polynomial..
        /// N + 1 coefficients per binned image row.
        /// </summary>
        private byte m_RowFitPolynomialOrder;

        /// <summary>
        /// Array of length Mask Height x (RowFitPolynomialOrder + 1) coefficients
        /// stored by rows from lowest order to highest.
        /// </summary>
        private float[,] m_RowFitCoefficients;

        /// <summary>
        /// Minimum wave length supported by the instrument.
        /// </summary>
        private float m_MinimumWavelength;

        /// <summary>
        /// Maximum wave length supported by the instrument.
        /// </summary>
        private float m_MaximumWavelength;

        /// <summary>
        /// NIST intensity correction factors.
        /// </summary>
        private float[] m_NISTIntensityCorrectionFactors = null;


        /// <summary>
        /// A two dimensional array of MASK height rows by 2 columns.
        /// Each row contains the begin index and the end index of
        /// where we found the ith row of the mask on the CCD, via spatial correlation of Xenon pen
        /// lamp that is filtered with 882 bandpass filter.
        /// </summary>
        private ushort[,] m_MaskRows;

        #endregion // Private Members

        /* Don't needed currently
        /// <summary>
        /// Implements the CompareTo function of IComparable Interface
        /// </summary>
        /// <param name="otherCalibration">An object to compare with this object.</param>
        /// <returns>
        /// return value (objectsAreEqual) meaning 
        /// Less than zero 
        /// This object is less than the other parameter.            
        /// Zero 
        /// This object is equal to other.              
        /// Greater than zero 
        /// This object is greater than other. 
        ///</returns>
        public int CompareTo(ICalibration otherCalibration)
        {


            //Default to objects are indeed equal.
            int objectsAreEqual = 0;

            byte[] me_bytes = this.ToByteArray();
            byte[] other_bytes = otherCalibration.ToByteArray();

            if (other_bytes.Length != me_bytes.Length)
            {
                objectsAreEqual = (other_bytes.Length > me_bytes.Length) ? 1 : -1;
            }
            else
            {
                for (int i = 0; i < me_bytes.Length; i++)
                {
                    if (me_bytes[i] != other_bytes[i])
                    {
                        objectsAreEqual = (other_bytes[i] > me_bytes[i]) ? 1 : -1;
                        break;
                    }
                }
            }

            return objectsAreEqual;
        }
        */

        #region ICalibration Members

        /// <summary>
        /// See the ICalibration interface for a description of these members.
        /// </summary>

        #region Properties

        ///////////////////////////////////////////////////////////////////////
        /// <summary>
        /// Gets/Sets the schema of the USB Calibration command.
        /// </summary>
        public int Schema
        {
            get { return this.m_iSchema; }
            set { this.m_iSchema = value; }
        }



        ///////////////////////////////////////////////////////////////////
        /// <summary>
        /// Sets the name that should be associated with the calibration.
        /// This is a user specified name.
        /// </summary>
        public string Name
        {
            get { return this.m_Name; }
            set { this.m_Name = value; }
        }

        ///////////////////////////////////////////////////////////////////////
        /// <summary>
        /// Gets/Sets the array of observed pixel numbers with calibration spikes.
        /// </summary>
        public string SpectraSource
        {
            get { return this.m_spectraSource; }
            set { this.m_spectraSource = value; }
        }


        ///////////////////////////////////////////////////////////////////////
        /// <summary>
        /// Gets/Sets the array of actual wavelengths that are mapped to the
        /// observed pixel numbers.
        /// </summary>
        public float[] Wavelengths
        {
            get { return this.m_fActualWavelengths; }
            set { this.m_fActualWavelengths = value; }
        }


        ///////////////////////////////////////////////////////////////////////
        /// <summary>
        /// Gets/Sets the array of observed pixel numbers with calibration spikes.
        /// </summary>
        public float[] ObservedPixels
        {
            get { return this.m_fObservedPixels; }
            set { this.m_fObservedPixels = value; }
        }


        ///////////////////////////////////////////////////////////////////////
        /// <summary>
        /// Gets/Sets the number of source peaks that will be used for calibration.
        /// </summary>
        public ushort SourcePeaks
        {
            get { return this.m_uSourcePeaks; }
            set { this.m_uSourcePeaks = value; }
        }


        ///////////////////////////////////////////////////////////////////////
        /// <summary>
        /// Gets/Sets the polynomial order that should be used when 
        /// calculating wavelength using calibration data.
        /// </summary>
        public int PolynomialOrder
        {
            get { return this.m_iPolynomialOrder; }
            set { this.m_iPolynomialOrder = value; }
        }


        ///////////////////////////////////////////////////////////////////////
        /// <summary>
        /// Gets/Sets the number of dead (unusable) pixels on the left side of the CCD.
        /// </summary>
        public short DeadPixelsLeft
        {
            get { return this.m_nDeadPixelsLeft; }
            set { this.m_nDeadPixelsLeft = value; }
        }


        ///////////////////////////////////////////////////////////////////////
        /// <summary>
        /// Gets/Sets the number of dead (unusable) pixels on the right side of the CCD.
        /// </summary>
        public short DeadPixelsRight
        {
            get { return this.m_nDeadPixelsRight; }
            set { this.m_nDeadPixelsRight = value; }
        }


        //////////////////////////////////////////////////////////////////////////		
        /// <summary>
        /// Gets/Sets the binary reconstruction mask.
        /// </summary>
        public float[,] BinaryMask
        {
            get { return this.m_BinaryMask; }
            set { this.m_BinaryMask = value; }
        }


        /// <summary>
        /// Gets/Sets the gray scale reconstruction mask.
        /// </summary>
        public float[,] GrayScaleMask
        {
            get { return this.m_GrayScaleMask; }
            set { this.m_GrayScaleMask = value; }
        }


        ///////////////////////////////////////////////////////////////////////
        /// <summary>
        /// Gets/Sets the mask name that is to be used.
        /// </summary>
        public string MaskName
        {
            get { return this.m_szMaskName; }
            set { this.m_szMaskName = value; }
        }


        ///////////////////////////////////////////////////////////////////////
        /// <summary>
        /// Gets/Sets the pixel height of the reconstruction mask.
        /// </summary>
        public ushort MaskHeight
        {
            get { return this.m_MaskHeight; }
            set { this.m_MaskHeight = value; }
        }


        ///////////////////////////////////////////////////////////////////////
        /// <summary>
        /// Gets/Sets the pixel width of the reconstruction mask.
        /// </summary>
        public ushort MaskWidth
        {
            get { return this.m_MaskWidth; }
            set { this.m_MaskWidth = value; }
        }


        ///////////////////////////////////////////////////////////////////////
        /// <summary>
        /// Gets/Sets the horizontal mask ratio.  The horizontal mask ratio is the 
        /// number of CCD columns that comprise a mask element.
        /// </summary>
        public ushort MaskHorizontalRatio
        {
            get { return this.m_MaskHorizontalRatio; }
            set { this.m_MaskHorizontalRatio = value; }
        }


        ///////////////////////////////////////////////////////////////////////
        /// <summary>
        /// Gets/Sets the vertical mask ratio.  The vertical mask ratio is the 
        /// number of CCD rows that comprise a mask element.
        /// </summary>
        public ushort MaskVerticalRatio
        {
            get { return this.m_MaskVerticalRatio; }
            set { this.m_MaskVerticalRatio = value; }
        }


        ///////////////////////////////////////////////////////////////////////
        /// <summary>
        /// Gets/Sets a flag to determine if the mask is flipped in the 
        /// vertical direction or not.
        /// </summary>
        public bool MaskVertFlipped
        {
            get { return this.m_bMaskVertFlipped; }
            set { this.m_bMaskVertFlipped = value; }
        }


        ///////////////////////////////////////////////////////////////////////
        /// <summary>
        /// Gets/Sets a flag to determine if the mask is flipped in the 
        /// horizontal direction or not.
        /// </summary>
        public bool MaskHorzFlipped
        {
            get { return this.m_bMaskHorzFlipped; }
            set { this.m_bMaskHorzFlipped = value; }
        }


        ///////////////////////////////////////////////////////////////////////
        /// <summary>
        /// Gets/Sets a flag to determine if spectra data is lowest 
        /// to highest (false) or highest to lowest wavelength.
        /// </summary>        
        public bool SpectraFlipped
        {
            get { return this.m_bSpectraFlipped; }
            set { this.m_bSpectraFlipped = value; }
        }


        ///////////////////////////////////////////////////////////////////
        /// <summary>
        /// The mask indices used by the calibration.
        /// </summary>
        public int[] MaskIndices
        {
            get { Array.Sort(this.m_nMaskIndices); return this.m_nMaskIndices; }
            set { this.m_nMaskIndices = value; }
        }

        public ushort[,] MaskRows
        {
            get { return m_MaskRows; }
            set { m_MaskRows = value; }
        }

        ///////////////////////////////////////////////////////////////////////
        /// <summary>
        /// Gets/Sets the X alignment adjustment values.
        /// </summary>		
        public short[] MaskXAlignAdjust
        {
            get { return this.m_nMaskXAlignAdjust; }
            set { this.m_nMaskXAlignAdjust = value; }
        }


        ///////////////////////////////////////////////////////////////////////
        /// <summary>
        /// Gets/Sets the Y alignment adjustment values.
        /// </summary>
        public short[] MaskYAlignAdjust
        {
            get { return this.m_nMaskYAlignAdjust; }
            set { this.m_nMaskYAlignAdjust = value; }
        }


        ///////////////////////////////////////////////////////////////////////
        /// <summary>
        /// Gets/Sets the mask reconstruction shift.  The reconstruction shift 
        /// determines the number of pixels to shift reconstructed data in order
        /// to do valid summing of indice data.
        /// </summary>
        public short ReconstructionShift
        {
            get { return this.m_nReconShift; }
            set { this.m_nReconShift = value; }
        }


        ///////////////////////////////////////////////////////////////////////
        /// <summary>
        /// Gets/Sets the array of peak shifts that should be used to align
        /// spectra.
        /// </summary>
        public short[] Shifts
        {
            get { return this.m_nPeakShifts; }
            set { this.m_nPeakShifts = value; }
        }


        ///////////////////////////////////////////////////////////////////////
        /// <summary>
        /// Gets/Sets the serial number of the spectrometer that the calibration
        /// is for.
        /// </summary>
        public string SerialNumber
        {
            get { return this.m_serialNumber; }
            set { this.m_serialNumber = value; }
        }


        //////////////////////////////////////////////////////////////////////////
        /// <summary>
        /// The order of the row fit polynomial..
        /// N + 1 coefficients per binned image row.
        /// </summary>
        public byte RowFitPolynomialOrder
        {
            get { return this.m_RowFitPolynomialOrder; }
            set { this.m_RowFitPolynomialOrder = value; }
        }


        //////////////////////////////////////////////////////////////////////////		
        /// <summary>
        /// Array of length Mask Height x (RowFitPolynomialOrder + 1) coefficients
        /// stored by rows from lowest order to highest.
        /// </summary>
        public float[,] RowFitCoefficients
        {
            get { return this.m_RowFitCoefficients; }
            set { this.m_RowFitCoefficients = value; }
        }


        //////////////////////////////////////////////////////////////////////////		
        /// <summary>
        /// Gets/sets the minimum wave length supported by the instrument.
        /// </summary>
        public float MinimumWavelength
        {
            get { return this.m_MinimumWavelength; }
            set { this.m_MinimumWavelength = value; }
        }


        //////////////////////////////////////////////////////////////////////////
        /// <summary>
        /// Gets/Sets the maximum wavelength supported by the instrument.
        /// </summary>
        public float MaximumWavelength
        {
            get { return this.m_MaximumWavelength; }
            set { this.m_MaximumWavelength = value; }
        }


        //////////////////////////////////////////////////////////////////////////		
        /// <summary>
        /// Gets/Sets NIST intensity correction factors.
        /// </summary>
        public float[] NISTIntensityCorrectionFactors
        {
            get
            {
                return m_NISTIntensityCorrectionFactors;
            }
            set
            {
                m_NISTIntensityCorrectionFactors = value;
            }
        }


        //////////////////////////////////////////////////////////////////////////		
        /// <summary>
        /// Determines if the calibration is valid or not.
        /// </summary>
        public bool IsValid
        {
            get
            {
                // TBD:  Add other criteria to determine if the calibration is valid.
                bool isValid = false;
                lock (oSyncRoot)
                    isValid = (!m_bSuspended) &&
                    (MaskRows != null && MaskRows.Length > 0) &&
                    ((this.RowFitCoefficients != null) &&
                    (this.RowFitCoefficients.Rank == 2) &&
                    (this.RowFitCoefficients.GetLength(0) > 0) &&
                    (this.RowFitCoefficients.GetLength(1) > 0) &&
                    (this.RowFitCoefficients[0, 0] > Single.Epsilon ||
                     this.RowFitCoefficients[0, 0] < -(Single.Epsilon)));
                return isValid;
            }
        }
        /// <summary>
        /// Allows to temporary disable calibration from being used by spectrometer
        /// </summary>
        private bool m_bSuspended = false;

        /// <summary>
        /// Is true when calibration data disabled by Suspend()
        /// </summary>
        public bool Suspended
        {
            get
            {
                lock (oSyncRoot)
                    return m_bSuspended;
            }
        }
        /// <summary>
        /// Invalidates configuration untill resume is called
        /// </summary>
        public void Suspend()
        {
            lock (oSyncRoot)
                m_bSuspended = true;
        }
        public void Resume()
        {
            lock (oSyncRoot)
                m_bSuspended = false;
        }

        ///////////////////////////////////////////////////////////////////////
        /// <summary>
        /// Gets/Sets the number of valid entries in the calibration data.
        /// </summary>
        public int NumEntries
        {
            get
            {
                return m_iEntries;
            }
            set
            {
                m_iEntries = value;
            }
        }

        #endregion // Properties


        //////////////////////////////////////////////////////////////////////////		
        /// <summary>
        /// Converts the calibration object to a byte array.
        /// </summary>
        /// <returns>A byte array that represents a calibration object.</returns>
        public byte[] ToByteArray()
        {
            int iLength = 0;
            //Write the binary cal file header.
            System.IO.BinaryWriter writer = new System.IO.BinaryWriter(new System.IO.MemoryStream());
            writer.Write(Calibration.m_FileHeader);

            // Version number
            writer.Write(Schema);

            // Mask Height (16-bit unsigned integer)
            writer.Write(this.MaskHeight);

            // Mask Width (16-bit unsigned integer)
            writer.Write(this.MaskWidth);

            // Mask Vertical Ratio (16-bit unsigned integer)
            writer.Write(this.MaskVerticalRatio);

            // Mask Horizontal Ratio (16-bit unsigned integer)
            writer.Write(this.MaskHorizontalRatio);

            // Mask Flipped (ushort: 0x00 = no flip, 0x01 = vert, 0x02 = horz, 0x03 = horz & vert)
            byte bFlipped = 0;
            if (this.MaskVertFlipped)
                bFlipped |= VERT_MASK_FLIP;
            if (this.MaskHorzFlipped)
                bFlipped |= HORZ_MASK_FLIP;
            writer.Write(bFlipped);

            // Spectra Flipped (boolean: 0x01 = True, 0x00 = False)
            writer.Write(this.SpectraFlipped ? (byte)0x01 : (byte)0x00);

            // Mask Indices (32-bit integer array – see MaskHeight)
            iLength = this.MaskIndices.GetLength(0);
            if (iLength > 0)
            {
                for (int i = 0; i < iLength; i++)
                    writer.Write((int)this.MaskIndices[i]);
            }
            //MaskIndices never included a length field 
            //and since we no longer use the MaskIndices
            // (using MaskRows now)
            //we are forced to always include it even if empty
            //simply for backward compatibility.
            else
            {
                for (int i = 0; i < MaskHeight; i++)
                    writer.Write((int)0);
            }

            // Number of X Alignments (16-bit unsigned integer)
            iLength = 0;
            if (this.MaskXAlignAdjust != null)
                iLength = this.MaskXAlignAdjust.GetLength(0);
            writer.Write((ushort)iLength);

            // Mask X Alignment Adjustments (16-bit integer array)
            for (int i = 0; i < iLength; i++)
                writer.Write(this.MaskXAlignAdjust[i]);

            // Number of Y Alignments (16-bit unsigned integer)			
            iLength = 0;
            if (this.MaskYAlignAdjust != null)
                iLength = this.MaskYAlignAdjust.GetLength(0);
            writer.Write((ushort)iLength);

            // Mask Y Alignment Adjustments (16-bit integer array)							
            for (int i = 0; i < iLength; i++)
                writer.Write(this.MaskYAlignAdjust[i]);

            // Source Peaks (16-bit unsigned integer)            			
            writer.Write(this.SourcePeaks);

            // Shifts (16-bit integer array – see Required Mask Indices)
            iLength = 0;
            if (this.Shifts != null)
                iLength = this.Shifts.GetLength(0);
            for (int i = 0; i < MaskHeight; i++)
            {
                if (i < iLength)
                    writer.Write(this.Shifts[i]);
                else
                    writer.Write((short)0);
            }

            // Polynomial Order (32-bit integer)
            writer.Write(this.PolynomialOrder);

            // Number of Entries (32-bit integer)
            int numEntries = 0;
            if (this.Wavelengths != null)
                numEntries = this.Wavelengths.GetLength(0);
            writer.Write(numEntries);
            if (numEntries > 0)
            {
                // Observed Pixels (32-bit float array – see Number of Entries)
                iLength = 0;
                if (this.ObservedPixels != null)
                    iLength = this.ObservedPixels.GetLength(0);
                for (int i = 0; i < numEntries; i++)
                {
                    if (i < iLength)
                        writer.Write(this.ObservedPixels[i]);
                    else
                        writer.Write((float)0.0F);
                }

                // Actual Wavelengths (32-bit float array – see Number of Entries)
                iLength = 0;
                if (this.Wavelengths != null)
                    iLength = this.Wavelengths.GetLength(0);
                for (int i = 0; i < numEntries; i++)
                {
                    if (i < iLength)
                        writer.Write(this.Wavelengths[i]);
                    else
                        writer.Write((float)0.0F);
                }
            }
            // Dead Pixels Left (16-bit unsigned integer)
            writer.Write(this.DeadPixelsLeft);

            // Dead Pixels Right (16-bit unsigned integer)
            writer.Write(this.DeadPixelsRight);

            // Calibration Name - Length Prefixed String
            WriteString(this.Name, writer);

            // Mask Name - Length Prefixed String
            WriteString(this.MaskName, writer);

            // Calibration Spectra Source - Length Prefixed String
            WriteString(this.SpectraSource, writer);

            // Serial Number - Length Prefixed String
            WriteString(this.SerialNumber, writer);

            // Binary Mask (32-bit 2-D float array – see MaskWidth and MaskHeight)			
            for (int i = 0; i < this.MaskHeight; i++)
            {
                for (int j = 0; j < this.MaskWidth; j++)
                {
                    if (this.BinaryMask != null)
                        writer.Write(this.BinaryMask[i, j]);
                    else
                        writer.Write((float)0.0F);
                }
            }

            // Gray Scale Mask (32-bit 2-D float array – see MaskWidth and MaskHeight)       
            for (int i = 0; i < this.MaskHeight; i++)
            {
                for (int j = 0; j < this.MaskWidth; j++)
                {
                    if (this.GrayScaleMask != null)
                        writer.Write(this.GrayScaleMask[i, j]);
                    else
                        writer.Write((float)0.0F);
                }
            }

            // Row Polynomial Fit Order
            writer.Write(this.RowFitPolynomialOrder);

            // Row Polynomial Coefficients
            for (int i = 0; i < this.MaskHeight; i++)
            {
                for (int j = 0; j < this.RowFitPolynomialOrder + 1; j++)
                {
                    if (this.RowFitCoefficients != null)
                        writer.Write(this.RowFitCoefficients[i, j]);
                    else
                        writer.Write((float)1.0F);
                }
            }

            // Minimum supported wavelength
            writer.Write(this.MinimumWavelength);

            // Maximum supported wavelength
            writer.Write(this.MaximumWavelength);

            // NIST 2241 Intensity Correction factors.
            iLength = 0;
            if (this.NISTIntensityCorrectionFactors != null)
                iLength = this.NISTIntensityCorrectionFactors.GetLength(0);
            writer.Write((ushort)iLength);
            for (int i = 0; i < iLength; i++)
                writer.Write(this.NISTIntensityCorrectionFactors[i]);

            // Mask Rows (Alternate expression of Indices)
            // Used in updated Row Binning algorithm
            for (int i = 0; i < this.MaskRows.GetLength(0); i++)
            {
                //Second dimension len, should always be 2 even though
                //we are dynamically reading it here.
                System.Diagnostics.Trace.Assert(MaskRows.GetLength(1) == 2);
                for (int j = 0; j < this.MaskRows.GetLength(1); j++)
                {
                    writer.Write(this.MaskRows[i, j]);
                }
            }


            return ((System.IO.MemoryStream)writer.BaseStream).ToArray();
        }


        //////////////////////////////////////////////////////////////////////////		
        /// <summary>
        /// Writes a string to the specified binary writer stream.
        /// </summary>
        /// <param name="data">The string to be written.</param>
        /// <param name="writer">The binary writer steam to write to.</param>
        private void WriteString(string data, System.IO.BinaryWriter writer)
        {
            // Get the size of the string including null.
            ushort stringLength = TerminatorSize(data);

            // Write the string length.
            writer.Write(stringLength);

            // Convert the string data to ascii byte formated data.
            byte[] stringData = System.Text.Encoding.ASCII.GetBytes(data);
            writer.Write(stringData);

            // Pad data so it ends on an even byte boundary.
            for (int i = 0; i < (stringLength - stringData.Length); i++)
                writer.Write((byte)0);
        }


        ///////////////////////////////////////////////////////////////////
        /// <summary>
        /// Gets the string length of the given string plus a null terminator.
        /// If the resulting string length is odd, then the string length is 
        /// incremented by one.
        /// </summary>
        private ushort TerminatorSize(string str)
        {
            ushort uSize = 0;

            // Determine if we have a valid string.
            if (!string.IsNullOrEmpty(str))
            {
                // Get the size of the string including a null character.
                uSize = (ushort)(str.Length + 1);

                if (0 != uSize % 2)
                    uSize++;
            }

            return uSize;
        }


        //////////////////////////////////////////////////////////////////////////		
        /// <summary>
        /// Initializes a calibration object with data from a byte array 
        /// representation of a calibration object.
        /// </summary>
        /// <param name="data">The byte array to be processed.</param>
        /// <remarks>This methods functionality is controlled by the currently 
        /// configured schema value.</remarks>
        public void FromByteArray(byte[] data)
        {
            System.IO.BinaryReader reader = new System.IO.BinaryReader(new System.IO.MemoryStream(data));

            bool validHeader = true;
            byte[] streamHeader = reader.ReadBytes(m_FileHeader.Length);
            for (int i = 0; i < Calibration.m_FileHeader.Length; i++)
            {
                if (streamHeader[i] != Calibration.m_FileHeader[i])
                {
                    validHeader = false;
                    break;
                }
            }

            if (validHeader)
            {
                this.Schema = reader.ReadInt32();
            }
            //See if it is cal version "alpha" that came before we
            //started serializing a byte stream ID header.
            else
            {
                MemoryStream memStream = reader.BaseStream as MemoryStream;
                memStream.Position = 0;
            }

            // Mask Height (16-bit unsigned integer)
            this.MaskHeight = reader.ReadUInt16();

            // Mask Width (16-bit unsigned integer)
            this.MaskWidth = reader.ReadUInt16();

            // Mask Vertical Ratio (16-bit unsigned integer)
            this.MaskVerticalRatio = reader.ReadUInt16();

            // Mask Horizontal Ratio (16-bit unsigned integer)
            this.MaskHorizontalRatio = reader.ReadUInt16();

            if (!validHeader)
            {
                if (MaskHeight == 64 &&
                    MaskHeight == 64 &&
                    MaskVerticalRatio == 4 &&
                    MaskHorizontalRatio == 4)
                {
                    //Assume good Alpha calibration file
                    Schema = 8;
                }
                else
                {
                    throw new ArgumentException("Invalid calibration byte stream", "data");
                }
            }

            // Mask Flipped (ushort: 0x00 = no flip, 0x01 = vert, 0x02 = horz, 0x03 = horz & vert)
            byte maskFlip = reader.ReadByte();
            this.MaskVertFlipped = (maskFlip & 0x01) == 0x01;
            this.MaskHorzFlipped = (maskFlip & 0x02) == 0x02;

            // Spectra Flipped (boolean: 0x01 = True, 0x00 = False)
            this.SpectraFlipped = 0x01 == reader.ReadByte();

            // Mask Indices (32-bit integer array – see Required Mask Indices)
            List<int> valueList = new List<int>();
            for (int i = 0; i < this.MaskHeight; i++)
                valueList.Add(reader.ReadInt32());
            this.MaskIndices = valueList.ToArray();

            // Number of X Alignments (16-bit unsigned integer)
            ushort nXAlignmentCount = reader.ReadUInt16();
            if (nXAlignmentCount > 0)
            {
                // Mask X Alignment Adjustments (16-bit  integer array – see CCD height)			
                this.MaskXAlignAdjust = new short[nXAlignmentCount];
                for (int i = 0; i < nXAlignmentCount; i++)
                    this.MaskXAlignAdjust[i] = reader.ReadInt16();
            }

            // Number of Y Alignments (16-bit unsigned integer)
            ushort nYAlignmentCount = reader.ReadUInt16();
            if (nYAlignmentCount > 0)
            {
                // Mask X Alignment Adjustments (16-bit  integer array – see CCD width)
                this.MaskYAlignAdjust = new short[nYAlignmentCount];
                for (int i = 0; i < nYAlignmentCount; i++)
                    this.MaskYAlignAdjust[i] = reader.ReadInt16();
            }

            // Source Peaks (16-bit unsigned integer)
            this.SourcePeaks = reader.ReadUInt16();
            if (SourcePeaks > 0)
            {
                // Shifts (16-bit integer array – see MaskHeight)
                this.Shifts = new short[this.MaskHeight];
                for (int i = 0; i < this.MaskHeight; i++)
                    this.Shifts[i] = reader.ReadInt16();
            }

            // Polynomial Order (32-bit integer)
            this.PolynomialOrder = reader.ReadInt32();

            // Number of Entries (32-bit integer)
            int numEntries = reader.ReadInt32();
            if (numEntries > 0)
            {
                // Observed Pixels (32-bit float array – see Number of Entries)
                this.ObservedPixels = new float[numEntries];
                for (int i = 0; i < numEntries; i++)
                    this.ObservedPixels[i] = reader.ReadSingle();

                // Wavelengths (32-bit float array – see Number of Entries)
                this.Wavelengths = new float[numEntries];
                for (int i = 0; i < numEntries; i++)
                    this.Wavelengths[i] = reader.ReadSingle();
            }

            // Dead Pixels Left (16-bit unsigned integer)
            this.DeadPixelsLeft = reader.ReadInt16();

            // Dead Pixels Right (16-bit unsigned integer)
            this.DeadPixelsRight = reader.ReadInt16();

            // Name – string size (16-bit unsigned integer)
            ushort nStringLen = reader.ReadUInt16();

            // Name – string (byte array with null terminator)
            this.Name = Encoding.ASCII.GetString(reader.ReadBytes(nStringLen),
                    0, nStringLen).Trim(new char[] { '\0' });

            // Mask – string size (16-bit unsigned integer)
            nStringLen = reader.ReadUInt16();

            // Mask – string (byte array with null terminator)
            this.MaskName = Encoding.ASCII.GetString(
                reader.ReadBytes(nStringLen), 0, nStringLen).Trim(new char[] { '\0' });

            // Spectral Source – string size (16-bit unsigned integer)
            nStringLen = reader.ReadUInt16();

            // Spectral Source (byte array with null terminator)
            this.SpectraSource = Encoding.ASCII.GetString(
                reader.ReadBytes(nStringLen), 0, nStringLen).Trim(new char[] { '\0' });

            // Serial Number – string size (16-bit unsigned integer)
            nStringLen = reader.ReadUInt16();

            // Serial Number (byte array with null terminator)		
            this.SerialNumber = Encoding.ASCII.GetString(
                reader.ReadBytes(nStringLen), 0, nStringLen).Trim(new char[] { '\0' });

            // Processing of gray scale and binary mask calibration data.
            // Binary Mask Elements (32-bit float array – see Number of MaskHeight and MaskWidth)
            this.BinaryMask = new float[this.MaskHeight, this.MaskWidth];
            for (int i = 0; i < this.MaskHeight; i++)
            {
                for (int j = 0; j < this.MaskWidth; j++)
                {
                    this.BinaryMask[i, j] = reader.ReadSingle();
                }
            }

            // Gray Scale Mask Elements (32-bit float array – see Number of MaskHeight and MaskWidth)
            this.GrayScaleMask = new float[this.MaskHeight, this.MaskWidth];
            for (int i = 0; i < this.MaskHeight; i++)
            {
                for (int j = 0; j < this.MaskWidth; j++)
                {
                    this.GrayScaleMask[i, j] = reader.ReadSingle();
                }
            }


            // Row Polynomial Fit Order
            this.RowFitPolynomialOrder = reader.ReadByte();

            this.RowFitCoefficients = new float[this.MaskHeight, this.RowFitPolynomialOrder + 1];
            // Row Polynomial Coefficients
            for (int i = 0; i < this.MaskHeight; i++)
            {
                for (int j = 0; j < this.RowFitPolynomialOrder + 1; j++)
                {
                    this.RowFitCoefficients[i, j] = reader.ReadSingle();
                }
            }

            // Minimum supported wavelength
            this.MinimumWavelength = reader.ReadSingle();

            // Maximum supported wavelength
            this.MaximumWavelength = reader.ReadSingle();

            // NIST 2241 Intensity Correction factors.
            // TODO: Again we need to fix the fact that the correction buffer
            // is defined in terms of the CCD Width and that information is
            // not a part of the calibration or configuration file.
            ushort bufferSize = reader.ReadUInt16();
            NISTIntensityCorrectionFactors = new float[bufferSize];
            for (ushort i = 0; i < bufferSize; i++)
                NISTIntensityCorrectionFactors[i] = reader.ReadSingle();

            ushort[,] maskRowsBuffer = new ushort[MaskHeight, 2];
            if (Schema == 8)
            {
                CreateMaskElementRowLocationsFromMaskIndices();
                UpdateHorizontalAlignment();
                Schema = 9;
            }
            //I shot myself in the foot here, by not including the length in the encoding
            //protocol.  For now handle a calibration that has not been completed using
            //this length check.
            else if (reader.BaseStream.Length > reader.BaseStream.Position)
            {
                //Initialize based on where each line was found by the spatial correlation
                //during the CRE calibration of MaskCurvature correction.
                for (ushort i = 0; i < MaskHeight; i++)
                {
                    maskRowsBuffer[i, 0] = reader.ReadUInt16();
                    maskRowsBuffer[i, 1] = reader.ReadUInt16();
                }
                MaskRows = maskRowsBuffer;
            }
            else
            {
                MaskRows = new ushort[0, 2];
            }

            if (string.IsNullOrEmpty(this.Name))
                this.Name = this.SerialNumber;
        }


        void CreateMaskElementRowLocationsFromMaskIndices()
        {
            ushort[,] maskRowsBuffer = new ushort[MaskHeight, 2];
            //Initialize based on the MaskIndices of previous calibration method.
            if (MaskIndices != null && MaskIndices.Length == MaskHeight)
            {
                for (ushort i = 0; i < MaskHeight; i++)
                {
                    if (m_bMaskVertFlipped)
                    {
                        maskRowsBuffer[i, 0] = (ushort)(this.MaskIndices[i] + 1);
                        maskRowsBuffer[i, 1] = (ushort)(maskRowsBuffer[i, 0] + this.MaskVerticalRatio - 1);
                    }
                    else
                    {
                        maskRowsBuffer[i, 0] = (ushort)(this.MaskIndices[i] - this.MaskVerticalRatio);
                        maskRowsBuffer[i, 1] = (ushort)(maskRowsBuffer[i, 0] + this.MaskVerticalRatio - 1);
                    }
                }
            }
            MaskRows = maskRowsBuffer;
        }

        void UpdateHorizontalAlignment()
        {
            short minShift = short.MaxValue;

            for (int i = 0; i < MaskXAlignAdjust.Length; i++)
            {
                if (i < m_MaskRows[0, 0] || m_MaskRows[m_MaskRows.GetLength(0) - 1, 1] < i)
                {
                    m_nMaskXAlignAdjust[i] = 0;
                }
                else //Find the minimum shift in the region of the mask.
                {
                    if (m_nMaskXAlignAdjust[i] < minShift)
                    {
                        minShift = m_nMaskXAlignAdjust[i];
                    }

                }
            }

            if (minShift < 0)
            {
                throw new ApplicationException("Invalid Curvature shift array");
            }

            //Update the shift buffer to minimize the shift to only
            //what is necessary to align the mask.
            for (int i = m_MaskRows[0, 0];
                i <= m_MaskRows[m_MaskRows.GetLength(0) - 1, 1];
                i++)
            {
                m_nMaskXAlignAdjust[i] -= minShift;
            }
        }


        #endregion

        #region SerializationMethods

        ///////////////////////////////////////////////////////////////////////
        /// <summary>
        /// Constructs a calibration object from a stream of serialized data.
        /// </summary>
        /// <param name="info">The serialized data.</param>
        /// <param name="context">The context of the serialized data.</param>
        protected Calibration(SerializationInfo info, StreamingContext context)
        {
            // Process based on the object's schema value.
            // This is the class schema for file serialization
            int iSchema = info.GetInt32("m_iSchema");

            switch (iSchema)
            {

                case 9:
                    MaskRows = (ushort[,])info.GetValue("m_MaskRows", typeof(ushort[,]));
                    goto case 8;
                case 8: // Schema 8 
                    // This is the USB Device Cmd Schema. See Clear Shot II(I) 
                    // USB Port Interface Communications and Control Information
                    // Specification.
                    Name = info.GetString("m_Name");
                    RowFitPolynomialOrder = info.GetByte("m_RowFitPolynomialOrder");
                    RowFitCoefficients = (float[,])info.GetValue("m_RowFitCoefficients", typeof(float[,]));
                    MaximumWavelength = info.GetSingle("m_MaxWaveLength");
                    MinimumWavelength = info.GetSingle("m_MinWaveLength");
                    NISTIntensityCorrectionFactors = (float[])info.GetValue("m_NISTIntensityCorrectionFactors", typeof(float[]));
                    MaskWidth = info.GetUInt16("m_MaskWidth");
                    MaskHeight = info.GetUInt16("m_MaskHeight");
                    MaskVerticalRatio = info.GetUInt16("m_MaskVerticalRatio");
                    MaskHorizontalRatio = info.GetUInt16("m_MaskHorizontalRatio");
                    BinaryMask = (float[,])info.GetValue("m_BinaryMask", typeof(float[,]));
                    GrayScaleMask = (float[,])info.GetValue("m_GrayScaleMask", typeof(float[,]));
                    MaskHorzFlipped = info.GetBoolean("m_bMaskHorzFlipped");
                    DeadPixelsLeft = (short)info.GetUInt16("m_nDeadPixelsLeft");
                    DeadPixelsRight = (short)info.GetUInt16("m_nDeadPixelsRight");
                    MaskName = info.GetString("m_szMask");
                    MaskVertFlipped = info.GetBoolean("m_bMaskFlipped");
                    SpectraFlipped = info.GetBoolean("m_bSpectraFlipped");
                    MaskIndices = info.GetValue("m_nMaskIndices", typeof(int[])) as int[];
                    MaskXAlignAdjust = (short[])info.GetValue("m_nMaskXAlignAdjust", typeof(short[]));
                    MaskYAlignAdjust = (short[])info.GetValue("m_nMaskYAlignAdjust", typeof(short[]));
                    SourcePeaks = info.GetUInt16("m_uSourcePeaks");
                    Shifts = (short[])info.GetValue("m_nPeakShifts", typeof(short[]));
                    PolynomialOrder = (int)info.GetInt32("m_iPolynomialOrder");
                    SpectraSource = (string)info.GetString("m_spectraSource");
                    SerialNumber = (string)info.GetString("m_serialNumber");
                    NumEntries = (int)info.GetInt32("m_iEntries");
                    ObservedPixels = (float[])info.GetValue("m_fObservedPixels", typeof(float[]));
                    Wavelengths = (float[])info.GetValue("m_fActualWavelengths", typeof(float[]));
                    if (iSchema < 9)
                    {
                        CreateMaskElementRowLocationsFromMaskIndices();
                        UpdateHorizontalAlignment();
                        Schema = 9;
                    }
                    else
                    {
                        Schema = iSchema;
                    }


                    break;

                default:
                    throw new SerializationException("Calibration::Calibration - Schema not supported");
            }

        }


        ///////////////////////////////////////////////////////////////////////
        /// <summary>
        /// Serializes a calibration object.
        /// </summary>
        /// <param name="info">The serialized data.</param>
        /// <param name="context">The context of the serialized data.</param>
        [SecurityPermissionAttribute(SecurityAction.Demand, SerializationFormatter = true)]
        public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            // Store the version of this object.
            info.AddValue("m_iSchema", Schema);
            // Version 9.
            info.AddValue("m_MaskRows", this.MaskRows, typeof(ushort[,]));
            // Version 8.
            info.AddValue("m_Name", Name);
            info.AddValue("m_RowFitPolynomialOrder", RowFitPolynomialOrder);
            info.AddValue("m_RowFitCoefficients", RowFitCoefficients, typeof(float[,]));
            info.AddValue("m_MaxWaveLength", MaximumWavelength);
            info.AddValue("m_MinWaveLength", MinimumWavelength);
            info.AddValue("m_NISTIntensityCorrectionFactors", NISTIntensityCorrectionFactors, typeof(float[]));
            info.AddValue("m_MaskWidth", MaskWidth);
            info.AddValue("m_MaskHeight", MaskHeight);
            info.AddValue("m_MaskVerticalRatio", MaskVerticalRatio);
            info.AddValue("m_MaskHorizontalRatio", MaskHorizontalRatio);
            info.AddValue("m_BinaryMask", this.BinaryMask, typeof(float[,]));
            info.AddValue("m_GrayScaleMask", this.GrayScaleMask, typeof(float[,]));
            info.AddValue("m_bMaskHorzFlipped", MaskHorzFlipped);
            info.AddValue("m_nDeadPixelsLeft", DeadPixelsLeft);
            info.AddValue("m_nDeadPixelsRight", DeadPixelsRight);
            info.AddValue("m_szMask", MaskName);
            info.AddValue("m_bMaskFlipped", MaskVertFlipped);
            info.AddValue("m_bSpectraFlipped", SpectraFlipped);
            info.AddValue("m_nMaskIndices", MaskIndices, typeof(ArrayList));
            info.AddValue("m_nMaskXAlignAdjust", MaskXAlignAdjust, typeof(short[]));
            info.AddValue("m_nMaskYAlignAdjust", MaskYAlignAdjust, typeof(short[]));
            info.AddValue("m_uSourcePeaks", SourcePeaks);
            info.AddValue("m_nPeakShifts", Shifts, typeof(short[]));
            info.AddValue("m_iPolynomialOrder", PolynomialOrder);
            info.AddValue("m_spectraSource", SpectraSource);
            info.AddValue("m_serialNumber", SerialNumber);
            info.AddValue("m_iEntries", NumEntries);
            info.AddValue("m_fObservedPixels", ObservedPixels, typeof(float[]));
            info.AddValue("m_fActualWavelengths", Wavelengths, typeof(float[]));
        }
        #endregion

        #region FileAccess

        ///////////////////////////////////////////////////////////////////////
        /// <summary>
        /// Loads the specified calibration file.
        /// </summary>
        /// <param name="szFilename">The calibration file to load.</param>
        /// <param name="calibration">The calibration object that will hold the loaded calibration data.</param>
        /// <returns>Returns true if the wavelength calibration data was loaded without error.</returns>
        static public bool Load(string szFilename, ref Calibration calibration)
        {
            bool bResult = false;
            calibration = null;

            try
            {
                using (Stream stream = new FileStream(szFilename, FileMode.Open, FileAccess.Read, FileShare.Read))
                {
                    bResult = Load(stream, ref calibration);
                }
            }
            catch (Exception e)
            {
                calibration = null;
                Trace.WriteLine("Calibration::Load - " + e.ToString());
            }

            return bResult;
        }


        ///////////////////////////////////////////////////////////////////////
        /// <summary>
        /// Loads the specified calibration file.
        /// </summary>
        /// <param name="stream">The serialized calibration stream to load.</param>
        /// <param name="calibration">The calibration object that will hold the loaded calibration data.</param>
        /// <returns>Returns true if the wavelength calibration data was loaded without error.</returns>
        static public bool Load(Stream stream, ref Calibration calibration)
        {
            bool bResult = false;
            calibration = null;

            try
            {
                // Read a saved camera image from the specified file.
                BinaryFormatter formatter = new BinaryFormatter();
                formatter.AssemblyFormat = System.Runtime.Serialization.Formatters.FormatterAssemblyStyle.Simple;
                formatter.Binder = new CalibrationBinder();
                calibration = (Calibration)formatter.Deserialize(stream);

                // Success.
                bResult = true;
            }
            catch (Exception e)
            {
                calibration = null;
                Trace.WriteLine("Calibration::Load - " + e.ToString());
            }

            return bResult;
        }


        ///////////////////////////////////////////////////////////////////////
        /// <summary>
        /// Saves the currently defined wavelength calibration to the specified file.
        /// </summary>
        /// <param name="szFilename">Path of where the calibration data should be saved to.</param>
        /// <param name="calibration">The calibration object to be saved.</param>
        /// <returns>Returns true if the wavelength calibration data was saved to the 
        /// specified file without error.</returns>
        static public bool Save(string szFilename, Calibration calibration)
        {
            bool bResult = false;
            Stream stream = null;

            try
            {
                // Write the calibration data to disk.
                BinaryFormatter formatter = new BinaryFormatter();
                formatter.AssemblyFormat = System.Runtime.Serialization.Formatters.FormatterAssemblyStyle.Simple;
                stream = new FileStream(szFilename, FileMode.Create, FileAccess.Write, FileShare.None);
                formatter.Serialize(stream, calibration);

                // Success.
                bResult = true;
            }
            catch (Exception e)
            {
                Trace.WriteLine("Calibration::Save - " + e.ToString());
            }
            finally
            {
                // Close our file if it is open.
                if (stream != null)
                    stream.Close();
            }
            return bResult;
        }

        #endregion
    }
}
