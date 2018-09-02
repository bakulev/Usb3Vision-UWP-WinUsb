using Centice.Spectrometry.Base;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace BaslerWinUsb
{
    public class BaslerParams : IParamStorage
    {
        public string Name => throw new NotImplementedException();

        public string ModelNumber => throw new NotImplementedException();

        public string SerialNumber => throw new NotImplementedException();

        public bool IsAttached => throw new NotImplementedException();

        public Calibration Calibration => throw new NotImplementedException();

        public SpectrometerCalibration DeviceCalibration => throw new NotImplementedException();

        public event EventHandler<EventArgs> Attached;
        public event EventHandler<EventArgs> Detached;

        public List<Tuple<int, int>> GetBadPixels()
        {
            throw new NotImplementedException();
        }

        public Task<byte[]> GetCalibrationByteArray(CancellationToken ct)
        {
            throw new NotImplementedException();
        }

        public Task<string> GetCalibrationString(CancellationToken ct)
        {
            throw new NotImplementedException();
        }

        public double[,] GetDarkImage(float exposure, float temperature)
        {
            throw new NotImplementedException();
        }

        public float GetLaserWavelength()
        {
            throw new NotImplementedException();
        }

        public double[][] GetNistSpectrum()
        {
            throw new NotImplementedException();
        }

        public int GetShift()
        {
            throw new NotImplementedException();
        }

        public Task LoadCalibration(CancellationToken ct)
        {
            throw new NotImplementedException();
        }

        public void SetShift(int shift)
        {
            throw new NotImplementedException();
        }
    }
}
