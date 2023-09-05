using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Notify.Azure.HttpClient;
using Notify.Core;
using Notify.Helpers;
using Notify.Services;
using Notify.ViewModels.Popups;
using Notify.Views.SubViews;
using Notify.Views.Views;
using Rg.Plugins.Popup.Services;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace Notify.ViewModels
{
    public class FriendsPageViewModel : INotifyPropertyChanged
    {
        private readonly LoggerService r_Logger = LoggerService.Instance;
        public List<User> Friends { get; set; }
        public List<User> FilteredFriends { get; set; }
        public User SelectedFriend { get; set; }
        
        public Command ShowFriendRequestsCommand { get; set; }
        public Command SelectedFriendCommand { get; set; }
        public Command DeleteFriendCommand { get; set; }
        public Command ShowPendingFriendRequestsCommand { get; set; }
        public Command ExecuteSearchCommand { get; set; }
        public Command EditFriendCommand { get; set; }
        
        private bool m_IsRefreshing;
        public bool IsRefreshing { set => SetField(ref m_IsRefreshing, value); }
        
        private string m_SearchFriendsInput;
        public string SearchFriendsInput
        {
            get => m_SearchFriendsInput;
            set
            {
                SetField(ref m_SearchFriendsInput, value);
                applyFilterAndSearch();
            }
        }
        
        public FriendsPageViewModel()
        {
            ShowPendingFriendRequestsCommand = new Command(onShowPendingFriendRequestsClicked);
            RefreshFriendsCommand = new Command(onRefreshFriendsClicked);
            DeleteFriendCommand = new Command<User>(onDeleteFriendButtonClicked);
            ShowFriendRequestsCommand = new Command(onShowFriendRequestsClicked);
            SelectedFriendCommand = new Command(onSelectedFriendClicked);
            ExecuteSearchCommand = new Command(applyFilterAndSearch);
            EditFriendCommand = new Command<User>(onEditFriendButtonClicked);
            
            RefreshFriendsList();
            onRefreshFriendsClicked();
        }

        private void onEditFriendButtonClicked(User friend)
        {
            string newLocationPermission, newTimePermission, newDynamicPermission;
            string selectedLocationPermission = friend.Permissions.LocationNotificationPermission;
            string selectedTimePermission = friend.Permissions.TimeNotificationPermission;
            string selectedDynamicPermission = friend.Permissions.DynamicNotificationPermission;
            EditFriendPopupPage popup = new EditFriendPopupPage(selectedLocationPermission, selectedTimePermission, selectedDynamicPermission);
            bool isSucceeded;

            MessagingCenter.Subscribe<EditFriendPopupPage, (string, string, string)>(this, "EditFriendValues",
                async (sender, newPermissionValues) =>
                {
                    newLocationPermission = newPermissionValues.Item1 ?? selectedLocationPermission;
                    newTimePermission = newPermissionValues.Item2 ?? selectedTimePermission;
                    newDynamicPermission = newPermissionValues.Item3 ?? selectedDynamicPermission;

                    if (newLocationPermission != selectedLocationPermission || newTimePermission != selectedTimePermission || newDynamicPermission != selectedDynamicPermission)
                    {
                        isSucceeded = await AzureHttpClient.Instance.UpdateFriendPermissionsAsync(friend.UserName,
                            newLocationPermission, newTimePermission, newDynamicPermission);

                        if (!isSucceeded)
                        {
                            await Application.Current.MainPage.DisplayAlert("Error",
                                "There was a problem updating user permissions.", "OK");

                        }

                        onRefreshFriendsClicked();
                    }

                    MessagingCenter
                        .Unsubscribe<EditFriendPopupPage, (string, string, string)>(this, "EditFriendValues");

                    Device.BeginInvokeOnMainThread(async () =>
                    {
                        if (PopupNavigation.Instance.PopupStack.Count > 0)
                        {
                            await PopupNavigation.Instance.PopAsync();
                        }
                    });
                });

            Device.BeginInvokeOnMainThread(async () => { await PopupNavigation.Instance.PushAsync(popup); });
        }

        private async void RefreshFriendsList()
        {
            string friendsJson;
            
            try
            {
                friendsJson = Preferences.Get(Constants.PREFERENCES_FRIENDS, string.Empty);
                
                if (!friendsJson.Equals(string.Empty))
                {
                    r_Logger.LogDebug("Friends found in preferences");
                    Friends = JsonConvert.DeserializeObject<List<User>>(friendsJson);
                    FilteredFriends = new List<User>(Friends);
                }
                else
                {
                    Friends = await AzureHttpClient.Instance.GetFriends();
                    FilteredFriends = new List<User>(Friends);
                }
            }
            catch (Exception ex)
            {
                r_Logger.LogError(ex.Message);
            }
        }

        public Command RefreshFriendsCommand { get; set; }

        private async void onRefreshFriendsClicked()
        {
            IsRefreshing = true;

            await Task.Run(() => Friends = AzureHttpClient.Instance.GetFriends().Result);
            applyFilterAndSearch();
            
            IsRefreshing = false;
        }
        
        private void applyFilterAndSearch()
        {
            FilteredFriends.Clear();
    
            if (!string.IsNullOrWhiteSpace(SearchFriendsInput))
            {
                FilteredFriends = Friends.Where(friend =>
                {
                    bool containedInUsername = friend.UserName.ToLower().Contains(SearchFriendsInput.ToLower());
                    bool containedInTelephone = friend.Telephone.Contains(SearchFriendsInput);

                    return containedInUsername || containedInTelephone;
                }).ToList();
            }
            else
            {
                FilteredFriends = new List<User>(Friends);
            }
    
            OnPropertyChanged(nameof(FilteredFriends));
        }
        
        private async void onSelectedFriendClicked()
        {
            await Shell.Current.Navigation.PushAsync(new FriendDetailsPage(SelectedFriend));
        }

        private async void onShowFriendRequestsClicked()
        {
            await Shell.Current.Navigation.PushAsync(new FriendRequestPage());
        }
        
        private async void onDeleteFriendButtonClicked(User friend)
        {
            string messageTitle = "Friend Deletion";
            bool isDeleted;
            bool isConfirmed = await App.Current.MainPage.DisplayAlert(messageTitle,
                $"Are you sure you want to delete {friend.UserName} from your friends list?",
                "Yes", "No");

            if (isConfirmed)
            {
                isDeleted = await AzureHttpClient.Instance.DeleteFriendAsync(friend.UserName);

                if (isDeleted)
                {
                    onRefreshFriendsClicked();
                }
                else
                {
                    await App.Current.MainPage.DisplayAlert(messageTitle,
                        "Failed to delete friend",
                        "OK");
                }
            }
        }
        
        private async void onShowPendingFriendRequestsClicked()
        {
            await Shell.Current.Navigation.PushAsync(new PendingFriendRequestsPage());
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
