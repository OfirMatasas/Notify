using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Notify.Models;
using Notify.Services.Information;
using Notify.Views.Popups;
using Xamarin.CommunityToolkit.Extensions;
using Xamarin.CommunityToolkit.UI.Views;
using Xamarin.Forms;

namespace Notify.ViewModels
{
    public class DriverDetailsPageViewModel: BaseViewModel, IQueryAttributable
    {
        #region Fields

        private readonly IInformationService _informationsService;

        #endregion

        #region Properties
        
        public ObservableCollection<RaceEventModel> RaceResults { get; set; }
        public DriverModel Driver { get; set; }
        public DriverBasicInformationsModel DriverInformations { get; set; }
        public string SelectedSeason { get; set; }

        public LayoutState ResultsState { get; set; }
        public LayoutState InformationsState { get; set; }

        #endregion

        #region Commands

        public Command BackCommand { get; set; }
        public Command SelectSeasonCommand { get; set; }

        #endregion

        #region Constructors

        public DriverDetailsPageViewModel(IInformationService informationsService)
        {
            _informationsService = informationsService;

            BackCommand = new Command(BackCommandHandler);
            SelectSeasonCommand = new Command(SelectSeasonCommandHandler);
        }

        #endregion

        #region Command Handlers

        private async void BackCommandHandler()
        {
            await Shell.Current.GoToAsync("..");
        }

        private async void SelectSeasonCommandHandler()
        {
            var season = await Shell.Current.Navigation.ShowPopupAsync(new SeasonPopupPage());
            if (season != null)
            {
                SelectedSeason = season.ToString() == DateTime.Now.Year.ToString() ? "Current Season" : season.ToString();
                ResultsState = LayoutState.Loading;
                await GetResults();            
            }
        }

        #endregion

        #region IQueryAttributable

        public async void ApplyQueryAttributes(IDictionary<string, string> query)
        {
            query.TryGetValue("driver", out var driverParam);
            var driver = driverParam.ToString();
            if (!string.IsNullOrEmpty(driver))
            {
                MainState = LayoutState.Loading;
                await GetDriver(driver);
            }
        }

        #endregion

        #region Private Functionality

        private async Task GetDriver(string driver)
        {
            
        }

        private async Task GetResults()
        { 
            RaceResults = null;
            ResultsState = LayoutState.Empty;
        }

        private async Task GetInformations()
        {
            var res = await _informationsService.GetDriverInformation(string.Format("{0}-{1}", Driver.GivenName.ToLower(), Driver.FamilyName.ToLower()));
            if (res != null)
            {
                DriverInformations = res;
                InformationsState = LayoutState.None;
            }
            else
            {
                DriverInformations = null;
                InformationsState = LayoutState.Empty;
            }
        }

        #endregion
    }
}
