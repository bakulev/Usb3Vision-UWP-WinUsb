namespace CodaDevices.Spectrometry.Model
{
    public interface IDeviceAvailabilityObserver
    {
        void OnDeviceAvailabilityChanged(bool availability);
    }
}
