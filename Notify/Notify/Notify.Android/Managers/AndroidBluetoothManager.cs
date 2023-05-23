using Android.Bluetooth;
using Notify.Helpers;
using Notify.Interfaces.Managers;
using Xamarin.Forms;
using Debug = System.Diagnostics.Debug;

[assembly: Dependency(typeof(Notify.Droid.Managers.AndroidBluetoothManager))]
namespace Notify.Droid.Managers
{
    public class AndroidBluetoothManager : IBluetoothManager
    {
        private readonly LoggerService r_logger = LoggerService.Instance;

        public void PrintAllBondedBluetoothDevices()
        {
            BluetoothAdapter adapter = BluetoothAdapter.DefaultAdapter;
            
            if (adapter == null)
            {
                r_logger.LogDebug("No Bluetooth adapter found.");
            }
            
            if (!adapter!.IsEnabled)
            {
                r_logger.LogDebug("Bluetooth adapter is not enabled.");
            }
            
            if (adapter.BondedDevices?.Count == 0)
            {
                r_logger.LogDebug("No Bluetooth devices found.");
            }
            else
            {
                foreach (var device in adapter.BondedDevices!)
                {
                    r_logger.LogDebug($"Found Bluetooth device: {device.Name}, address: {device.Address}");
                }
            }
        }
    }
}
