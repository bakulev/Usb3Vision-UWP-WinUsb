using System;
using System.Text;

namespace CodaDevices.Spectrometry.Spectrometers
{
    public partial class FileSpectrometer
    {
        //TODO This class duplicates class Calibration. Only one should remain.
        public class CalibrationBinary
        {

            #region Schema property

            private int _schema; // Configuration Schema

            public int Schema
            {
                get { return _schema; }
                set { _schema = value; }
            }

            #endregion

            #region CCDPixelBPP property

            private byte _ccdPixelBpp; // CCD Bits Per Pixel

            public byte CCDPixelBPP
            {
                get { return _ccdPixelBpp; }
                set { _ccdPixelBpp = value; }
            }

            #endregion

            #region CCDWidth property

            private uint _ccdWidth; // CCD Total Width in columns.

            public uint CCDWidth
            {
                get { return _ccdWidth; }
                set { _ccdWidth = value; }
            }

            #endregion

            #region CCDHeight property

            private uint _ccdHeight; // CCD Total Height in rows.

            public uint CCDHeight
            {
                get { return _ccdHeight; }
                set { _ccdHeight = value; }
            }

            #endregion

            #region CCDLeft property

            private uint _ccdLeft; // Horizontal index of first non-dead CCD column.

            public uint CCDLeft
            {
                get { return _ccdLeft; }
                set { _ccdLeft = value; }
            }

            #endregion

            #region CCDTop property

            private uint _ccdTop; // Vertical index of first non-dead CCD row.

            public uint CCDTop
            {
                get { return _ccdTop; }
                set { _ccdTop = value; }
            }

            #endregion

            #region CCDRight property

            private uint _ccdRight; // Horizontal index of last non-dead CCD column.

            public uint CCDRight
            {
                get { return _ccdRight; }
                set { _ccdRight = value; }
            }

            #endregion

            #region CCDBottom property

            private uint _ccdBottom; // Vertical index of last non-dead CCD row.

            public uint CCDBottom
            {
                get { return _ccdBottom; }
                set { _ccdBottom = value; }
            }

            #endregion

            #region Mask property

            private double[,] _adMask;

            public double[,] Mask
            {
                get { return _adMask; }
                set { _adMask = value; }
            }

            #endregion

            #region CorrectPolynom property

            private double[,] _adCorrectPolynom;

            // Return the polynomial order of the row calibration curve fits
            // Set the polynomial order of the row calibration curve fits
            // Return the Horizontal Dispersion Correction Fit Coefficients
            public double[,] CorrectPolynom
            {
                get { return _adCorrectPolynom; }
                set { _adCorrectPolynom = value; }
            }

            #endregion

            #region MinimumWavelength 

            private double _minWavelength;

            // Return the minimum configured wavelength
            public double MinimumWavelength
            {
                get { return _minWavelength; }
                set { _minWavelength = value; }
            }

            #endregion

            #region MaximumWavelength 

            private double _maxWavelength;

            public double MaximumWavelength
            {
                get { return _maxWavelength; }
                set { _maxWavelength = value; }
            }

            #endregion

            #region MaskModel property

            // Mask Class Name
            private string _strMaskModel;

            public string MaskModel
            {
                get { return _strMaskModel; }
                set { _strMaskModel = value; }
            }

            #endregion

            #region InstrumentName property

            // Instrument Name
            private string _strName;

            public string InstrumentName
            {
                get { return _strName; }
                set { _strName = value; }
            }

            #endregion

            #region InstrumentSerialNumber property

            // Instrument Serial Number
            private string _strSerial;

            public string InstrumentSerialNumber
            {
                get { return _strSerial; }
                set { _strSerial = value; }
            }

            #endregion

            #region MaskHorizontalRatio property

            // Number of pixels per mask element in the horizontal direction.
            private double _maskHorzRatio;

            public double MaskHorizontalRatio
            {
                get { return _maskHorzRatio; }
                set { _maskHorzRatio = value; }
            }

            #endregion

            #region MaskVerticalRatio property

            // Number of pixels per mask element in the vertical direction.
            private double _maskVertRatio;

            public double MaskVerticalRatio
            {
                get { return _maskVertRatio; }
                set { _maskVertRatio = value; }
            }

            #endregion

            #region MaskIndices property

            // Array of CCD row indices over which the mask elements are aligned.
            //int[] maskIndices = null;
            /*public int MaskIndices()
            {
                if (0 >= this.maskIndices.Length())
                {
                    return null;
                }
                return (this.maskIndices[0]);
            }*/

            // Set the array of CCD row indices over which the mask elements are aligned.
            /*public void SetMaskIndices(int[] pval, int count)
            {
                this.maskIndices.resize(count);
                for (int i = 0; i < count; i++)
                {
                    this.maskIndices[i] = pval[i];
                }
            }*/

            #endregion

            #region HorizontalAlignment property

            // Array of CCD horizontal alignment shifts.
            short[] _alignHorz;

            public short[] HorizontalAlignment
            {
                get { return _alignHorz; }
                set { _alignHorz = value.Clone() as short[]; }
            }

            #endregion

            #region VerticalAlignment property
            // Array of CCD vertical alignment shifts.
            /*public short VerticalAlignment()
             {
                 if (0 >= this.alignVert.Length())
                 {
                     return null;
                 }
                 return (this.alignVert[0]);
             }*/

            // Set the array of CCD vertical alignment shifts.
            /*public void SetVerticalAlignment(short[] pval, int count)
            {
                this.alignVert.resize(count);
                for (int i = 0; i < count; i++)
                {
                    this.alignVert[i] = pval[i];
                }
            }*/
            #endregion

            #region Shifts property

            // Array of shifts to align inverted image.
            private int[] _shifts = null;

            public int[] Shifts
            {
                get { return _shifts; }
                set { _shifts = value.Clone() as int[]; }
            }

            #endregion

            #region Orientation properties 

            private bool _isMaskHorzInvert;

            // Return the flag to indicate that the mask is horizontally inverted
            public bool IsMaskHorzInvert
            {
                get { return _isMaskHorzInvert; }
                set { _isMaskHorzInvert = value; }
            }

            private bool _isMaskVertInvert;

            // Return the flag to indicate that the mask is vertically inverted
            public bool IsMaskVertInvert
            {
                get { return _isMaskVertInvert; }
                set { _isMaskVertInvert = value; }
            }

            private bool _isSpectraInvert;

            // Return the flag to indicate that spectra are reversed (ccd inverted)
            public bool IsSpectraInvert
            {
                get { return _isSpectraInvert; }
                set { _isSpectraInvert = value; }
            }

            #endregion

            // The order of the fit polynomial.
            //private byte rowFitPolynomialOrder;

            #region IsHadamardMask property

            // Indicates that the current binary mask is a Hadamard Mask
            // Array of CCD pixel rows that contain a MASK element.
            protected bool _isHadamardMask;

            // Indicates if the mask is a Hadamard mask (has negative bins)
            public bool IsHadamardMask
            {
                get { return _isHadamardMask; }
                set { _isHadamardMask = value; }
            }

            #endregion

            #region MaskRows property

            // NOTE: maskRows is used in a new BINNING method and hence removes the need
            // for the maskIndices.
            private int[,] _maskRows;

            public int[,] MaskRows
            {
                get { return _maskRows; }
                set { _maskRows = value.Clone() as int[,]; }
            }

            #endregion

            // Return the height of mask model in elements.
            public int MaskHeight
            {
                get { return Mask.GetLength(0); }
            }

            // Return the width of mask model in elements.
            public int MaskWidth
            {
                get { return Mask.GetLength(1); }
            }

            #region Methods

            public void DeserializeCalibration(byte[] calibrationBlob, int ccdWidth = 784, int ccdHeight = 520)
            {
                byte[] fileHeader = { 0xfe, 0xfc, 0x4c, 0x01, 0x10, 0xc4, 0xcf, 0xef };
                bool validHeader = true;

                for (int i = 0; i < fileHeader.Length; i++)
                {
                    if (calibrationBlob[i] != fileHeader[i])
                    {
                        validHeader = false;
                        break;
                    }
                }

                if (validHeader)
                {

                    //FG Temporary  hard code values for now.
                    //Need to add these as parameters to this particular constructor.
                    CCDWidth = (uint)ccdWidth;
                    CCDHeight = (uint)ccdHeight;

                    int dataPtr = 8;
                    Schema = BitConverter.ToInt32(calibrationBlob, dataPtr);
                    dataPtr += sizeof(int);

                    if (Schema == 9)
                    {
                        // Mask Height (16-bit unsigned integer)
                        ushort MaskHeight = BitConverter.ToUInt16(calibrationBlob, dataPtr); //(ushort)dataPtr;
                        dataPtr += sizeof(ushort);
                        // Mask Width (16-bit unsigned integer)
                        ushort MaskWidth = BitConverter.ToUInt16(calibrationBlob, dataPtr); //(ushort)dataPtr;
                        dataPtr += sizeof(ushort);

                        // Mask Vertical Ratio (16-bit unsigned integer)
                        MaskVerticalRatio = BitConverter.ToUInt16(calibrationBlob, dataPtr); // (ushort)dataPtr);
                        dataPtr += sizeof(ushort);
                        // Mask Horizontal Ratio (16-bit unsigned integer)
                        MaskHorizontalRatio = BitConverter.ToUInt16(calibrationBlob, dataPtr); // (ushort)dataPtr);
                        dataPtr += sizeof(ushort);

                        // Mask Flipped (ushort: 0x00 = no flip, 0x01 = vert, 0x02 = horz, 0x03 = horz & vert)
                        byte maskFlip = calibrationBlob[dataPtr++]; //dataPtr++;

                        IsMaskHorzInvert = (maskFlip & 0x02) == 0x02;
                        IsMaskVertInvert = (maskFlip & 0x01) == 0x01;
                        IsSpectraInvert = (0x01 == calibrationBlob[dataPtr++]);

                        //Read past the Mask Indices.  We no longer use that data
                        //We now use the MaskRows data to determine mask element row positions.
                        dataPtr += (sizeof(int) * MaskHeight);

                        ushort nXAlignmentCount = BitConverter.ToUInt16(calibrationBlob, dataPtr); //(ushort)dataPtr;
                        dataPtr += sizeof(ushort);

                        short[] alignHorz = new short[nXAlignmentCount];
                        for (int i = 0; i < nXAlignmentCount; i++)
                        {
                            alignHorz[i] = BitConverter.ToInt16(calibrationBlob, dataPtr);
                            dataPtr += sizeof(short);
                        }
                        HorizontalAlignment = alignHorz;

                        ushort nYAlignmentCount = BitConverter.ToUInt16(calibrationBlob, dataPtr); //(ushort)dataPtr;
                        dataPtr += sizeof(ushort);
                        //Not using YAlignment even during calibration.
                        dataPtr += sizeof(short) * nYAlignmentCount;

                        // Source Peaks (16-bit unsigned integer)
                        ushort sourcePeaks = BitConverter.ToUInt16(calibrationBlob, dataPtr); //(ushort)dataPtr;
                        dataPtr += sizeof(ushort);
                        if (sourcePeaks > 0)
                        {
                            // Shifts (16-bit integer array see MaskHeight)

                            int[] shifts = new int[MaskHeight];
                            for (int i = 0; i < shifts.Length; i++)
                            {
                                shifts[i] = BitConverter.ToInt16(calibrationBlob, dataPtr);
                                dataPtr += sizeof(short);
                            }
                            Shifts = shifts;
                        }

                        //Read past the Polynomial order. Only used during calibration.
                        dataPtr += sizeof(int);

                        // Number of Entries (32-bit integer)
                        int numEntries = BitConverter.ToInt32(calibrationBlob, dataPtr); //(int)dataPtr;
                        dataPtr += sizeof(int);
                        if (numEntries > 0)
                        {
                            // Read past Observed Pixels (32-bit float array see Number of Entries)
                            // Only used in calibration.
                            dataPtr += sizeof(float) * numEntries;
                            // Read past Wavelengths (32-bit float array see Number of Entries)
                            // Only used during calibration.
                            dataPtr += sizeof(float) * numEntries;
                        }

                        // Dead Pixels Left (16-bit unsigned integer)
                        CCDLeft = (uint)BitConverter.ToInt16(calibrationBlob, dataPtr); // (short)dataPtr);
                        dataPtr += sizeof(short);
                        // Dead Pixels Right (16-bit unsigned integer)

                        CCDRight = CCDWidth - (uint)BitConverter.ToInt16(calibrationBlob, dataPtr); // (short)dataPtr);
                        dataPtr += sizeof(short);

                        //Advance pointer past the Calibration Name field.
                        //Name string size (16-bit unsigned integer)
                        string name = Encoding.ASCII.GetString(calibrationBlob, dataPtr + sizeof(ushort), BitConverter.ToUInt16(calibrationBlob, dataPtr));
                        dataPtr += sizeof(ushort) + BitConverter.ToUInt16(calibrationBlob, dataPtr); //(ushort)dataPtr;

                        //Advance pointer past the Mask Name field.
                        //MaskName string size (16-bit unsigned integer)
                        string maskName = Encoding.ASCII.GetString(calibrationBlob, dataPtr + sizeof(ushort), BitConverter.ToUInt16(calibrationBlob, dataPtr));
                        dataPtr += sizeof(ushort) + BitConverter.ToUInt16(calibrationBlob, dataPtr); //(ushort)dataPtr;

                        //Advance pointer past the Spectral Source Name field.
                        //Spectral Source string size (16-bit unsigned integer)
                        string spectralSource = Encoding.ASCII.GetString(calibrationBlob, dataPtr + sizeof(ushort), BitConverter.ToUInt16(calibrationBlob, dataPtr));
                        dataPtr += sizeof(ushort) + BitConverter.ToUInt16(calibrationBlob, dataPtr); //(ushort)dataPtr;

                        //Advance pointer past the Serial Number field.
                        // Serial Number string size (16-bit unsigned integer)
                        string serialNumber = Encoding.ASCII.GetString(calibrationBlob, dataPtr + sizeof(ushort), BitConverter.ToUInt16(calibrationBlob, dataPtr));
                        dataPtr += sizeof(ushort) + BitConverter.ToUInt16(calibrationBlob, dataPtr); //(ushort)dataPtr;

                        // Deserialize mask.
                        double[,] maskBinary = new double[MaskHeight, MaskWidth];
                        for (int i = 0; i < MaskHeight; i++)
                        {
                            for (int j = 0; j < MaskWidth; j++)
                            {
                                maskBinary[i, j] = BitConverter.ToSingle(calibrationBlob, dataPtr);
                                dataPtr += sizeof(float);
                            }
                        }
                        Mask = maskBinary;

                        // Advance the pointer past the unused Gray Scale Mask Elements
                        // Gray Scale Mask Elements (32-bit float array see Number of MaskHeight and MaskWidth)
                        dataPtr += sizeof(float) * MaskHeight * MaskWidth;

                        //Read our dispersion correction coefficients.
                        int iCorrectPolynomOrder = calibrationBlob[dataPtr++]; // * dataPtr++);
                        double[,] fitCeficients = new double[MaskHeight, iCorrectPolynomOrder + 1];
                        for (int i = 0; i < MaskHeight; i++)
                        {
                            for (int j = 0; j < iCorrectPolynomOrder + 1; j++)
                            {
                                fitCeficients[i, j] = BitConverter.ToSingle(calibrationBlob, dataPtr);
                                dataPtr += sizeof(float);
                            }
                        }
                        CorrectPolynom = fitCeficients;

                        // Minimum supported wavelength
                        MinimumWavelength = BitConverter.ToSingle(calibrationBlob, dataPtr); // (float)dataPtr);
                        dataPtr += sizeof(float);
                        // Maximum supported wavelength
                        MaximumWavelength = BitConverter.ToSingle(calibrationBlob, dataPtr); // (float)dataPtr);
                        dataPtr += sizeof(float);
                        //Advance buffer pointer past the NIST 2241 Correction factor
                        //The correction factor is no longer used as a precomputed factor
                        dataPtr += sizeof(ushort) + (sizeof(float) * BitConverter.ToUInt16(calibrationBlob, dataPtr)); //(ushort)dataPtr);

                        //MaskRows
                        //This is the location of the MASK elements on the CCD
                        if ((dataPtr - calibrationBlob.Length + 1) < calibrationBlob.Length)
                        {
                            //todo Vector<Vector<int>> myMaskRows = pInstrumentConfiguration.GetMaskRows();
                            //todo myMaskRows.resize(MaskHeight, Vector<int>(DefineConstants.MASK_ROW_INDICES_LEN));
                            int[,] myMaskRows = new int[MaskHeight, 2];
                            int lastRow = MaskHeight;
                            for (int i = 0; i < lastRow; i++)
                            {
                                //todo myMaskRows[i][DefineConstants.ELEMENT_BEGIN_NDX] = (ushort)dataPtr;
                                myMaskRows[i, 0] = BitConverter.ToUInt16(calibrationBlob, dataPtr);
                                dataPtr += sizeof(ushort);
                                //myMaskRows[i][DefineConstants.ELEMENT_END_NDX] = (ushort)dataPtr;
                                myMaskRows[i, 1] = BitConverter.ToUInt16(calibrationBlob, dataPtr);
                                dataPtr += sizeof(ushort);
                            }
                            MaskRows = myMaskRows;
                        }
                    }
                }

            }

            #endregion
        }
    }
}