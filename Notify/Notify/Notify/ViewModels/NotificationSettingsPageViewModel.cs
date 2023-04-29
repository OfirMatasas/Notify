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
            await Shell.Current.GoToAsync("///settings");
        }
    }
}