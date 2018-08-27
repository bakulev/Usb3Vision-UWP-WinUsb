using System;
using System.IO;
using System.Text;
using System.Diagnostics;
using System.Xml;
using System.Xml.Serialization;

namespace Centice.Spectrometry.Base
{

    [Serializable()]
    public class SpectrometerCalibration
    {

        float _LaserWavelength;
        float? _LaserCalibrationTemperature;
        Calibration _Calibration;
        Spectra _NistSpectra;

        public SpectrometerCalibration()
        {
        }

        public SpectrometerCalibration(Calibration cre)
        {
            _Calibration = cre;
        }

        public SpectrometerCalibration(Calibration cre, float laserWavelength)
        {
            _Calibration = cre;
            _LaserWavelength = laserWavelength;
        }

        public SpectrometerCalibration(Calibration cre, float laserWavelength, float calibrationTemperature)
        {
            _Calibration = cre;
            _LaserWavelength = laserWavelength;
            _LaserCalibrationTemperature = calibrationTemperature;
        }

        public static SpectrometerCalibration Load(string pathName)
        {
            string xmlObjArchive = File.ReadAllText(pathName, Encoding.UTF8);
            return ParseCalibration(xmlObjArchive);
        }

        public static SpectrometerCalibration ParseCalibration(string calibration)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(SpectrometerCalibration));
            using (StringReader stringReader = new StringReader(calibration))
            {
                using (XmlTextReader xmlReader = new XmlTextReader(stringReader))
                {
                    return serializer.Deserialize(xmlReader) as SpectrometerCalibration;
                }
            }
        }

        [XmlElement(IsNullable = true)]
        public byte[] ReconstructionCalibrationByteArray
        {
            get
            {
                if (null != _Calibration)
                {
                    return _Calibration.ToByteArray();
                }

                return null;
            }
            set
            {
                CreateCalibration(value);
            }
        }

        [XmlElement(IsNullable = true)]
        public Spectra NISTSpectra
        {
            get
            {
                return _NistSpectra;
            }
            set
            {
                _NistSpectra = value;
            }
        }

        [XmlIgnore()]
        public Calibration ReconstructionCalibration
        {
            get
            {
                return _Calibration;
            }
            set
            {
                _Calibration = value;
            }
        }

        public float LaserWavelength
        {
            get
            {
                return _LaserWavelength;
            }
            set
            {
                _LaserWavelength = value;
            }
        }

        [XmlElement(IsNullable = true)]
        public float? LaserCalibrationTemperature
        {
            get
            {
                return _LaserCalibrationTemperature;
            }
            set
            {
                _LaserCalibrationTemperature = value;
            }
        }

        private void CreateCalibration(byte[] value)
        {
            if (null == value)
            {
                _Calibration = null;
            }
            else
            {
                Centice.Spectrometry.Base.Calibration c = new Centice.Spectrometry.Base.Calibration(value);
                _Calibration = c as Calibration;
            }
        }

        public bool IsValidReconstructionCalibration
        {
            get
            {
                return null != ReconstructionCalibration
                       && ReconstructionCalibration.IsValid;
            }
        }

        public bool IsValidSpectrometerCalibration
        {
            get
            {
                return IsValidReconstructionCalibration
                       && null != NISTSpectra
                       && null != NISTSpectra.Spectrum
                       && NISTSpectra.Spectrum[0].Length > 0
                       && NISTSpectra.Spectrum[1].Length == NISTSpectra.Spectrum[0].Length;
            }
        }

        #if false

#region NIST

        public double[][] GetNISTCorrection()
        {
            return (NISTSpectra != null && NISTSpectra.Spectrum != null) ? GetNISTCorrection(NISTSpectra, LaserWavelength) : GetNISTCorrection(LaserWavelength);
        }

        /// <summary>
        /// Gets NIST correction factors from spectrometer calibration.
        /// </summary>
        /// <param name="wavelength">
        /// The wavelength of the spectrometer's laser.
        /// </param>
        /// <returns>
        /// The NIST correction factors from <seealso cref="ReconstructionCalibration"/>,
        /// Raman-shifted based on <paramref name="wavelength"/>.
        /// </returns>
        private double[][] GetNISTCorrection(float wavelength)
        {
            Debug.Assert(wavelength > 0.0f);

            // Covert from wave lengths to Raman shift values.				
            double minRamanShift = RamanShift.GetRamanShift(_Calibration.MinimumWavelength, wavelength);
            double maxRamanShift = RamanShift.GetRamanShift(_Calibration.MaximumWavelength, wavelength);

            // Fetch an array of evenly spaced Raman shift values based on the 
            // number of NIST correction factors.  
            double[] ramanShifts = RangeArray(minRamanShift, maxRamanShift, _Calibration.NISTIntensityCorrectionFactors.Length);

            // Build a NIST correction array.
            double[][] nist = new double[][]
            {
                ramanShifts,
                ToDoubleArray(_Calibration.NISTIntensityCorrectionFactors)
            };

            return nist;
        }

        /// <summary>
        /// from centice.pass.commonlibrary.utility
        /// Creates an array of evenly spaced values given a value range
        /// and number of required steps.
        /// </summary>
        /// <param name="min">The minimum value of the desired range.</param>
        /// <param name="max">The maximum value of the desired range.</param>
        /// <param name="NumOfSteps">The number of steps between <paramref name="Min"/>
        /// and <paramref name="max"/>, inclusive.
        /// </param>
        /// <returns>An array of evenly spaced range values.</returns>
        public static double[] RangeArray(double min, double max, int NumOfSteps)
        {
            if (max <= min)
            {
                throw new ArgumentException("max must be greater than min");
            }

            double[] result = new double[NumOfSteps];
            double increment = (max - min) / (NumOfSteps - 1);

            // Build the range.
            result[0] = min;
            for (int iIndex = 1; iIndex < NumOfSteps; iIndex++)
            {
                result[iIndex] = result[iIndex - 1] + increment;
            }
            result[result.Length - 1] = max;

            return result;
        }

        /// <summary>
        /// from centice.pass.commonlibrary.utility
        /// Converts an array of floats to an array of doubles.
        /// </summary>
        /// <param name="floats">An array of floats.</param>
        /// <returns>An array of doubles.</returns>
        public static double[] ToDoubleArray(float[] floats)
        {
            return Array.ConvertAll<float, double>(
                floats,
                delegate (float f) { return (double)f; });
        }

        private double[][] GetNISTCorrection(Spectra NistSpectra, float wavelength)
        {

            Debug.Assert(wavelength > 0.0f);
            Debug.Assert(NistSpectra.Spectrum != null);

            double[][] ramanNISTSpectra = NISTCalibrationUtil.RamanShiftAndInterpolate(NistSpectra.Spectrum, wavelength);
            double[][] NISTCorrection = NISTCalibrationUtil.SolveSRM2241Correction(ramanNISTSpectra, wavelength);

            return NISTCorrection;
        }

        /// <summary>
        /// Given a spectrum, applies NIST correction
        /// </summary>
        /// <param name="spectrum"> spectrum to NIST correct</param>
        /// <param name="exposureTime">exposure time</param>
        /// <param name="doCorrection">true if correction needs to be applied</param>
        /// <returns>NIST corrected spectrum</returns>
        public double[] GetNISTCorrectedSpectra(double[] spectrum, double exposureTime, bool doCorrection = true)
        {
            double[] correctedSpectrum = (double[])spectrum.Clone();
            double[] NistCorrectionFactor = GetNISTCorrection()[1];

            for (int i = 0; i < correctedSpectrum.Length; i++)
            {
                correctedSpectrum[i] *= NistCorrectionFactor[i];

                if (doCorrection)
                {
                    correctedSpectrum[i] /= exposureTime;
                }
            }

            return correctedSpectrum;
        }

        #endregion

        #endif

    }

    [Serializable()]
    public class Spectra
    {
        double[][] _SpectraData;
        float _ExposureTime;
        uint _ExposuresAveraged = 1;
        bool _Saturated;

        /// <summary>
        /// Default ctor required for XML serialization.
        /// </summary>
        public Spectra() { }

        /// <summary>
        /// The time the CCD shutter was open for each and every
        /// spectrometer image captured and averaged into the composite.
        /// </summary>
        public float ExposureTime
        {
            get { return _ExposureTime; }
            set { _ExposureTime = value; }
        }

        /// <summary>
        /// Spectra data as returned from the reconstruction engine.
        /// </summary>
        [System.Xml.Serialization.XmlArray(IsNullable = true)]
        public double[][] Spectrum
        {
            //We want to copy the data so that clients don't overwrite the data when they
            //compute the correction factor. In this case a shallow copy (ie. clone) is
            //acceptable, since the cloned data is a value type and not a reference type.
            get
            {
                return new double[][] {
                    _SpectraData[0].Clone() as double [],
                    _SpectraData[1].Clone() as double []};
            }
            set
            {
                if (value == null ||
                    (value.Length == 2 &&
                    value[0].Length > 0 &&
                    value[0].Length == value[1].Length)
                    )
                {
                    _SpectraData = value;
                }
            }
        }

        /// <summary>
        /// Number of exposures that were averaged into
        /// one composite image. 
        /// </summary>
        public uint ExposuresAveraged
        {
            get { return _ExposuresAveraged; }
            set
            {
                if (value > 0)
                {
                    _ExposuresAveraged = value;
                }
            }
        }

        /// <summary>
        /// CRE detected that the image used to produce this spectra is saturated.
        /// </summary>
        public bool Saturated
        {
            get { return _Saturated; }
            set { _Saturated = value; }
        }
    }
}
