using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Notify.Bluetooth;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Notify.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class BluetoothSettingsPage : ContentPage
    {
        private BluetoothManager m_BluetoothManager;
        
        public BluetoothSettingsPage()
        {
            InitializeComponent();
            BindingContext = new ViewModels.BluetoothSettingsPageViewModel();
        }
    }
}