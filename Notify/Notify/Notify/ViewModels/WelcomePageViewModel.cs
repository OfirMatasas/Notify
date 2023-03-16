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

        public Task Init { get; }

        private string userName;
        private string password;
        public string UserName
        {
            get => userName;
            set => SetProperty(ref userName, value);
        }

        public string Password
        {
            get => password;
            set => SetProperty(ref password, value);
        }

        #endregion

        #region Constructors

        public WelcomePageViewModel()
        {
            LogInCommand = new Command(OnLoginClicked);

            Init = Initialize();
        }

        #endregion

        #region Command Handlers

        private async void OnLoginClicked()
        {
            permis();
            IsBusy = true;
            bool debugAutoLogin = true;

            if (!debugAutoLogin)
            {
                try
                {
                    if (userName.Equals(Constants.Username) && Password.Equals(Constants.Password))
                    {
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
                await Shell.Current.GoToAsync("///main");
            }
        }
        
        private void Current_PositionChanged(object sender, Plugin.Geolocator.Abstractions.PositionEventArgs e)
        {
            Console.WriteLine($"{e.Position.Latitude}, {e.Position.Longitude}, {e.Position.Timestamp.TimeOfDay}");
        }

        private async void permis()
        {
            var permission = await Permissions.RequestAsync<Permissions.LocationAlways>();

            if (permission == PermissionStatus.Denied)
            {
                // TODO Let the user know they need to accept
                return;
            }

            if (Device.RuntimePlatform == Device.iOS)
            {
                if (CrossGeolocator.Current.IsListening)
                {
                    await CrossGeolocator.Current.StopListeningAsync();
                    CrossGeolocator.Current.PositionChanged -= Current_PositionChanged;

                    return;
                }

                await CrossGeolocator.Current.StartListeningAsync(TimeSpan.FromSeconds(1), 10, false, new Plugin.Geolocator.Abstractions.ListenerSettings
                {
                    ActivityType = Plugin.Geolocator.Abstractions.ActivityType.AutomotiveNavigation,
                    AllowBackgroundUpdates = true,
                    DeferLocationUpdates = false,
                    DeferralDistanceMeters = 10,
                    DeferralTime = TimeSpan.FromSeconds(5),
                    ListenForSignificantChanges = true,
                    PauseLocationUpdatesAutomatically = true
                });

                CrossGeolocator.Current.PositionChanged += Current_PositionChanged;
            }
            else if (Device.RuntimePlatform == Device.Android)
            {
                if (Preferences.Get("LocationServiceRunning", false) == false)
                {
                    StartService();
                }
                else
                {
                    StopService();
                }
            }
        }

        #endregion

        #region Private Functionality

        private async Task Initialize()
        {
            VersionTracking.Track();
            if (VersionTracking.IsFirstLaunchEver)
            {
                await Shell.Current.GoToAsync("///welcome");
            }
            else
            {
                await Shell.Current.GoToAsync("///welcome");
            }
            
            if (Device.RuntimePlatform == Device.Android)
            {
                MessagingCenter.Subscribe<LocationMessage>(this, "Location", message => {
                    Device.BeginInvokeOnMainThread(() => {

                        Console.WriteLine($"{message.Latitude}, {message.Longitude}, {DateTime.Now.ToLongTimeString()}");
                    });
                });

                MessagingCenter.Subscribe<StopServiceMessage>(this, "ServiceStopped", message => {
                    Device.BeginInvokeOnMainThread(() => {
                        Console.WriteLine("Location Service has been stopped!");
                    });
                });

                MessagingCenter.Subscribe<LocationErrorMessage>(this, "LocationError", message => {
                    Device.BeginInvokeOnMainThread(() => {
                        Console.WriteLine("There was an error updating location!");
                    });
                });

                if (Preferences.Get("LocationServiceRunning", false) == true)
                {
                    StartService();
                }
            }
        }
        
        private void StartService()
        {
            var startServiceMessage = new StartServiceMessage();
            MessagingCenter.Send(startServiceMessage, "ServiceStarted");
            Preferences.Set("LocationServiceRunning", true);
            Console.WriteLine("Location Service has been started!");
        }

        private void StopService()
        {
            var stopServiceMessage = new StopServiceMessage();
            MessagingCenter.Send(stopServiceMessage, "ServiceStopped");
            Preferences.Set("LocationServiceRunning", false);
        }

        #endregion
    }
}
