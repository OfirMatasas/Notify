using Android.Bluetooth;
using Notify.Interfaces.Managers;
using Xamarin.Forms;
using Debug = System.Diagnostics.Debug;

[assembly: Dependency(typeof(Notify.Droid.Managers.AndroidBluetoothManager))]
namespace Notify.Droid.Managers
{
    public class AndroidBluetoothManager : IBluetoothManager
    {
        public void PrintAllBondedBluetoothDevices()
        {
            BluetoothAdapter adapter = BluetoothAdapter.DefaultAdapter;
            
            if (adapter == null)
            {
                Debug.WriteLine("No Bluetooth adapter found.");
            }
            
            if (!adapter!.IsEnabled)
            {
                Debug.WriteLine("Bluetooth adapter is not enabled.");
            }
            
            if (adapter.BondedDevices?.Count == 0)
            {
                Debug.WriteLine("No Bluetooth devices found.");
            }
            else
            {
                foreach (var device in adapter.BondedDevices!)
                {
                    Debug.WriteLine($"Found Bluetooth device: {device.Name}, address: {device.Address}");
                }
            }
        }
    }
}
