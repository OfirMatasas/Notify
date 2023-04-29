using Xamarin.Forms;

namespace Notify.ViewModels
{
    public class BluetoothSettingsPageViewModel
    {
        public Command BackCommand { get; set; }
        
        public BluetoothSettingsPageViewModel()
        {
            BackCommand = new Command(onBackButtonClicked);
        }
        
        private async void onBackButtonClicked()
        {
            await Shell.Current.GoToAsync("///settings");
        }
    }
}
