using Xamarin.Essentials;

namespace Notify.WiFi
{
    public interface IWiFiManager
    {
        void PrintConnectedWiFi(object sender, ConnectivityChangedEventArgs e);
    }
}
