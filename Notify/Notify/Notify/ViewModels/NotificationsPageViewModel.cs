using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Notify.Core;
using Notify.HttpClient;
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
            CreateNotificationCommand = new Command(onCreateNotificationClicked);
            RefreshNotificationsCommand = new Command(onNotificationsRefreshClicked);
            NotificationSelectedCommand = new Command(onNotificationSelected);

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
        }

        #endregion

        #region Notifications_List

        public List<Notification> Notifications { get; set; } = new List<Notification>();
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
