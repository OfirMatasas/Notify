using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Notify.Azure.HttpClient;
using Notify.Core;
using Notify.Helpers;
using Notify.Views.Views;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace Notify.ViewModels
{
    public class FriendsPageViewModel : INotifyPropertyChanged
    {
        private readonly LoggerService r_Logger = LoggerService.Instance;

        #region Commands
        
        public Command ShowFriendRequestsCommand { get; set; }
        public Command SelectedFriendCommand { get; set; }

        #endregion

        #region Members

        public List<Friend> Friends { get; set; }
        public Friend SelectedFriend { get; set; }

        #endregion
        
        #region Constructor

        public FriendsPageViewModel()
        {
            RefreshFriendsCommand = new Command(onRefreshFriendsClicked);
            ShowFriendRequestsCommand = new Command(onShowFriendRequestsClicked);
            SelectedFriendCommand = new Command(onSelectedFriendClicked);

            RefreshFriendsList();
            onRefreshFriendsClicked();
        }

        private void RefreshFriendsList()
        {
            string friendsJson;
            
            try
            {
                friendsJson = Preferences.Get(Constants.PREFERENCES_FRIENDS, string.Empty);
                
                if (!friendsJson.Equals(string.Empty))
                {
                    r_Logger.LogDebug("Friends found in preferences");
                    Friends = JsonConvert.DeserializeObject<List<Friend>>(friendsJson);
                }
            }
            catch (Exception ex)
            {
                r_Logger.LogError(ex.Message);
            }
        }

        #endregion

        #region Refresh_Friends

        public Command RefreshFriendsCommand { get; set; }

        private async void onRefreshFriendsClicked()
        {
            await Task.Run(() => Friends = AzureHttpClient.Instance.GetFriends().Result);
        }
        
        #endregion
        private async void onSelectedFriendClicked()
        {
            await Shell.Current.Navigation.PushAsync(new FriendDetailsPage(SelectedFriend));
        }

        private void onShowFriendRequestsClicked()
        {
            //TODO: Show friend requests
        }
        
        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        protected bool SetField<T>(ref T field, T value, [CallerMemberName] string propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(field, value)) return false;
            field = value;
            OnPropertyChanged(propertyName);
            return true;
        }
    }
}
