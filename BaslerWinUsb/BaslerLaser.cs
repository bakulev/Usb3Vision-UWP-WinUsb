using Centice.Spectrometry.Base;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace BaslerWinUsb
{
    public class BaslerLaser : IExcitationLasers
    {
        #region Constructors
        public BaslerLaser(IImageDevice device)
        {
            _device = device;
        }
        #endregion

        #region Fields
        IImageDevice _device;
        #endregion

        public string Name => throw new NotImplementedException();

        public string ModelNumber => throw new NotImplementedException();

        public string SerialNumber => throw new NotImplementedException();

        public bool IsAttached => throw new NotImplementedException();

        public event EventHandler<EventArgs> Attached;
        public event EventHandler<EventArgs> Detached;
        public event EventHandler<EventArgs> Enabled;
        public event EventHandler<EventArgs> Disabled;

        public Task<bool> GetEnabled(ushort Laser)
        {
            if (Laser != 0)
                throw new Exception("Wrong laserNum");
            return _device.GetEnabled(Laser);
        }

        public Task<float> GetLaserTemperature(ushort Laser)
        {
            throw new NotImplementedException();
        }

        public Task SetLaserState(ushort Laser, bool Enabled)
        {
            if (Laser != 0)
                throw new Exception("Wrong laserNum");

            return _device.SetLaserState(Laser, Enabled);
        }
    }
}
