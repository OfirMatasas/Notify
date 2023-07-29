using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
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
        public Command SearchTextChangedCommand { get; set; }
        public Command<Friend> SendRequestCommand { get; set; }
        public Command<Friend> ShowFriendDetailsCommand { get; set; }

        private string m_SearchText;
        public string SearchText
        {
            get => m_SearchText;
            set => SetField(ref m_SearchText, value);
        }

        private List<Friend> UsersList { get; set; }
        
        private List<Friend> m_UsersSelectionList;

        public List<Friend> UsersSelectionList
        {
            get => m_UsersSelectionList;
            set
            {
                m_UsersSelectionList = value;
                OnPropertyChanged(nameof(UsersSelectionList));
            }
        }

        private Friend m_SelectedUserName;

        public FriendRequestPageViewModel()
        {
            BackCommand = new Command(onBackButtonClicked);
            SendRequestCommand = new Command<Friend>(onSendRequestButtonClicked);
            SearchTextChangedCommand = new Command(onSearchTextChanged);
            ShowFriendDetailsCommand = new Command<Friend>(onFriendClicked);
            PopulateUsersList();
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
            }
            else
            {
                UsersSelectionList = UsersList.FindAll(friend => 
                    friend.UserName.ToLower().Contains(searchText.ToLower().Trim()) || 
                    friend.Telephone.Contains(searchText.Trim()));
            }
        }

        private async void PopulateUsersList()
        {
            string usersListJson = Preferences.Get(Constants.PREFERENCES_NOT_FRIENDS_USERS, "");
            
            UsersList = JsonConvert.DeserializeObject<List<Friend>>(usersListJson);
            UsersSelectionList = UsersList = await AzureHttpClient.Instance.GetNotFriendsUsers();
        }
        
        private void onFriendClicked(Friend friend)
        {
            if (!(friend is null))
            {
                App.Current.MainPage.DisplayAlert("Friend Details", $"Name: {friend.Name}\nUsername: {friend.UserName}\nTelephone: {friend.Telephone}", "OK");
            }
        }

        private void onSendRequestButtonClicked(Friend friend)
        {
            if (!(friend is null))
            {
                AzureHttpClient.Instance.SendFriendRequest(friend.UserName);
                App.Current.MainPage.DisplayAlert("Friend Request Sent", $"Friend request sent to {friend.UserName}",
                    "OK");
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
