using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Newtonsoft.Json;
using Notify.Azure.HttpClient;
using Notify.Core;
using Xamarin.Essentials;
using Xamarin.Forms;
using Constants = Notify.Helpers.Constants;

namespace Notify.ViewModels
{
    public class FriendRequestPageViewModel : INotifyPropertyChanged
    {
        public Command BackCommand { get; set; }
        public Command ExecuteSearchCommand { get; set; }
        public Command<User> SendRequestCommand { get; set; }
        public Command<User> ShowFriendDetailsCommand { get; set; }
        public Command RefreshPotentialFriendsCommand { get; set; }
        public Command AcceptFriendRequestCommand { get; set; }
        public Command RejectFriendRequestCommand { get; set; }

        private string m_SearchText;
        public string SearchText
        {
            get => m_SearchText;
            set => SetField(ref m_SearchText, value);
        }
        
        private bool m_IsRefreshing;
        public bool IsRefreshing { set => SetField(ref m_IsRefreshing, value); }

        private List<User> UsersList { get; set; }
        
        private List<User> m_UsersSelectionList;
        public List<User> UsersSelectionList
        {
            get => m_UsersSelectionList;
            set
            {
                m_UsersSelectionList = value;
                OnPropertyChanged(nameof(UsersSelectionList));
            }
        }
        
        private List<FriendRequest> m_PendingFriendRequestsList;
        public List<FriendRequest> PendingFriendRequestsList
        {
            get => m_PendingFriendRequestsList;
            set
            {
                m_PendingFriendRequestsList = value;
                OnPropertyChanged(nameof(PendingFriendRequestsList));
            }
        }
        
        private List<FriendRequest> m_FilteredPendingFriendRequestsList;
        public List<FriendRequest> FilteredPendingFriendRequestsList
        {
            get => m_FilteredPendingFriendRequestsList;
            set
            {
                m_FilteredPendingFriendRequestsList = value;
                OnPropertyChanged(nameof(m_FilteredPendingFriendRequestsList));
            }
        }

        private User m_SelectedUserName;

        public FriendRequestPageViewModel()
        {
            BackCommand = new Command(onBackButtonClicked);
            SendRequestCommand = new Command<User>(onSendRequestButtonClicked);
            ExecuteSearchCommand = new Command(onSearchTextChanged);
            ShowFriendDetailsCommand = new Command<User>(onFriendClicked);
            RefreshPotentialFriendsCommand = new Command(onRefreshPotentialFriendsClicked);
            AcceptFriendRequestCommand = new Command<FriendRequest>(onAcceptFriendRequestClicked);
            RejectFriendRequestCommand = new Command<FriendRequest>(onRejectFriendRequestClicked);
            
            populateUsersList();
            populatePendingFriendRequestsList();
        }

        private async void onAcceptFriendRequestClicked(FriendRequest friendRequest)
        {
            bool isSucceeded = await AzureHttpClient.Instance.AcceptFriendRequest(friendRequest.UserName, friendRequest.Requester);
            
            if (isSucceeded)
            {
                PendingFriendRequestsList.Remove(friendRequest);
                FilteredPendingFriendRequestsList.Remove(friendRequest);
                onRefreshPotentialFriendsClicked();
            }
            else
            {
                App.Current.MainPage.DisplayAlert("Friend Request", $"Failed to accept friend request from {friendRequest.Requester}", "OK");
            }
        }
        
        private async void onRejectFriendRequestClicked(FriendRequest friendRequest)
        {
            bool isSucceeded = await AzureHttpClient.Instance.RejectFriendRequest(friendRequest.UserName, friendRequest.Requester);
            
            if (isSucceeded)
            {
                PendingFriendRequestsList.Remove(friendRequest);
                FilteredPendingFriendRequestsList.Remove(friendRequest);
                onRefreshPotentialFriendsClicked();
            }
            else
            {
                App.Current.MainPage.DisplayAlert("Friend Request", $"Failed to reject friend request from {friendRequest.Requester}", "OK");
            }
        }

        private async void onRefreshPotentialFriendsClicked()
        {
            IsRefreshing = true;
            
            UsersList = await AzureHttpClient.Instance.GetNotFriendsUsers();
            PendingFriendRequestsList = await AzureHttpClient.Instance.GetFriendRequests();
            onSearchTextChanged();
            
            IsRefreshing = false;
        }

        private void onSearchTextChanged()
        {
            FilterUsersList(SearchText);
        }

        private void FilterUsersList(string searchText)
        {
            if (string.IsNullOrWhiteSpace(searchText))
            {
                UsersSelectionList = UsersList;
                FilteredPendingFriendRequestsList = PendingFriendRequestsList;
            }
            else
            {
                UsersSelectionList = UsersList.FindAll(friend =>
                    friend.UserName.ToLower().Contains(searchText.ToLower().Trim()) ||
                    friend.Telephone.Contains(searchText.Trim()));

                FilteredPendingFriendRequestsList = PendingFriendRequestsList.FindAll(friendRequest =>
                    friendRequest.Requester.ToLower().Contains(searchText.ToLower().Trim()));
            }
        }
        
        private async void populatePendingFriendRequestsList()
        {
            string pendingFriendRequestsListJson = Preferences.Get(Constants.PREFERENCES_PENDING_FRIEND_REQUESTS, "");
            
            PendingFriendRequestsList = JsonConvert.DeserializeObject<List<FriendRequest>>(pendingFriendRequestsListJson);
            FilteredPendingFriendRequestsList = PendingFriendRequestsList = await AzureHttpClient.Instance.GetFriendRequests();        
        }

        private async void populateUsersList()
        {
            string usersListJson = Preferences.Get(Constants.PREFERENCES_NOT_FRIENDS_USERS, "");
            
            UsersList = JsonConvert.DeserializeObject<List<User>>(usersListJson);
            UsersSelectionList = UsersList = await AzureHttpClient.Instance.GetNotFriendsUsers();
        }
        
        private void onFriendClicked(User friend)
        {
            if (!(friend is null))
            {
                App.Current.MainPage.DisplayAlert("Friend Details", $"Name: {friend.Name}\nUsername: {friend.UserName}\nTelephone: {friend.Telephone}", "OK");
            }
        }

        private async void onSendRequestButtonClicked(User friend)
        {
            bool isSucceeded;
            
            if (!(friend is null))
            {
                isSucceeded = await AzureHttpClient.Instance.SendFriendRequest(friend.UserName);
                
                if (isSucceeded)
                {
                    App.Current.MainPage.DisplayAlert("Friend Request", $"Friend request sent to {friend.UserName}", "OK");
                    UsersList.Remove(friend);
                    UsersSelectionList.Remove(friend);
                    onRefreshPotentialFriendsClicked();
                }
                else
                {
                    App.Current.MainPage.DisplayAlert("Friend Request", $"Failed to send friend request to {friend.UserName}", "OK");
                }
            }
        }

        private async void onBackButtonClicked()
        {
            await Shell.Current.Navigation.PopAsync();
        }
        
        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void SetField<T>(ref T field, T value, [CallerMemberName] string propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(field, value)) return;
            field = value;
            OnPropertyChanged(propertyName);
        }
    }
}
