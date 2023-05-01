using Xamarin.Forms;

namespace Notify.ViewModels
{
    public class WifiSettingsPageViewModel
    {
        public Command BackCommand { get; set; }
        
        public WifiSettingsPageViewModel()
        {
            BackCommand = new Command(onBackButtonClicked);
        }
        
        private async void onBackButtonClicked()
        {
            await Shell.Current.GoToAsync("///settings");
        }
    }
}
