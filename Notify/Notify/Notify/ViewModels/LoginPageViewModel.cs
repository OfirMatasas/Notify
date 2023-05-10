using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Threading.Tasks;
using Notify.Azure.HttpClient;
using Notify.Services.Location;
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

            await Shell.Current.GoToAsync("///login");

            locationService.SubscribeToLocationMessaging();
        }

        private async void onLoginClicked()
        {
            bool debugAutoLogin = false;
            IsBusy = true;

            if (debugAutoLogin)
            {
                try
                {
                    if (areCredentialsValid())
                    {
                        if (RememberMe)
                        {
                            storeUserCredentialsInPreferences(UserName, Password);
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

        private bool areCredentialsValid()
        {
            return AzureHttpClient.Instance.CheckIfCredentialsAreValid(UserName, Password);
        }

        private async void onSignUpClicked()
        {
            await Shell.Current.GoToAsync("///register");
        }

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void storeUserCredentialsInPreferences(string userName, string password)
        {
            Preferences.Set("NotifyUserName", userName);
            Preferences.Set("NotifyPassword", password);
            
            Debug.WriteLine("User credentials saved in preferences");
        }
    }
}
