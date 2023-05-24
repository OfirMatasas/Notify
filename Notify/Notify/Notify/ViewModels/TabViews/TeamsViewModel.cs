using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Notify.Models;
using Xamarin.CommunityToolkit.UI.Views;
using Xamarin.Forms;

namespace Notify.ViewModels.TabViews
{
    public class TeamsViewModel: BaseViewModel
    {
        #region Fields
        
        #endregion

        #region Properties

        public Task Init { get; }

        public ObservableCollection<ConstructorStadingsModel> TeamsList { get; set; }

        #endregion

        #region Commands

        public Command TeamDetailsCommand { get; set; }

        #endregion

        #region Constructors

        public TeamsViewModel()
        {
            Title = "Team";
            
            TeamDetailsCommand = new Command<ConstructorStadingsModel>(TeamDetailsCommandHandler);

            Init = Initialize();
        }

        #endregion

        #region Command Handlers

        private async void TeamDetailsCommandHandler(ConstructorStadingsModel team)
        {
            await Shell.Current.GoToAsync($"/details?team={team.Constructor.ConstructorId}");
        }

        #endregion

        #region Private Functionality

        private async Task Initialize()
        {
            MainState = LayoutState.Loading;
            TeamsList = new ObservableCollection<ConstructorStadingsModel>(null);
            MainState = LayoutState.None;
        }

        #endregion
    }
}
