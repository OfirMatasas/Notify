using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Microsoft.IdentityModel.Tokens;
using Notify.Azure.HttpClient;
using Notify.Core;
using Notify.Helpers;
using Xamarin.Essentials;
using Xamarin.Forms;
using Newtonsoft.Json;

namespace Notify.ViewModels
{
    public class FriendDetailsPageViewModel : INotifyPropertyChanged
    {
        public string Name { get; set; }
        public string UserName { get; set; }
        public string Telephone { get; set; }
        public ImageSource ProfileImage { get; set; }
        public List<string> PermissionOptions { get; set; } = new List<string> { Constants.NOTIFICATION_PERMISSION_ALLOW, Constants.NOTIFICATION_PERMISSION_DISALLOW };
        public Command BackCommand { get; set; }
        public Command UpdateFriendPermissionsCommand { get; set; }
        public string DynamicNotificationsPermission { get; set; }
        public string LocationNotificationsPermission { get; set; }
        public string TimeNotificationsPermission { get; set; }
        
        public FriendDetailsPageViewModel(User selectedFriend)
        {
            BackCommand = new Command(onBackButtonClicked);
            UpdateFriendPermissionsCommand = new Command(onUpdateFriendPermissionsButtonClicked);
            Task.Run(() => setSelectedFriendDetails(selectedFriend));
        }

        private async void onUpdateFriendPermissionsButtonClicked()
        {
            bool isUpdated = await AzureHttpClient.Instance.UpdateFriendPermissionsAsync(UserName, LocationNotificationsPermission, TimeNotificationsPermission, DynamicNotificationsPermission);
            
            if(isUpdated)
            {
                await Shell.Current.DisplayAlert("Success", "Friend permissions updated successfully", "OK");
            }
            else
            {
                await Shell.Current.DisplayAlert("Error", "Failed to update friend permissions", "OK");
            }
        }

        private async void onBackButtonClicked()
        {
            await Shell.Current.Navigation.PopAsync();
        }

        private void setSelectedFriendDetails(User friend)
        {
            setSelectedFriendStaticDetails(friend);
            setSelectedFriendPermissionsDetails(friend.UserName);
        }

        private async void setSelectedFriendPermissionsDetails(string friendUsername)
        {
            Permission currentFriendPermission;
            List<Permission> allFriendsPermissions;
            string friendPermissionsJson = Preferences.Get(Constants.PREFERENCES_FRIENDS_PERMISSIONS, string.Empty);

            if (friendPermissionsJson.IsNullOrEmpty())
            {
                allFriendsPermissions = await AzureHttpClient.Instance.GetFriendsPermissions();
            }
            else 
            {
                allFriendsPermissions = JsonConvert.DeserializeObject<List<Permission>>(friendPermissionsJson);
            }

            currentFriendPermission = allFriendsPermissions.Find(permission => permission.FriendUsername.Equals(friendUsername));

            if(currentFriendPermission == null)
            {
                LocationNotificationsPermission = Constants.NOTIFICATION_PERMISSION_DISALLOW;
                TimeNotificationsPermission = Constants.NOTIFICATION_PERMISSION_DISALLOW;
                DynamicNotificationsPermission = Constants.NOTIFICATION_PERMISSION_DISALLOW;
            }
            else
            {
                LocationNotificationsPermission = currentFriendPermission.LocationNotificationPermission;
                TimeNotificationsPermission = currentFriendPermission.TimeNotificationPermission;
                DynamicNotificationsPermission = currentFriendPermission.DynamicNotificationPermission;
            }
        }

        private void setSelectedFriendStaticDetails(User friend)
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
