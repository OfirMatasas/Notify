using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Threading.Tasks;
using Notify.Helpers;
using Notify.Services.Location;
using Plugin.Geolocator;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace Notify.ViewModels
{
    public class LoginPageViewModel : BaseViewModel, INotifyPropertyChanged
    {
        private string m_UserName;
        private string m_Password;
        private bool m_RememberMe;
        private LocationService locationService;
        public event PropertyChangedEventHandler PropertyChanged;
        public Command LogInCommand { get; set; }
        public Command SignUpCommand { get; set; }

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

        public bool RememberMe
        {
            get => m_RememberMe;
            set
            {
                if (m_RememberMe != value)
                {
                    m_RememberMe = value;
                    OnPropertyChanged(nameof(RememberMe));
                }
            }
        }

        public LoginPageViewModel()
        {
            LogInCommand = new Command(onLoginClicked);
            SignUpCommand = new Command(onSignUpClicked);
            initialize();
        }

        private async Task initialize()
        {
            locationService = new LocationService();

            VersionTracking.Track();
            if (VersionTracking.IsFirstLaunchEver)
            {
                await Shell.Current.GoToAsync("///login");
            }
            else
            {
                await Shell.Current.GoToAsync("///login");
            }

            locationService.SubscribeToLocationMessaging();
        }

        private async void onLoginClicked()
        {
            bool debugAutoLogin = true;
            IsBusy = true;

            if (debugAutoLogin)
            {
                try
                {
                    bool areCredentialsValid =
                        UserName.Equals(Constants.USERNAME) && Password.Equals(Constants.PASSWORD);

                    if (areCredentialsValid)
                    {
                        if (RememberMe)
                        {
                            storeUserCredentialsInPreferences();
                        }

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

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        
        private void storeUserCredentialsInPreferences()
        {
            if (!Preferences.ContainsKey("NotifyUserName"))
            {
                Preferences.Set("NotifyUserName", Constants.USERNAME);
            }

            if (!Preferences.ContainsKey("NotifyPassword"))
            {
                Preferences.Set("NotifyPassword", Constants.PASSWORD);
            }

            Debug.WriteLine("User credentials saved in preferences");
        }

    }
}