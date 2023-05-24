using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Notify.Models;
using Xamarin.CommunityToolkit.UI.Views;
using Xamarin.Forms;

namespace Notify.ViewModels.TabViews
{
    public class DriversViewModel : BaseViewModel
    {
        #region Fields


        #endregion

        #region Properties

        public Task Init { get; }

        public ObservableCollection<DriverStadingsModel> DriversList { get; set; }

        #endregion

        #region Commands

        public Command DriverDetailsCommand { get; set; }

        #endregion

        #region Constructors

        public DriversViewModel()
        {
            Title = "Driver";

            DriverDetailsCommand = new Command<DriverStadingsModel>(DriverDetailsCommandHandler);

            Init = Initialize();
        }

        #endregion

        #region Command Handlers

        private async void DriverDetailsCommandHandler(DriverStadingsModel driver)
        {
            await Shell.Current.GoToAsync($"/details?driver={driver.Driver.DriverId}");
        }

        #endregion

        #region Private Functionality

        private async Task Initialize()
        {
            MainState = LayoutState.Loading;
            MainState = LayoutState.None;
        }

        #endregion
    }
}
