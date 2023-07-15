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
        public Command SearchTextChangedCommand { get; set; }
        
        public Command SendRequestCommand { get; set; }

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
        public Friend SelectedUserName
        {
            get => m_SelectedUserName;
            set
            {
                m_SelectedUserName = value;
                OnPropertyChanged(nameof(SelectedUserName));
            }
        }

        public FriendRequestPageViewModel()
        {
            BackCommand = new Command(onBackButtonClicked);
            SendRequestCommand = new Command(onSendRequestButtonClicked);
            SearchTextChangedCommand = new Command(onSearchTextChanged);
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

        private void onSendRequestButtonClicked()
        {
            if (SelectedUserName is null)
            {
                App.Current.MainPage.DisplayAlert("Error", "Please select a user to send a request to", "OK");
            }
            else
            {
                AzureHttpClient.Instance.SendFriendRequest(SelectedUserName.UserName);
                App.Current.MainPage.DisplayAlert("Friend Request Sent", $"Friend request sent to {SelectedUserName.UserName}", "OK");
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

        protected bool SetField<T>(ref T field, T value, [CallerMemberName] string propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(field, value)) return false;
            field = value;
            OnPropertyChanged(propertyName);
            return true;
        }
    }
}
