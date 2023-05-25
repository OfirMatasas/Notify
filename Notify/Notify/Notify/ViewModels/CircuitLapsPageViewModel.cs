using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Notify.Models;
using Xamarin.CommunityToolkit.UI.Views;
using Xamarin.Forms;

namespace Notify.ViewModels
{
    public class CircuitLapsPageViewModel : BaseViewModel, IQueryAttributable
    {
        #region Fields
        
        #endregion

        #region Properties

        public RaceResultsLapByLapModel LapResults { get; set; }

        public LayoutState LapsState { get; set; }

        #endregion

        #region Commands

        public Command BackCommand { get; set; }

        #endregion

        #region Constructors

        public CircuitLapsPageViewModel()
        {
            BackCommand = new Command(BackCommandHandler);
        }

        #endregion

        #region Command Handlers

        private async void BackCommandHandler()
        {
            await Shell.Current.Navigation.PopAsync();
        }

        #endregion

        #region IQueryAttributable

        public async void ApplyQueryAttributes(IDictionary<string, string> query)
        {
            query.TryGetValue("round", out var roundParam);
            query.TryGetValue("driverId", out var driverIdParam);
            if (!string.IsNullOrEmpty(roundParam) && !string.IsNullOrEmpty(driverIdParam))
            {
                var round = Convert.ToInt32(roundParam);
                var driverId = driverIdParam.ToString();
                LapsState = LayoutState.Loading;
                await GetLapResults(round, driverId);
            }
        }

        #endregion

        #region Private Functionality

        private async Task GetLapResults(int round, string driverId)
        {
            LapResults = null;
            LapsState = LayoutState.Empty;
        }

        #endregion
    }
}
