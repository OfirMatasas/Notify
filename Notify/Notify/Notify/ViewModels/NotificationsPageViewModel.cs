using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Notify.Azure.HttpClient;
using Notify.Core;
using Notify.Helpers;
using Notify.Views.Views;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace Notify.ViewModels
{
    public sealed class NotificationsPageViewModel : INotifyPropertyChanged
    {
        #region Constructor

        public NotificationsPageViewModel()
        {
            string notificationsJson;

            CreateNotificationCommand = new Command(onCreateNotificationClicked);
            RefreshNotificationsCommand = new Command(onNotificationsRefreshClicked);
            NotificationSelectedCommand = new Command(onNotificationSelected);

            try
            {
                notificationsJson = Preferences.Get(Constants.PREFERENCES_NOTIFICATIONS, string.Empty);
                if (!notificationsJson.Equals(string.Empty))
                {
                    Debug.WriteLine("Notifications found in preferences");
                    Notifications = JsonConvert.DeserializeObject<List<Notification>>(notificationsJson);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }

            onNotificationsRefreshClicked();
        }

        #endregion

        #region Create_Notification

        public Command CreateNotificationCommand { get; set; }

        private async void onCreateNotificationClicked()
        {
            await Shell.Current.GoToAsync("///create_notification");
        }

        #endregion

        #region Refresh_Notifications

        public Command RefreshNotificationsCommand { get; set; }

        private async void onNotificationsRefreshClicked()
        {
            await Task.Run(() => Notifications = AzureHttpClient.Instance.GetNotifications().Result);
            Preferences.Set(Constants.PREFERENCES_NOTIFICATIONS, JsonConvert.SerializeObject(Notifications));
        }

        #endregion

        #region Notifications_List

        public List<Notification> Notifications { get; set; }
        public Command NotificationSelectedCommand { get; set; }
        
        public Notification SelectedNotification { get; set; }
        
        private async void onNotificationSelected()
        {
            await Shell.Current.Navigation.PushAsync(new NotificationDetailsPage(SelectedNotification));
        }

        #endregion

        #region Interface_Methods
        
        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
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
        
        #endregion
    }
}
