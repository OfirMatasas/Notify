using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Xamarin.Forms;

namespace Notify.ViewModels
{
    public class FriendsPageViewModel : INotifyPropertyChanged
    {
        public Command RefreshFriendsCommand { get; set; }
        public Command ShowFriendRequestsCommand { get; set; }

        public FriendsPageViewModel()
        {
            RefreshFriendsCommand = new Command(onRefreshFriendsClicked);
            ShowFriendRequestsCommand = new Command(onShowFriendRequestsClicked);
        }

        private void onShowFriendRequestsClicked()
        {
            App.Current.MainPage.DisplayAlert("Show Friends Clicked", "Show Friends Clicked", "OK");
        }

        private void onRefreshFriendsClicked()
        {
            App.Current.MainPage.DisplayAlert("Refresh Friends Clicked", "Refresh Friends Clicked", "OK");
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
