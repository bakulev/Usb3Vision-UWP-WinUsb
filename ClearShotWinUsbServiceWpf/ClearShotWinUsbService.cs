using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using LibUsbDotNet;
using LibUsbDotNet.DeviceNotify;
using LibUsbDotNet.Main;

namespace Centice.Spectrometry.Spectrometers.Cameras
{
    public class ClearShotWinUsbService : IClearShotWinUsbService
    {
        #region Private Variables

        private IDeviceNotifier _devNotifier;
        private UsbDevice _targetDevice;
        private readonly object _myLock = new object();

        #endregion

        #region Manage Camera Connection/Disconnection

        public bool IsConnected { get { return _targetDevice != null; } }

        public ClearShotWinUsbService()
        {
            _targetDevice = UsbDevice.OpenUsbDevice(x => x.Pid == ClearShotDevice.ProductId && x.Vid == ClearShotDevice.VendorId);

            if(_targetDevice != null)
                OnConnected(this, EventArgs.Empty);

            _devNotifier = DeviceNotifier.OpenDeviceNotifier();
            _devNotifier.OnDeviceNotify += OnDevNotify;
        }

        private void OnDevNotify(object sender, DeviceNotifyEventArgs e)
        {
            if (e.Device.IdProduct == ClearShotDevice.ProductId && e.Device.IdVendor == ClearShotDevice.VendorId)
            {
                if (e.EventType == EventType.DeviceArrival)
                {
                    //Set and raise event.
                    _targetDevice = UsbDevice.OpenUsbDevice(x => x.Pid == ClearShotDevice.ProductId && x.Vid == ClearShotDevice.VendorId);
                    OnConnected(this, EventArgs.Empty);
                }
                else if (e.EventType == EventType.DeviceRemoveComplete)
                {
                    if (_targetDevice != null)
                    {
                        _targetDevice.Close();
                        _targetDevice = null;
                    }
                    OnDisconnected(this, EventArgs.Empty);
                }
            }
        }

        #endregion

        #region MyEvents

        public event EventHandler<EventArgs> Connected;

        protected virtual void OnConnected(object sender, EventArgs e)
        {
            Connected?.Invoke(sender, e);
        }

        public event EventHandler<EventArgs> Disconnected;

        protected virtual void OnDisconnected(object sender, EventArgs e)
        {
            Disconnected?.Invoke(sender, e);
        }

        #endregion

        #region PrivateFuncs

        public async Task<bool> SendCommand(byte[] cmd, string cmdName)
        {
            return await Task.Run(() =>
            {
                if (_targetDevice == null) return false;
                lock (_myLock)
                {
                    //Clear all dt
                    var reader = _targetDevice.OpenEndpointReader(ClearShotDevice.GET_COMMAND_RESULT_INPOINT);
                    reader.ReadFlush();
                    //reader.Dispose();
                    reader = _targetDevice.OpenEndpointReader(ClearShotDevice.GET_IMAGE_RESULT_INPOINT);
                    reader.ReadFlush();
                    //reader.Dispose();
                    reader = null;

                    //WriteAllData.
                    var writer = _targetDevice.OpenEndpointWriter(ClearShotDevice.SEND_COMMAND_ENDPOINT);
                    int transferLength;
                    var status = writer.Write(cmd, 10000, out transferLength);
                    if (status == ErrorCode.None)
                    {
                        Debug.WriteLine($"* {cmdName} - {transferLength} bytes written.");
                        return true;
                    }
                    else
                    {
                        Debug.WriteLine($"* {cmdName} - Write failed! - " + status);
                    }
                    return false;
                }
            });
        }

        /// <summary>
        /// Get Result Of Latest CMD.
        /// </summary>
        /// <param name="cmd">The Expected Command.</param>
        /// <param name="inPipe">1 For CMD Response and 0 for Image Data.</param>
        /// <returns></returns>
        public async Task<byte[]> GetResults(byte cmd, string cmdName, ReadEndpointID inPipe = ClearShotDevice.GET_COMMAND_RESULT_INPOINT)
        {
            return await Task.Run(() =>
            {
                if (_targetDevice == null) return null;
                lock (_myLock)
                {
                    //Reader.
                    var reader = _targetDevice.OpenEndpointReader(inPipe);

                    //Get First data.
                    byte[] result = new byte[64];
                    int length; ErrorCode eReturn;
                    if ((eReturn = reader.Read(result, 10000, out length)) == ErrorCode.None)
                    {
                        Debug.WriteLine($"* {cmdName} - {length} bytes read.");
                    }
                    else
                    {
                        Debug.WriteLine($"* {cmdName} - No data to read! " + eReturn);
                    }

                    //Make sure this is result of command that we want.
                    if (result[0] != cmd)
                    {
                        Debug.WriteLine($"* {cmdName} - Result not for expected command.");
                    }
                    else
                    {

                        //Change result depending on size.
                        var size = BitConverter.ToUInt32(result, 1);

                        if (size > result.Count())
                        {
                            Debug.WriteLine($"* {cmdName} - Getting extra data.....");

                            byte[] newResult = new byte[size - result.Length];
                            eReturn = reader.Read(newResult, 10000, out length);
                            if (eReturn == ErrorCode.None)
                            {
                                var newRes = ConcatArrays(result, newResult);
                                result = newRes;
                            }
                            else
                            {
                                Debug.WriteLine($"* {cmdName} - Error reading extra data! " + eReturn);
                            }

                            Debug.WriteLine($"* {cmdName} - Done getting extra data.");
                        }

                        //Remove padded zeros and return.
                        if (size < result.Count())
                        {
                            return result.Take((int)size).ToArray();
                        }
                    }
                    return result;
                }
            });
        }

        /// <summary>
        /// Concatinates arrays.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list"></param>
        /// <returns></returns>
        private static T[] ConcatArrays<T>(params T[][] list)
        {
            var result = new T[list.Sum(a => a.Length)];
            int offset = 0;
            for (int x = 0; x < list.Length; x++)
            {
                list[x].CopyTo(result, offset);
                offset += list[x].Length;
            }
            return result;
        }

        #endregion
    }
}
