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
    public class CircuitDetailsPageViewModel : BaseViewModel, IQueryAttributable
    {
        #region Fields

        private readonly IInformationService _informationsService;

        #endregion

        #region Properties

        public RaceEventResultsModel Results { get; set; }
        public RaceEventModel RaceEvent { get; set; }
        public CircuitBasicInformationsModel CircuitInformations { get; set; }
        public string SelectedRaceType { get; set; }
        public int SelectedTab { get; set; }

        public LayoutState ResultsState { get; set; }
        public LayoutState InformationsState { get; set; }

        #endregion

        #region Commands

        public Command BackCommand { get; set; }
        public Command SelectRaceTypeCommand { get; set; }
        public Command ViewLapByLapCommand { get; set; }

        #endregion

        #region Constructors

        public CircuitDetailsPageViewModel(IInformationService informationsService)
        {
            _informationsService = informationsService;

            BackCommand = new Command(BackCommandHandler);
            SelectRaceTypeCommand = new Command(SelectRaceTypeCommandHandler);
            ViewLapByLapCommand = new Command<RaceResultModel>(ViewLapByLapCommandHandler);
        }

        #endregion

        #region Command Handlers

        private async void BackCommandHandler()
        {
            await Shell.Current.Navigation.PopAsync();
        }

        private async void SelectRaceTypeCommandHandler()
        {
            var raceType = await Shell.Current.Navigation.ShowPopupAsync(new RaceTypePopupPage());
            if(raceType != null)
            {
                SelectedRaceType = raceType.ToString();
                ResultsState = LayoutState.Loading;
                await GetResults();
            }
        }
        private async void ViewLapByLapCommandHandler(RaceResultModel result)
        {
            await Shell.Current.GoToAsync($"laps?round={RaceEvent.Round}&driverId={result.Driver.DriverId}");
        }

        #endregion

        #region IQueryAttributable

        public async void ApplyQueryAttributes(IDictionary<string, string> query)
        {
            query.TryGetValue("round", out var roundParam);
            query.TryGetValue("selectedTab", out var selectedTabParam);
            if (!string.IsNullOrEmpty(roundParam))
            {
                var round = Convert.ToInt32(roundParam);
                MainState = LayoutState.Loading;
                SelectedRaceType = "Race";
                SelectedTab = string.IsNullOrEmpty(selectedTabParam) ? 0 : Convert.ToInt32(selectedTabParam);
                await GetRaceEvent(round);
            }
        }

        #endregion

        #region Private Functionality

        private async Task GetRaceEvent(int round)
        {
            RaceEventModel res = null;
            if (res != null)
            {
                RaceEvent = res;
                SelectedRaceType = "Race";
                MainState = LayoutState.None;
                ResultsState = LayoutState.Loading;
                InformationsState = LayoutState.Loading;
                await GetResults();
                await GetInformations();
            }
        }

        private async Task GetResults()
        {
            Results = null;
            if (SelectedRaceType == "Sprint")
            {
                ResultsState = LayoutState.Success;
            }
            else
            {
                ResultsState = LayoutState.Empty;
            }
        }

        private async Task GetInformations()
        {
            var res = await _informationsService.GetCircuitInformation(RaceEvent.Circuit.Location.Country);
            if (res != null)
            {
                CircuitInformations = res;
                InformationsState = LayoutState.None;
            }
            else
            {
                CircuitInformations = null;
                InformationsState = LayoutState.Empty;
            }
        }

        private string ConvertNameToRaceType(string name)
        {
            switch(name.ToLower())
            {
                case "race": return "results";
                case "qualification": return "qualifying";
                case "sprint": return "sprint";
                default: return "results";
            }
        }

        #endregion
    }
}
