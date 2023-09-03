using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Notify.Azure.HttpClient;
using Notify.Core;
using Notify.Helpers;
using Notify.Services;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace Notify.ViewModels
{
    public sealed class PendingFriendRequestsPageViewModel : INotifyPropertyChanged
    {
        private readonly LoggerService r_Logger = LoggerService.Instance;
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
        
        public List<FriendRequest> PendingFriendRequests { get; set; }

        public PendingFriendRequestsPageViewModel()
        {
            FriendRequests = new ObservableCollection<FriendRequest>(); 
            LoadPendingFriendRequests();

            AcceptFriendRequestCommand = new Command<FriendRequest>(async request => await AcceptRequest(request));
            RejectFriendRequestCommand = new Command<FriendRequest>(async request => await RejectRequest(request));
            ShowFriendDetailsCommand = new Command<FriendRequest>(onFriendClicked);
            BackCommand = new Command(onBackButtonClicked);
            RefreshPendingRequestsList();
        }
        
        private void RefreshPendingRequestsList()
        {
            string requestsJson;
            
            try
            {
                requestsJson = Preferences.Get(Constants.PREFERENCES_PENDING_FRIEND_REQUESTS, string.Empty);
                
                if (!requestsJson.Equals(string.Empty))
                {
                    r_Logger.LogDebug("Pending friend requests found in preferences");
                    PendingFriendRequests = JsonConvert.DeserializeObject<List<FriendRequest>>(requestsJson);
                }
            }
            catch (Exception ex)
            {
                r_Logger.LogError(ex.Message);
            }
        }
        
        private void onFriendClicked(FriendRequest request)
        {
            App.Current.MainPage.DisplayAlert("Request Details", request.ToString() , "OK");
        }

        private async Task AcceptRequest(FriendRequest request)
        {
            await AzureHttpClient.Instance.AcceptFriendRequest(request.UserName, request.Requester);
        }

        private async Task RejectRequest(FriendRequest request)
        {
            await AzureHttpClient.Instance.RejectFriendRequest(request.UserName, request.Requester);
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
            List<FriendRequest> pendingFriendRequests;
            
            try
            {
                pendingFriendRequests = await AzureHttpClient.Instance.GetFriendRequests();
                
                foreach (FriendRequest friendRequest in pendingFriendRequests)
                {
                    FriendRequests.Add(friendRequest);
                }
            }
            catch (Exception ex)
            {
                r_Logger.LogError($"Error occured on LoadPendingRequests: {ex.Message}");
            }
            
            Preferences.Set(Constants.PREFERENCES_PENDING_FRIEND_REQUESTS, JsonConvert.SerializeObject(FriendRequests));
        }
    }
}
