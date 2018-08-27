//TODO should be solved using Centice.PASS.CommonLibrary.Utility;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Centice.Spectrometry.Spectrometers.Cameras
{
    public class CalibrationParameters
    {
        // Reference level. exposure -> 1(temperature -> 1(averageValue)).
        public Dictionary<double, Dictionary<double, double>> tempRef;
        // Reference image. exposure -> 1(temperature -> 1(averageValue, averageWeight))
        public Dictionary<double, Dictionary<double, Tuple<double[,], int>>> tempImages;
        // History of measurements. exposure -> 1(temperature -> *(averageValue, date)).
        public Dictionary<double, Dictionary<double, List<Tuple<double, DateTimeOffset>>>> tempCalib;
        // List of bad pixels.
        public List<Tuple<int, int>> badPixels;

        public double imageDarkSlope = 1;
        public double imageDarkIntercept = 0;
        private double _avgValue = 0;
        private double[,] _avgImage = null;

        public void AddImage(double exposure, double temperature, double[,] image, int weight)
        {
            if (tempImages == null) tempImages = new Dictionary<double, Dictionary<double, Tuple<double[,], int>>>();

            exposure = System.Math.Round(exposure, 3);
            temperature = System.Math.Round(temperature, 1);

            if (tempImages.ContainsKey(exposure))
                if (tempImages[exposure].ContainsKey(temperature))
                    tempImages[exposure][temperature] =
                        ImageAverageWeight(tempImages[exposure][temperature], image);
                else
                    tempImages[exposure].Add(
                        temperature,
                        new Tuple<double[,], int>(image, weight)
                    );
            else
                tempImages.Add(
                    exposure,
                    new Dictionary<double, Tuple<double[,], int>>()
                        { { temperature, new Tuple<double[,], int>(image, weight) } }
                );

            // recalculate average image
            int count = 0;
            _avgImage = null;
            foreach (var a in tempImages)
                foreach (var b in a.Value)
                {
                    count++;
                    if (_avgImage == null) _avgImage = new double[b.Value.Item1.GetLength(0), b.Value.Item1.GetLength(1)];
                    for (int x = 0; x < _avgImage.GetLength(0); x++)
                        for (int y = 0; y < _avgImage.GetLength(1); y++)
                            if (x < b.Value.Item1.GetLength(0) && y < b.Value.Item1.GetLength(1))
                                _avgImage[x, y] += b.Value.Item1[x, y];
                }
            for (int x = 0; x < _avgImage.GetLength(0); x++)
                for (int y = 0; y < _avgImage.GetLength(1); y++)
                    _avgImage[x, y] = _avgImage[x, y] / count;
        }

        public double[,] GetImage(double exposure, double temperature)
        {
            double[,] image = _avgImage;

            if (tempImages != null)
            {
                exposure = System.Math.Round(exposure, 3);
                temperature = System.Math.Round(temperature, 1);

                if (tempImages.ContainsKey(exposure))
                    if (tempImages[exposure].ContainsKey(temperature))
                        image = tempImages[exposure][temperature].Item1;
                    else
                        image = _avgImage;
                else
                    image = _avgImage;
            }

            return image;
        }

        public void AddTemp(double exposure, double temperature, double imageAvg, DateTimeOffset date)
        {
            if (tempCalib == null) tempCalib = new Dictionary<double, Dictionary<double, List<Tuple<double, DateTimeOffset>>>>();

            exposure = System.Math.Round(exposure, 3);
            temperature = System.Math.Round(temperature, 1);

            if (tempCalib.ContainsKey(exposure))
                if (tempCalib[exposure].ContainsKey(temperature))
                    tempCalib[exposure][temperature].Add(
                        new Tuple<double, DateTimeOffset>(imageAvg, date)
                    );
                else
                    tempCalib[exposure].Add(
                        temperature,
                        new List<Tuple<double, DateTimeOffset>>()
                            { new Tuple<double, DateTimeOffset>(imageAvg, date) }
                    );
            else
                tempCalib.Add(exposure,
                    new Dictionary<double, List<Tuple<double, DateTimeOffset>>>() {
                            { temperature, new List<Tuple<double, DateTimeOffset>>()
                            { { new Tuple<double, DateTimeOffset>(imageAvg, date) } }
                        } }
                );

            // recalculate reference value.
            int count = 0;
            double maxAvg = double.MinValue;
            foreach (var a in tempCalib[exposure][temperature])
            {
                count++;
                if (maxAvg < a.Item1) maxAvg = a.Item1;
            }
            SetRef(exposure, temperature, maxAvg);
        }

        public void SetRef(double exposure, double temperature, double imageAvg)
        {
            if (tempRef == null) tempRef = new Dictionary<double, Dictionary<double, double>>();

            exposure = System.Math.Round(exposure, 3);
            temperature = System.Math.Round(temperature, 1);

            if (tempRef.ContainsKey(exposure))
                if (tempRef[exposure].ContainsKey(temperature))
                    tempRef[exposure][temperature] = imageAvg;
                else
                    tempRef[exposure].Add(temperature, imageAvg);
            else
                tempRef.Add(exposure,
                    new Dictionary<double, double>() {
                            { temperature, imageAvg }
                    }
                );

            // recalculate mean value
            int count = 0;
            _avgValue = 0;
            foreach (var a in tempRef)
                foreach (var b in a.Value)
                {
                    count++;
                    _avgValue += b.Value;
                }
            _avgValue = _avgValue / count;
        }

        public double GetRef(double exposure, double temperature)
        {
            double imageAvg = _avgValue;
            if (tempRef != null)
            {
                exposure = System.Math.Round(exposure, 3);
                temperature = System.Math.Round(temperature, 1);

                if (tempRef.ContainsKey(exposure))
                    if (tempRef[exposure].ContainsKey(temperature))
                        imageAvg = tempRef[exposure][temperature];
                    else
                        imageAvg = _avgValue;
                else
                    imageAvg = _avgValue;
            }
            return imageAvg;
        }

        private static Tuple<double[,], int> ImageAverageWeight(Tuple<double[,], int> avg, double[,] image)
        {
            double[,] avgImage = avg.Item1;
            int avgWeight = avg.Item2;
            double[,] result = new double[avgImage.GetLength(0), avgImage.GetLength(1)];
            if (image.GetLength(0) != avgImage.GetLength(0) ||
                image.GetLength(1) != avgImage.GetLength(1)) return avg;
            for (int i = 0; i < avgImage.GetLength(0); i++)
                for (int j = 0; j < avgImage.GetLength(1); j++)
                    result[i, j] = (avgImage[i, j] * avgWeight + image[i, j]) / (avgWeight + 1);
            return new Tuple<double[,], int>(result, avgWeight + 1);
        }

        #region Load and Save Section

        public static string CalibrationDirectory(string logDirectory, string serialNumber)
        {
            if (logDirectory == null) logDirectory = Path.Combine(Path.GetTempPath(), "CodaDevices", "Tmp");
            string parametersPath = Path.Combine(Path.GetDirectoryName(logDirectory),
                "CalibrationParameters", serialNumber);
            Directory.CreateDirectory(parametersPath);

            return parametersPath;
        }

        public double[,] GetImage(string serialNumber, string logDirectory, string setName,
            double exposure, double temperature)
        {
            string binDirectory = Path.Combine(Path.GetDirectoryName(logDirectory),
                "CalibrationParameters", serialNumber, setName);
            int weight = 30;
            //var binDirectory = Path.Combine(parametersPath, "Bin");

            /*
            var imagesFileNames = Director.EnumerateFiles(
                binDirectory,
                "*.bin", SearchOption.TopDirectoryOnly);

            foreach (var imagesFileName in imagesFileNames)
            {
                var image = LoadDoubleArrToBinary(imagesFileName, badPixels);
                image = Centice.Util.ImageUtils.ImageMedianFilterPoints(image, calibrationParameters.badPixels);
                var values = Path.GetFileNameWithoutExtension(imagesFileName).Split(new char[] { '-' });
                double exposure = int.Parse(values[0]) / 1000d;
                double temperature = int.Parse(values[1]) / 10d;
                int weight = int.Parse(values[2])
                    */

            string exposureStr = exposure.ToString("000.000",
                System.Globalization.CultureInfo.InvariantCulture).Replace(".", "");
            string temperatureStr = temperature.ToString("00.0",
                System.Globalization.CultureInfo.InvariantCulture).Replace(".", "");
            string weightStr = weight.ToString("000",
                System.Globalization.CultureInfo.InvariantCulture).Replace(".", "");
            var imageFileName = exposureStr + "-" + temperatureStr + "-" + weightStr;

            var image = LoadDoubleArrToBinary(Path.Combine(binDirectory, imageFileName + ".bin"));

            return image;
        }

        public static List<Tuple<int, int>> LoadCalibrationBadPixels(string logDirectory, string serialNumber,
            string imageFileName, double level)
        {
            var result = new List<Tuple<int, int>>();
            string parametersPath = Path.Combine(Path.GetDirectoryName(logDirectory),
                "CalibrationParameters", serialNumber);
            var binDirectory = Path.Combine(parametersPath, "Bin");
            var image = LoadDoubleArrToBinary(Path.Combine(binDirectory, imageFileName + ".bin"));
            for (int h = 0; h < image.GetLength(0); h++)
                for (int w = 0; w < image.GetLength(1); w++)
                    if (image[h, w] > level)
                        result.Add(new Tuple<int, int>(h, w));
            return result;
        }


        public void LoadCalibrationParameters(string parametersPath, List<Tuple<int, int>> badPixels = null)
        {
            CalibrationParameters calibrationParameters = new CalibrationParameters();

            /*string parametersFile = Path.Combine(parametersPath, "Parameters.xml");
            string parametersString = "";
            if (File.Exists(parametersFile))
            {
                parametersString = File.ReadAllText(parametersFile);
            }
            if (!string.IsNullOrEmpty(parametersString))
            {
                calibrationParameters = XmlSerializationUtil.FromXml<CalibrationParameters>(parametersString);
            }
            else
            {
                //System.Windows.MessageBox.Show(
                //    string.Format("Could not load calibration for device serial number {0}", _Camera.SerialNumber));
            }*/

            calibrationParameters.badPixels = new List<Tuple<int, int>>();
            string badPixelsFile = Path.Combine(parametersPath, "BadPixels.csv");
            if (File.Exists(badPixelsFile))
            {
                var badPixelsStrings = File.ReadAllLines(badPixelsFile);

                foreach (var badPixelsString in badPixelsStrings)
                {
                    var values = badPixelsString.Split(new char[] { ';' });
                    if (values.GetLength(0) > 1)
                    {
                        int item1 = int.Parse(values[0], System.Globalization.CultureInfo.InvariantCulture);
                        int item2 = int.Parse(values[1], System.Globalization.CultureInfo.InvariantCulture);

                        calibrationParameters.badPixels.Add(new Tuple<int, int>(item1, item2));
                    }
                }
            }

            /*string exposuresFile = Path.Combine(parametersPath, "ExpusBadPixelsuresDark.csv");
            if (File.Exists(exposuresFile))
            {
                var exposuresStrings = File.ReadAllLines(exposuresFile);

                foreach (var exposuresString in exposuresStrings)
                {
                    var values = exposuresString.Split(new char[] { ';' });
                    if (values.GetLength(0) > 2)
                    {
                        double exposure = double.Parse(values[0], System.Globalization.CultureInfo.InvariantCulture);
                        double temperature = double.Parse(values[1], System.Globalization.CultureInfo.InvariantCulture);
                        double imageAvg = double.Parse(values[2], System.Globalization.CultureInfo.InvariantCulture);
                        DateTimeOffset datetime = DateTimeOffset.Parse(values[3], System.Globalization.CultureInfo.InvariantCulture);

                        calibrationParameters.AddTemp(exposure, temperature, imageAvg, datetime);
                    }
                }
            }*/

            {
                //string fileName = "008000-440-009"; // "004000-440-008";
                //calibrationParameters.badPixels = LoadCalibrationBadPixels(logDirectory, fileName, 16700);
                /*fileName = "000250-470-004";// "004000-440-008";
                var image = LoadDoubleArrToBinary(Path.Combine(
                    Path.GetDirectoryName(logDirectory), "CalibrationParameters",
                    "Bin", fileName + ".bin"));
                image = ImageMedianFilterPoints(image, calibrationParameters.badPixels);
                //image = ImageRollingBallFilter(400, image);
                Imaging.Png16SaveImageData(image,
                    Path.Combine(
                    Path.GetDirectoryName(logDirectory), "CalibrationParameters",
                    "Png"), fileName + "-f.png");*/
            }

            var binDirectory = Path.Combine(parametersPath, "Bin");
            if (Directory.Exists(binDirectory))
            {
                var imagesFileNames = Directory.EnumerateFiles(
                    binDirectory,
                    "*.bin", SearchOption.TopDirectoryOnly);

                foreach (var imagesFileName in imagesFileNames)
                {
                    var image = LoadDoubleArrToBinary(imagesFileName, badPixels);
                    //TODO should be uncommented
                    //image = Centice.Util.ImageUtils.ImageMedianFilterPoints(image, calibrationParameters.badPixels);
                    var values = Path.GetFileNameWithoutExtension(imagesFileName).Split(new char[] { '-' });
                    double exposure = int.Parse(values[0]) / 1000d;
                    double temperature = int.Parse(values[1]) / 10d;
                    int weight = int.Parse(values[2]);
                    calibrationParameters.AddImage(exposure, temperature, image, weight);
                }
            }

            //return calibrationParameters;
        }

        public static void SaveCalibrationParameters(string parametersPath, CalibrationParameters calibrationParameters)
        {
            string parametersFile = Path.Combine(parametersPath, "Parameters.xml");
            //string parametersString = "";
            //parametersString = XmlSerializationUtil.ToXml(calibrationParameters);
            ///File.WriteAllText(parametersFile, parametersString);

            string badPixelsFile = Path.Combine(parametersPath, "BadPixels.csv");
            string badPixelsString = "";
            foreach (var p in calibrationParameters.badPixels)
                badPixelsString += string.Format(
                            System.Globalization.CultureInfo.InvariantCulture,
                            "{0};{1}", // exposure, temperature, avg, date
                            p.Item1.ToString(System.Globalization.CultureInfo.InvariantCulture),
                            p.Item2.ToString(System.Globalization.CultureInfo.InvariantCulture)
                        ) + Environment.NewLine;
            File.WriteAllText(badPixelsFile, badPixelsString);

            if (calibrationParameters.tempCalib != null)
            {
                string exposuresFile = Path.Combine(parametersPath, "ExpusuresDark.csv");
                string exposuresString = "";
                foreach (var p in calibrationParameters.tempCalib)
                    foreach (var q in p.Value)
                        foreach (var r in q.Value)
                            exposuresString += string.Format(
                                System.Globalization.CultureInfo.InvariantCulture,
                                "{0};{1};{2};{3}", // exposure, temperature, avg, date
                                p.Key.ToString(System.Globalization.CultureInfo.InvariantCulture),
                                q.Key.ToString(System.Globalization.CultureInfo.InvariantCulture),
                                r.Item1.ToString(System.Globalization.CultureInfo.InvariantCulture),
                                r.Item2.ToString("o", System.Globalization.CultureInfo.InvariantCulture)
                            ) + Environment.NewLine;
                File.WriteAllText(exposuresFile, exposuresString);
            }

            var pngDirectory = Path.Combine(parametersPath, "Png");
            var binDirectory = Path.Combine(parametersPath, "Bin");
            if (Directory.Exists(pngDirectory)) Directory.Delete(pngDirectory, true);
            if (Directory.Exists(binDirectory)) Directory.Delete(binDirectory, true);
            foreach (var dic in calibrationParameters.tempImages)
                foreach (var img in dic.Value)
                {
                    var exposure = dic.Key;
                    var temperature = img.Key;
                    var image = img.Value.Item1;
                    var weight = img.Value.Item2;
                    string exposureStr = exposure.ToString("000.000",
                        System.Globalization.CultureInfo.InvariantCulture).Replace(".", "");
                    string temperatureStr = temperature.ToString("00.0",
                        System.Globalization.CultureInfo.InvariantCulture).Replace(".", "");
                    string weightStr = weight.ToString("000",
                        System.Globalization.CultureInfo.InvariantCulture).Replace(".", "");
                    var fileNameWithoutExtension = exposureStr + "-" + temperatureStr + "-" + weightStr;
                    //TODO should be uncommented
                    //Imaging.Png16SaveImageData(image,
                    //    pngDirectory,
                    //    fileNameWithoutExtension + ".png"
                    //);
                    SaveDoubleArrToBinary(
                        Path.Combine(binDirectory, fileNameWithoutExtension + ".bin"),
                        image);
                }
        }

        #endregion

        #region for logging

        private static void SaveDoubleArrToBinary(string fileName, double[,] values)
        {
            try
            {
                Directory.CreateDirectory(Path.GetDirectoryName(fileName));
                // создаем объект BinaryWriter
                using (BinaryWriter writer = new BinaryWriter(File.Open(fileName, FileMode.Create)))
                {
                    writer.Write(values.GetLength(0));
                    writer.Write(values.GetLength(1));
                    for (int i = 0; i < values.GetLength(0); i++)
                        for (int j = 0; j < values.GetLength(1); j++)
                            writer.Write(values[i, j]);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }

        private static double[,] LoadDoubleArrToBinary(string fileName, List<Tuple<int, int>> badBixels = null)
        {
            double[,] values = null;
            try
            {
                // создаем объект BinaryWriter
                using (BinaryReader reader = new BinaryReader(File.Open(fileName, FileMode.Open)))
                {
                    var x = reader.ReadInt32();
                    var y = reader.ReadInt32();
                    values = new double[x, y];
                    for (int i = 0; i < values.GetLength(0); i++)
                        for (int j = 0; j < values.GetLength(1); j++)
                            values[i, j] = reader.ReadDouble();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
            return values;
        }

        #endregion
    }
}
