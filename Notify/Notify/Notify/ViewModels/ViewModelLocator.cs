using Notify.ViewModels.Popups;
using Notify.ViewModels.TabViews;

namespace Notify.ViewModels
{
    public class ViewModelLocator
    {
        public ViewModelLocator()
        {
            //httpClientFactory = new Lazy<HttpClientFactory>(() => new HttpClientFactory());
        }

        public LoginPageViewModel LoginPage => new LoginPageViewModel();
        public RegistrationPageViewModel RegistrationPage => new RegistrationPageViewModel();
        public ProfilePageViewModel ProfilePage => new ProfilePageViewModel();
        public SettingsPageViewModel SettingsPage => new SettingsPageViewModel();
        public LocationSettingsPageViewModel AccountSettingsPage => new LocationSettingsPageViewModel();
        public NotificationSettingsPageViewModel NotificationSettingsPage => new NotificationSettingsPageViewModel();
        public WifiSettingsPageViewModel WifiSettingsPage => new WifiSettingsPageViewModel();
        public BluetoothSettingsPageViewModel BluetoothSettingsPage => new BluetoothSettingsPageViewModel();
        public NotificationCreationViewModel NotificationCreationPage => new NotificationCreationViewModel();
        public NotificationsPageViewModel NotificationPage => new NotificationsPageViewModel();
        public FriendsPageViewModel FriendsPage => new FriendsPageViewModel();

        public HomeViewModel HomeView => new HomeViewModel();
        public ScheduleViewModel ScheduleView => new ScheduleViewModel();
        public DriversViewModel DriversView => new DriversViewModel();
        public TeamsViewModel TeamsView => new TeamsViewModel();
        public HistoryViewModel HistoryView => new HistoryViewModel();
        
        public DriverDetailsPageViewModel DriverDetailsPage => new DriverDetailsPageViewModel();
        public CircuitDetailsPageViewModel CircuitDetailsPage => new CircuitDetailsPageViewModel();
        public CircuitLapsPageViewModel CircuitLapsPage => new CircuitLapsPageViewModel();
        public TeamDetailsPageViewModel TeamDetailsPage => new TeamDetailsPageViewModel();
        
        public SeasonPopupPageViewModel SeasonPopupPage => new SeasonPopupPageViewModel();
        public RaceTypePopupPageViewModel RaceTypePopupPage => new RaceTypePopupPageViewModel();
    }
}
