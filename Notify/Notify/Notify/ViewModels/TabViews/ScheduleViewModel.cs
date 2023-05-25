using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Notify.Models;
using Xamarin.CommunityToolkit.UI.Views;
using Xamarin.Forms;

namespace Notify.ViewModels.TabViews
{
    public class ScheduleViewModel: BaseViewModel
    {
        #region Fields
        
        #endregion

        #region Properties

        public Task Init { get; }

        public ObservableCollection<RaceEventModel> UpcomingRaceEventList { get; set; }
        public ObservableCollection<RaceEventModel> PastRaceEventList { get; set; }

        #endregion

        #region Commands

        public Command CircuitDetailsCommand { get; set; }

        #endregion

        #region Constructors

        public ScheduleViewModel()
        {
            Title = "Schedule";

            CircuitDetailsCommand = new Command<RaceEventModel>(CircuitDetailsCommandHandler);

            Init = Initialize();
        }

        #endregion

        #region Command Handlers

        private async void CircuitDetailsCommandHandler(RaceEventModel circuit)
        {
            await Shell.Current.GoToAsync($"/schedule/details?round={circuit.Round}");
        }

        #endregion

        #region Private Functionality

        private async Task Initialize()
        {
            MainState = LayoutState.Loading;
            UpcomingRaceEventList = new ObservableCollection<RaceEventModel>(null);
            PastRaceEventList = new ObservableCollection<RaceEventModel>(null);
            MainState = LayoutState.None;
        }

        #endregion
    }
}
