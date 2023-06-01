using Notify.Bluetooth;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Notify.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class BluetoothSettingsPage : ContentPage
    {
        //private BluetoothManager m_BluetoothManager;
        
        public BluetoothSettingsPage()
        {
            InitializeComponent();
            BindingContext = new ViewModels.BluetoothSettingsPageViewModel();
            //m_BluetoothManager = BluetoothManager.Instance;
        }

        // protected override async void OnAppearing()
        // {
        //     base.OnAppearing();
        //
        //     if (await m_BluetoothManager.CheckBluetoothStatus())
        //     {
        //         m_BluetoothManager.StartBluetoothScanning();
        //     }
        // }
        //
        // protected override void OnDisappearing()
        // {
        //     base.OnDisappearing();
        //
        //     m_BluetoothManager.StopScanningForDevices();
        // }
    }
}