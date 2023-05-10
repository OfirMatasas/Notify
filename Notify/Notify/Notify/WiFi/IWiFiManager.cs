using System.Collections.Generic;
using Xamarin.Essentials;

namespace Notify.WiFi
{
    public interface IWiFiManager
    {
        void PrintConnectedWiFi(object sender, ConnectivityChangedEventArgs e);
        List<string> GetAvailableNetworks();
        void SendNotifications(object sender, ConnectivityChangedEventArgs connectivityChangedEventArgs);
    }
}
