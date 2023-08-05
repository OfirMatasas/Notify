using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Notify.Core;
using Xamarin.Forms;

namespace Notify.ViewModels
{
    public class FriendDetailsPageViewModel : INotifyPropertyChanged
    {
        public string Name { get; set; }
        public string UserName { get; set; }
        public string Telephone { get; set; }
        public ImageSource ProfileImage { get; set; }

        public Command BackCommand { get; set; }
        
        public FriendDetailsPageViewModel(User selectedFriend)
        {
            BackCommand = new Command(onBackButtonClicked);
            Task.Run(() => setSelectedFriendDetails(selectedFriend));
        }
        
        private async void onBackButtonClicked()
        {
            await Shell.Current.Navigation.PopAsync();
        }

        private void setSelectedFriendDetails(User friend)
        {
            Name = friend.Name;
            UserName = friend.UserName;
            Telephone = friend.Telephone;
            ProfileImage = friend.ProfilePicture;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private bool SetField<T>(ref T field, T value, [CallerMemberName] string propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(field, value)) return false;
            field = value;
            OnPropertyChanged(propertyName);
            return true;
        }
    }
}
