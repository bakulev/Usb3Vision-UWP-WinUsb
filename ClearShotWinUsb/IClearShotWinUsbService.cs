using System;
using System.Threading.Tasks;
using LibUsbDotNet.Main;

namespace Centice.Spectrometry.Spectrometers.Cameras
{
    /// <summary>
    /// This interface for iplementing different camera interaction technicks.
    /// For example: one implementation using LibUsbDotNet
    ///              the other using Win10 Usb interaction.
    /// </summary>
    public interface IClearShotWinUsbService
    {
        // Fields

        bool IsConnected { get; }

        //Events

        event EventHandler<EventArgs> Connected;

        event EventHandler<EventArgs> Disconnected;

        //Functions

        //void CloseCameraConnection();

        //Camera Basic Funcs (all are async funcs)

        Task<bool> SendCommand(byte[] cmd, string cmdName);

        Task<byte[]> GetResults(byte cmd, string cmdName, ReadEndpointID inPipe = ClearShotDevice.GET_COMMAND_RESULT_INPOINT);
    }
}
