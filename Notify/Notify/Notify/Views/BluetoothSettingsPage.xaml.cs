using Notify.Bluetooth;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Notify.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class BluetoothSettingsPage : ContentPage
    {
        public BluetoothSettingsPage()
        {
            InitializeComponent();
            BindingContext = new ViewModels.BluetoothSettingsPageViewModel();
        }
    }
}