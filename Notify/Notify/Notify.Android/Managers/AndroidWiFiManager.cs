using Android.Content;
using Android.Net;
using Android.Net.Wifi;
using Notify.WiFi;
using Xamarin.Essentials;
using Xamarin.Forms;
using Debug = System.Diagnostics.Debug;

[assembly: Dependency(typeof(Notify.Droid.Managers.AndroidWiFiManager))]
namespace Notify.Droid.Managers
{
    public class AndroidWiFiManager : IWiFiManager
    {
        private readonly string m_preDefineSsid = "\"AndroidWifi\"";
        
        public void PrintConnectedWiFi(object sender, ConnectivityChangedEventArgs e)
        {
            ConnectivityManager connectivityManager = (ConnectivityManager)Android.App.Application.Context.GetSystemService(Context.ConnectivityService);
            NetworkCapabilities capabilities = connectivityManager.GetNetworkCapabilities(connectivityManager.ActiveNetwork);

            if (capabilities.HasTransport(TransportType.Wifi))
            {
                var wifiManager = (WifiManager)Android.App.Application.Context.GetSystemService(Context.WifiService);
                string ssid = wifiManager.ConnectionInfo.SSID;
        
                if (ssid == m_preDefineSsid)
                {
                    Debug.WriteLine($"You have just connected to your wifi network: {ssid}!");
                }
                else
                {
                    Debug.WriteLine($"Error with ssid: SSID: {ssid} \nPre define SSID: {m_preDefineSsid}");
                }
            }
            else
            {
                Debug.WriteLine("Disconnected from wifi network!");
            }
        }
    }
}