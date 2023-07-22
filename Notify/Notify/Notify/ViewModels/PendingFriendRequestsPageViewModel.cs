using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Input;
using Notify.Core;
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
            get { return m_FriendRequests; }
            set
            {
                if (m_FriendRequests != value)
                {
                    m_FriendRequests = value;
                    OnPropertyChanged();
                }
            }
        }
        
        public PendingFriendRequestsPageViewModel()
        {
            FriendRequests = new ObservableCollection<FriendRequest>
            {
                new FriendRequest
                {
                    Sender = new Friend("John Doe", "DekelR", "+1234567890")
                    {
                        IsSelected = false,
                        ProfileImage = "friend_request.png"
                    },
                    RequestDate = DateTime.Now.AddDays(-1),
                    Status = StatusType.Pending
                },
                new FriendRequest
                {
                    Sender = new Friend("Jane Doe", "MaTaSaS", "+0987654321")
                    {
                        IsSelected = false,
                        ProfileImage = "friend_request.png"
                    },
                    RequestDate = DateTime.Now.AddDays(-2),
                    Status = StatusType.Pending
                }
            };

            AcceptFriendRequestCommand = new Command<FriendRequest>(async request => await AcceptRequest(request));
            RejectFriendRequestCommand = new Command<FriendRequest>(async request => await RejectRequest(request));
            ShowFriendDetailsCommand = new Command<FriendRequest>(onFriendClicked);
            BackCommand = new Command(onBackButtonClicked);
        }
        
        private void onFriendClicked(FriendRequest request)
        {
            App.Current.MainPage.DisplayAlert("Request Details", $"Username: {request.Sender.UserName}\nName: {request.Sender.Name}\nTelephone: {request.Sender.Telephone}\nRequest Date: {request.RequestDate}\nStatus: {request.Status}", "OK");
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
    }
}