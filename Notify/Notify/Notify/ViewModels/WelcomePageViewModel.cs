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
            
            manageLocationTracking();

            if (!debugAutoLogin)
            {
                try
                {
                    if (m_UserName.Equals(Constants.Username) && Password.Equals(Constants.Password))
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
        
        #endregion

        #region Private Functionality
        
        private void current_PositionChanged(object sender, Plugin.Geolocator.Abstractions.PositionEventArgs e)
        {
            Console.WriteLine($"{e.Position.Latitude}, {e.Position.Longitude}, {e.Position.Timestamp.TimeOfDay}");
        }
        
        private async Task manageLocationTracking()
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
                    CrossGeolocator.Current.PositionChanged -= current_PositionChanged;

                    return;
                }

                await CrossGeolocator.Current.StartListeningAsync(TimeSpan.FromSeconds(1), 10, false,
                    new Plugin.Geolocator.Abstractions.ListenerSettings
                    {
                        ActivityType = Plugin.Geolocator.Abstractions.ActivityType.AutomotiveNavigation,
                        AllowBackgroundUpdates = true,
                        DeferLocationUpdates = false,
                        DeferralDistanceMeters = 10,
                        DeferralTime = TimeSpan.FromSeconds(5),
                        ListenForSignificantChanges = true,
                        PauseLocationUpdatesAutomatically = true
                    });

                CrossGeolocator.Current.PositionChanged += current_PositionChanged;
            }
            else if (Device.RuntimePlatform == Device.Android)
            {
                if (Preferences.Get("LocationServiceRunning", false) == false)
                {
                    startService();
                }
                else
                {
                    stopService();
                }
            }
        }

        private async Task initialize()
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
            
            subscribeToLocationMessaging();
        }

        private void subscribeToLocationMessaging()
        {
            if (Device.RuntimePlatform == Device.Android)
            {
                MessagingCenter.Subscribe<LocationMessage>(this, "Location",
                    message =>
                    {
                        Device.BeginInvokeOnMainThread(() =>
                        {
                            Console.WriteLine(
                                $"{message.Latitude}, {message.Longitude}, {DateTime.Now.ToLongTimeString()}");
                        });
                    });

                MessagingCenter.Subscribe<StopServiceMessage>(this, "ServiceStopped",
                    message =>
                    {
                        Device.BeginInvokeOnMainThread(() => { Console.WriteLine("Location Service has been stopped!"); });
                    });

                MessagingCenter.Subscribe<LocationErrorMessage>(this, "LocationError",
                    message =>
                    {
                        Device.BeginInvokeOnMainThread(() => { Console.WriteLine("There was an error updating location!"); });
                    });

                if (Preferences.Get("LocationServiceRunning", false) == true)
                {
                    startService();
                }
            }
        }

        private void startService()
        {
            var startServiceMessage = new StartServiceMessage();
            MessagingCenter.Send(startServiceMessage, "ServiceStarted");
            Preferences.Set("LocationServiceRunning", true);
            Console.WriteLine("Location Service has been started!");
        }

        private void stopService()
        {
            var stopServiceMessage = new StopServiceMessage();
            MessagingCenter.Send(stopServiceMessage, "ServiceStopped");
            Preferences.Set("LocationServiceRunning", false);
        }

        #endregion
    }
}
