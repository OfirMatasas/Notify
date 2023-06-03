using Notify.Helpers;
using Xamarin.Forms;

namespace Notify.ViewModels
{
    public class NotificationSettingsPageViewModel
    {
        public Command backCommand { get; set; }
        
        public NotificationSettingsPageViewModel()
        {
            backCommand = new Command(onBackButtonClicked);
        }
        
        private async void onBackButtonClicked()
        {
            await Shell.Current.GoToAsync(Constants.SHELL_NAVIGATION_SETTINGS);
        }
    }
}