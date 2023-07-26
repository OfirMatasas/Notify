using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Input;
using Notify.Azure.HttpClient;
using Notify.Core;
using Notify.Helpers;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace Notify.ViewModels
{
    public sealed class PendingFriendRequestsPageViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        
        public Command AcceptFriendRequestCommand { get; set; }
        public Command RejectFriendRequestCommand { get; set; }
        public Command BackCommand { get; set; }
        public Command ShowFriendDetailsCommand { get; set; }
        
        private ObservableCollection<FriendRequest> m_FriendRequests;
        public ObservableCollection<FriendRequest> FriendRequests
        {
            get => m_FriendRequests;
            set
            {
                m_FriendRequests = value;
                OnPropertyChanged();
            }
        }
        
        public PendingFriendRequestsPageViewModel()
        {
            FriendRequests = new ObservableCollection<FriendRequest>(); 
            LoadPendingFriendRequests();

            AcceptFriendRequestCommand = new Command<FriendRequest>(async request => await AcceptRequest(request));
            RejectFriendRequestCommand = new Command<FriendRequest>(async request => await RejectRequest(request));
            ShowFriendDetailsCommand = new Command<FriendRequest>(onFriendClicked);
            BackCommand = new Command(onBackButtonClicked);
        }
        
        
        private void onFriendClicked(FriendRequest request)
        {
            App.Current.MainPage.DisplayAlert("Request Details", request.ToString() , "OK");
        }

        private async Task AcceptRequest(FriendRequest request)
        {
            Debug.WriteLine("in AcceptRequest method");
        }

        private async Task RejectRequest(FriendRequest request)
        {
            Debug.WriteLine("in RejectRequest method");
        }
        
        private async void onBackButtonClicked()
        {
            await Shell.Current.Navigation.PopAsync();
        }

        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private async void LoadPendingFriendRequests()
        {
            string userName;
            List<FriendRequest> pendingFriendRequests;
            try
            {
                userName = Preferences.Get(Constants.PREFERENCES_USERNAME, "");
                pendingFriendRequests = await AzureHttpClient.Instance.GetPendingFriendRequests(userName);
                foreach (FriendRequest friendRequest in pendingFriendRequests)
                {
                    FriendRequests.Add(friendRequest);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error occured on LoadPendingRequests: {ex.Message}");
            }
        }
    }
}