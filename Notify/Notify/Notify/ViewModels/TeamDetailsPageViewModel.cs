using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Notify.Models;
using Notify.Views.Popups;
using Xamarin.CommunityToolkit.Extensions;
using Xamarin.CommunityToolkit.UI.Views;
using Xamarin.Forms;

namespace Notify.ViewModels
{
    public class TeamDetailsPageViewModel : BaseViewModel, IQueryAttributable
    {
        #region Fields
        
        #endregion

        #region Properties

        public ObservableCollection<RaceEventModel> RaceResults { get; set; }
        public ConstructorModel Constructor { get; set; }
        public ConstructorBasicInformationsModel ConstructorInformations { get; set; }
        public string SelectedSeason { get; set; }

        public LayoutState ResultsState { get; set; }
        public LayoutState InformationsState { get; set; }

        #endregion

        #region Commands

        public Command BackCommand { get; set; }
        public Command SelectSeasonCommand { get; set; }

        #endregion

        #region Constructors

        public TeamDetailsPageViewModel()
        {
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
            }
        }

        #endregion

        #region IQueryAttributable

        public async void ApplyQueryAttributes(IDictionary<string, string> query)
        {
            query.TryGetValue("team", out var teamParam);
            var team = teamParam.ToString();
            if (!string.IsNullOrEmpty(team))
            {
                MainState = LayoutState.Loading;
                await GetTeam(team);
            }
        }

        #endregion

        #region Private Functionality

        private async Task GetTeam(string team)
        {
        }

        private async Task GetResults()
        {
            RaceResults = null;
            ResultsState = LayoutState.Empty;
        }

        private async Task GetInformations()
        {
            ConstructorInformations = null;
            InformationsState = LayoutState.Empty;
        }

        #endregion
    }
}
