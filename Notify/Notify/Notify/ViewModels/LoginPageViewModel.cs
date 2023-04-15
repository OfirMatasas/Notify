using System;
using System.Threading.Tasks;
using Notify.Helpers;
using Notify.Services.Location;
using Plugin.Geolocator;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace Notify.ViewModels
{
    public class LoginPageViewModel : BaseViewModel
    {
        public Command LogInCommand { get; set; }
        public Command SignUpCommand { get; set; }
        
        private LocationService locationService;
        
        private string m_UserName;
        private string m_Password;
        private Task Init { get; }
        
        public string UserName
        {
            get => m_UserName;
            set => SetProperty(ref m_UserName, value);
        }

        public string Password
        {
            get => m_Password;
            set => SetProperty(ref m_Password, value);
        }
        
        public LoginPageViewModel()
        {
            LogInCommand = new Command(onLoginClicked);
            SignUpCommand = new Command(onSignUpClicked);

            Init = initialize();
        }
        
        private async Task initialize()
        {
            locationService = new LocationService();

            VersionTracking.Track();
            if (VersionTracking.IsFirstLaunchEver)
            {
                await Shell.Current.GoToAsync("///welcome");
            }
            else
            {
                await Shell.Current.GoToAsync("///welcome");
            }
            
            locationService.SubscribeToLocationMessaging();
        }
        
        private async void onLoginClicked()
        {
            bool debugAutoLogin = true;
            IsBusy = true;
            
            if (!debugAutoLogin)
            {
                try
                {
                    if (m_UserName.Equals(Constants.USERNAME) && Password.Equals(Constants.PASSWORD))
                    {
                        await locationService.ManageLocationTracking();
                        await Shell.Current.GoToAsync("///main");
                    }
                    else
                    {
                        await App.Current.MainPage.DisplayAlert("Error", "Invalid credentials", "OK");
                    }

                }
                catch (Exception)
                {
                    await App.Current.MainPage.DisplayAlert("Error", "Empty credentials", "OK");
                }

                IsBusy = false;
            }
            else
            {
                await locationService.ManageLocationTracking();
                await Shell.Current.GoToAsync("///main");
            }
        }

        private async void onSignUpClicked()
        {
            await Shell.Current.GoToAsync("///register");
        }
    }
}
