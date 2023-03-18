using System;
using System.Threading.Tasks;
using Notify.Helpers;
using Plugin.Geolocator;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace Notify.ViewModels
{
    public class WelcomePageViewModel : BaseViewModel
    {
        #region Commands

        public Command LogInCommand { get; set; }

        #endregion

        #region Properties

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

        #endregion

        #region Constructors

        public WelcomePageViewModel()
        {
            LogInCommand = new Command(onLoginClicked);

            Init = initialize();
        }

        #endregion

        #region Command Handlers

        private async void onLoginClicked()
        {
            bool debugAutoLogin = true;
            IsBusy = true;
            
            if (!debugAutoLogin)
            {
                try
                {
                    if (m_UserName.Equals(Constants.Username) && Password.Equals(Constants.Password))
                    {
                        locationService.ManageLocationTracking();
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
                locationService.ManageLocationTracking();
                await Shell.Current.GoToAsync("///main");
            }
        }
        
        #endregion

        #region Private Functionality
        
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
            

        }
        
        #endregion
    }
}
