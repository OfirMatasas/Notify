using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading;
using Notify.Core;
using Notify.HttpClient;
using Xamarin.Forms;

namespace Notify.ViewModels
{
    public class NotificationsPageViewModel : INotifyPropertyChanged
    {
        public NotificationsPageViewModel()
        {
            CreateNotificationCommand = new Command(onCreateNotificationClicked);
            RefreshNotificationsCommand = new Command(onNotificationsRefreshClicked);
            
            Notifications = AzureHttpClient.Instance.GetNotifications().Result;

            //new Thread(pullNotificationsFromServer) { IsBackground = true }.Start();
        }

        #region Create_Notification

        public Command CreateNotificationCommand { get; set; }
        public Command RefreshNotificationsCommand { get; set; }

        private async void onCreateNotificationClicked()
        {
            await Shell.Current.GoToAsync("///create_notification");
        }
        
        private void onNotificationsRefreshClicked()
        {
            Notifications = AzureHttpClient.Instance.GetNotifications().Result;
        }

        #endregion

        #region Notifications_List

        public List<Notification> Notifications { get; set; }
        
        private void pullNotificationsFromServer()
        {
            Console.WriteLine("Daemon thread started...");
            
            //while (true)
            {
                //Thread.Sleep(5000);
                Notifications = AzureHttpClient.Instance.GetNotifications().Result;
            }
        }

        #endregion

        #region Interface_Methods

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
        
        protected bool SetProperty<T>(ref T backingStore, T value, [CallerMemberName] string propertyName = "", Action onChanged = null)
        {
            if (EqualityComparer<T>.Default.Equals(backingStore, value))
                return false;

            backingStore = value;
            onChanged?.Invoke();
            OnPropertyChanged(propertyName);

            return true;
        }
        
        #endregion
    }
}
