using Notify.Interfaces.Managers;
using Xamarin.Forms;
using CoreBluetooth;
using Foundation;
using Notify.Helpers;

[assembly: Dependency(typeof(Notify.iOS.Managers.iOSBluetoothManager))]
namespace Notify.iOS.Managers
{
    public class iOSBluetoothManager : IBluetoothManager
    {
        private readonly LoggerService r_Logger = LoggerService.Instance;

        public void PrintAllBondedBluetoothDevices()
        {
            CBCentralManager centralManager = new CBCentralManager();
            CBPeripheral[] connectedPeripheralIds = centralManager.RetrievePeripheralsWithIdentifiers(new NSUuid[0]);

            foreach (CBPeripheral peripheral in connectedPeripheralIds)
            {
                r_Logger.LogInformation($"Name: {peripheral.Name}, UUID: {peripheral.Identifier}");
            }
        }
    }
}
