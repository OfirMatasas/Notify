using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Notify.Models;
using Xamarin.CommunityToolkit.UI.Views;
using Xamarin.Forms;

namespace Notify.ViewModels.TabViews
{
    public class HomeViewModel: BaseViewModel
    {
        #region Fields


        private RaceEventModel _latestRace;

        #endregion

        #region Properties

        public Task Init { get; }

        public string LatestRace { get; set; }
        public ObservableCollection<RaceResultModel> LatestResults { get; set; }
        public ObservableCollection<RaceEventModel> UpcomingRaceEventList { get; set; }
        public ObservableCollection<DriverStadingsModel> DriversList { get; set; }
        public ObservableCollection<ConstructorStadingsModel> TeamsList { get; set; }

        public LayoutState ResultsState { get; set; }
        public LayoutState ScheduleState { get; set; }
        public LayoutState DriverStadingsState { get; set; }
        public LayoutState TeamStadingsState { get; set; }

        #endregion

        #region Commands

        public Command ProfileCommand { get; set; }
        public Command SeeDriverCommand { get; set; }
        public Command SeeMoreResultsCommand { get; set; }
        public Command SeeEventCommand { get; set; }
        public Command SeeMoreScheduleCommand { get; set; }
        public Command SeeDriversCommand { get; set; }
        public Command SeeTeamsCommand { get; set; }
        public Command DriverDetailsCommand { get; set; }
        public Command TeamDetailsCommand { get; set; }

        #endregion

        #region Constructors

        public HomeViewModel()
        {
            Title = "Home";
            ProfileCommand = new Command(ProfileCommandHandler);
            SeeDriverCommand = new Command<RaceResultModel>(SeeDriverCommandHandler);
            SeeMoreResultsCommand = new Command(SeeMoreResultsCommandHandler);
            SeeEventCommand = new Command<RaceEventModel>(SeeEventCommandHandler);
            SeeMoreScheduleCommand = new Command(SeeMoreScheduleCommandHandler);
            SeeDriversCommand = new Command(SeeDriversCommandHandler);
            SeeTeamsCommand = new Command(SeeTeamsCommandHandler);
            DriverDetailsCommand = new Command<DriverStadingsModel>(DriverDetailsCommandHandler);
            TeamDetailsCommand = new Command<ConstructorStadingsModel>(TeamDetailsCommandHandler);

            Init = Initialize();
        }

        #endregion


        #region Command Handlers

        private async void ProfileCommandHandler()
        {
            await Shell.Current.GoToAsync($"profile");
        }

        private async void SeeDriverCommandHandler(RaceResultModel raceResult)
        {
            var driver = new DriverStadingsModel()
            {
                Driver = raceResult.Driver,
                Constructors = new List<ConstructorModel>() { raceResult.Constructor }
            };
            await Shell.Current.GoToAsync($"//main/drivers/details?driver={driver.Driver.DriverId}");
        }

        private async void SeeMoreResultsCommandHandler()
        {
            await Shell.Current.GoToAsync($"//main/schedule/details?round={_latestRace.Round}&selectedTab=1");
        }

        private async void SeeEventCommandHandler(RaceEventModel raceEvent)
        {
            await Shell.Current.GoToAsync($"//main/schedule/details?round={raceEvent.Round}");
        }

        private async void SeeMoreScheduleCommandHandler()
        {
            await Shell.Current.GoToAsync($"//main/schedule");
        }

        private async void SeeDriversCommandHandler()
        {
            await Shell.Current.GoToAsync($"//main/drivers");
        }

        private async void SeeTeamsCommandHandler()
        {
            await Shell.Current.GoToAsync($"//main/teams");
        }

        private async void DriverDetailsCommandHandler(DriverStadingsModel driver)
        {
            await Shell.Current.GoToAsync($"//main/drivers/details?driver={driver.Driver.DriverId}");
        }

        private async void TeamDetailsCommandHandler(ConstructorStadingsModel team)
        {
            await Shell.Current.GoToAsync($"//main/teams/details?team={team.Constructor.ConstructorId}");
        }

        #endregion

        #region Private Functionality

        private async Task Initialize()
        {
            ResultsState = LayoutState.Loading;
            ScheduleState = LayoutState.Loading;
            DriverStadingsState = LayoutState.Loading;
            TeamStadingsState = LayoutState.Loading;
            await GetResults();
            await GetSchedule();
            await GetDriverStadings();
            await GetTeamStadings();
        }

        private async Task GetResults()
        {
            ResultsState = LayoutState.None;
        }

        private async Task GetSchedule()
        {
            UpcomingRaceEventList.Add(new RaceEventModel());
            ScheduleState = LayoutState.None;
        }

        private async Task GetDriverStadings()
        {
            DriversList = new ObservableCollection<DriverStadingsModel>(null);
            DriverStadingsState = LayoutState.None;
        }

        private async Task GetTeamStadings()
        {
            TeamsList = new ObservableCollection<ConstructorStadingsModel>(null);
            TeamStadingsState = LayoutState.None;
        }

        #endregion
    }
}
