using Xamarin.Forms;

namespace Notify.ViewModels
{
    public class BluetoothSettingsPageViewModel
    {
        public Command backCommand { get; set; }
        
        public BluetoothSettingsPageViewModel()
        {
            backCommand = new Command(onBackButtonClicked);
        }
        
        private async void onBackButtonClicked()
        {
            // await Shell.Current.Navigation.PopAsync();
            await Shell.Current.GoToAsync("///settings");
        }
    }
}