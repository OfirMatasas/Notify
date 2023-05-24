using System;
using Notify.Core;
using Notify.Services.Information;
using Notify.ViewModels.Popups;
using Notify.ViewModels.TabViews;
using Notify.Views;

namespace Notify.ViewModels
{
    public class ViewModelLocator
    {
        private readonly Lazy<HttpClientFactory> httpClientFactory;
        private readonly Lazy<IInformationService> informationService;

        public ViewModelLocator()
        {
            httpClientFactory = new Lazy<HttpClientFactory>(() => new HttpClientFactory());
            informationService = new Lazy<IInformationService>(() => new InformationService(httpClientFactory.Value));
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
        
        public DriverDetailsPageViewModel DriverDetailsPage => new DriverDetailsPageViewModel(informationService.Value);
        public CircuitDetailsPageViewModel CircuitDetailsPage => new CircuitDetailsPageViewModel(informationService.Value);
        public CircuitLapsPageViewModel CircuitLapsPage => new CircuitLapsPageViewModel();
        public TeamDetailsPageViewModel TeamDetailsPage => new TeamDetailsPageViewModel(informationService.Value);
        
        public SeasonPopupPageViewModel SeasonPopupPage => new SeasonPopupPageViewModel();
        public RaceTypePopupPageViewModel RaceTypePopupPage => new RaceTypePopupPageViewModel();
    }
}
