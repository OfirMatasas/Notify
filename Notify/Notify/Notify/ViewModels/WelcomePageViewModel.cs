using System;
using System.Threading.Tasks;
using Notify.Views.TabViews;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace Notify.ViewModels
{
    public class WelcomePageViewModel : BaseViewModel
    {
        #region Commands

        public Command LogInCommand { get; set; }

        #endregion

        #region Properties

        public Task Init { get; }
        
        private string userName;
        private string password;
        public string UserName
        {
            get => userName;
            set => SetProperty(ref userName, value);
        }

        public string Password
        {
            get => password;
            set => SetProperty(ref password, value);
        }

        #endregion

        #region Constructors

        public WelcomePageViewModel()
        {
            LogInCommand = new Command(OnLoginClicked);
            
            Init = Initialize();
        }

        #endregion

        #region Command Handlers

        private async void OnLoginClicked()
        {
            IsBusy = true;
            try
            {
                if (userName.Equals("lin") && Password.Equals("123"))
                {
                    await Shell.Current.GoToAsync("///main");
                }
                else
                {
                    await App.Current.MainPage.DisplayAlert("Error", "Invalid credentials", "OK");
                }

            }
            catch (Exception e)
            {
                await App.Current.MainPage.DisplayAlert("Error", "Empty credentials", "OK");
            }

            IsBusy = false;
        }

        #endregion

        #region Private Functionality

        private async Task Initialize()
        {
            VersionTracking.Track();
            if (VersionTracking.IsFirstLaunchEver)
            {
                await Shell.Current.GoToAsync("///welcome");
            }
            else
            {
                await Shell.Current.GoToAsync("///welcome");
            }
        }

        #endregion
    }
}
