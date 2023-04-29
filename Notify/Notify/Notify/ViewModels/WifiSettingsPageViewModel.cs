using Xamarin.Forms;

namespace Notify.ViewModels
{
    public class WifiSettingsPageViewModel
    {
        public Command backCommand { get; set; }
        
        public WifiSettingsPageViewModel()
        {
            backCommand = new Command(onBackButtonClicked);
        }
        
        private async void onBackButtonClicked()
        {
            await Shell.Current.GoToAsync("///settings");
        }
    }
}