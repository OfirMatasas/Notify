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
                r_logger.LogInformation("No Bluetooth adapter found.");
            }
            
            if (!adapter!.IsEnabled)
            {
                r_logger.LogInformation("Bluetooth adapter is not enabled.");
            }
            
            if (adapter.BondedDevices?.Count == 0)
            {
                r_logger.LogInformation("No Bluetooth devices found.");
            }
            else
            {
                foreach (var device in adapter.BondedDevices!)
                {
                    r_logger.LogInformation($"Found Bluetooth device: {device.Name}, address: {device.Address}");
                }
            }
        }
    }
}
