using System;
using Notify.Core;
using Notify.Services.Ergast;
using Notify.Services.Information;
using Notify.ViewModels.Popups;
using Notify.ViewModels.TabViews;

namespace Notify.ViewModels
{
    public class ViewModelLocator
    {
        private readonly Lazy<HttpClientFactory> httpClientFactory;
        private readonly Lazy<IErgastService> ergastService;
        private readonly Lazy<IInformationService> informationService;

        public ViewModelLocator()
        {
            httpClientFactory = new Lazy<HttpClientFactory>(() => new HttpClientFactory());
            ergastService = new Lazy<IErgastService>(() => new ErgastService(httpClientFactory.Value));
            informationService = new Lazy<IInformationService>(() => new InformationService(httpClientFactory.Value));
        }

        public LoginPageViewModel LoginPage => new LoginPageViewModel();
        public RegistrationPageViewModel RegistrationPage => new RegistrationPageViewModel();
        public ProfilePageViewModel ProfilePage => new ProfilePageViewModel();
        public SettingsPageViewModel SettingsPage => new SettingsPageViewModel();
        public AccountSettingsPageViewModel AccountSettingsPage => new AccountSettingsPageViewModel();
        public NotificationSettingsPageViewModel NotificationSettingsPage => new NotificationSettingsPageViewModel();
        public WifiSettingsPageViewModel WifiSettingsPage => new WifiSettingsPageViewModel();
        public BluetoothSettingsPageViewModel BluetoothSettingsPage => new BluetoothSettingsPageViewModel();
        public NotificationCreationViewModel NotificationCreationPage => new NotificationCreationViewModel();

        
        public HomeViewModel HomeView => new HomeViewModel(ergastService.Value);
        public ScheduleViewModel ScheduleView => new ScheduleViewModel(ergastService.Value);
        public DriversViewModel DriversView => new DriversViewModel(ergastService.Value);
        public TeamsViewModel TeamsView => new TeamsViewModel(ergastService.Value);
        public HistoryViewModel HistoryView => new HistoryViewModel(ergastService.Value);
        
        public DriverDetailsPageViewModel DriverDetailsPage => new DriverDetailsPageViewModel(ergastService.Value, informationService.Value);
        public CircuitDetailsPageViewModel CircuitDetailsPage => new CircuitDetailsPageViewModel(ergastService.Value, informationService.Value);
        public CircuitLapsPageViewModel CircuitLapsPage => new CircuitLapsPageViewModel(ergastService.Value);
        public TeamDetailsPageViewModel TeamDetailsPage => new TeamDetailsPageViewModel(ergastService.Value, informationService.Value);
        
        public SeasonPopupPageViewModel SeasonPopupPage => new SeasonPopupPageViewModel();
        public RaceTypePopupPageViewModel RaceTypePopupPage => new RaceTypePopupPageViewModel();
    }
}
