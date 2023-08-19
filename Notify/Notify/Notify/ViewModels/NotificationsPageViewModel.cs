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
using Notify.Services;
using Notify.Views;
using Notify.Views.Views;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace Notify.ViewModels
{
    public sealed class NotificationsPageViewModel : INotifyPropertyChanged
    {
        private readonly LoggerService r_Logger = LoggerService.Instance;
        public bool IsRefreshing { set => SetField(ref m_IsRefreshing, value); }
        public Color Color { get => m_Color; set => SetField(ref m_Color, value); }
        public List<Notification> Notifications { get; set; }
        public Notification SelectedNotification { get; set; }
        
        public Command BackCommand { get; set; }
        public Command DeleteNotificationCommand { get; set; }
        public Command EditNotificationCommand { get; set; }
        public Command RenewNotificationCommand { get; set; }
        public Command NotificationSelectedCommand { get; set; }
        
        public event PropertyChangedEventHandler PropertyChanged;
        
        private bool m_IsRefreshing;
        private Color m_Color;
        
        private bool m_IsActivationType;
        public bool IsActivationType
        {
            get => m_IsActivationType;
            set
            {
                m_IsActivationType = value;
                OnPropertyChanged(nameof(IsActivationType));
            }
        }
        
        private bool m_IsLocationType;
        public bool IsLocationType
        {
            get => m_IsLocationType;
            set
            {
                m_IsLocationType = value;
                OnPropertyChanged(nameof(IsLocationType));
            }
        }
        
        public NotificationsPageViewModel()
        {
            string notificationsJson;

            CreateNotificationCommand = new Command(onCreateNotificationClicked);
            RefreshNotificationsCommand = new Command(onNotificationsRefreshClicked);
            NotificationSelectedCommand = new Command(onNotificationSelected);

            DeleteNotificationCommand = new Command<Notification>(onDeleteNotificationButtonClicked);
            EditNotificationCommand = new Command<Notification>(onEditNotificationButtonClicked);
            RenewNotificationCommand = new Command<Notification>(onRenewNotificationButtonClicked);

            try
            {
                notificationsJson = Preferences.Get(Constants.PREFERENCES_NOTIFICATIONS, string.Empty);
                if (!notificationsJson.Equals(string.Empty))
                {
                    r_Logger.LogDebug("Notifications found in preferences");
                    Notifications = JsonConvert.DeserializeObject<List<Notification>>(notificationsJson);
                }
            }
            catch (Exception ex)
            {
                r_Logger.LogError(ex.Message);
            }

            onNotificationsRefreshClicked();
        }
        
        public Command CreateNotificationCommand { get; set; }

        private async void onCreateNotificationClicked()
        {
            await Shell.Current.Navigation.PushAsync(new NotificationCreationPage());
        }

        public Command RefreshNotificationsCommand { get; set; }

        private async void onNotificationsRefreshClicked()
        {
            IsRefreshing = true;

            await Task.Run(() => Notifications = AzureHttpClient.Instance.GetNotifications().Result);
            Preferences.Set(Constants.PREFERENCES_NOTIFICATIONS, JsonConvert.SerializeObject(Notifications));

            IsRefreshing = false;
        }
        
        private async void onNotificationSelected()
        {
            await Shell.Current.Navigation.PushAsync(new NotificationDetailsPage(SelectedNotification));
        }
        
        private void SetField<T>(ref T field, T value, [CallerMemberName] string propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(field, value)) return;
            field = value;
            OnPropertyChanged(propertyName);
        }
        
        private async void onDeleteNotificationButtonClicked(Notification notification)
        {
            string messageTitle = "Notification Deletion";
            bool isDeleted;
            bool isConfirmed = await App.Current.MainPage.DisplayAlert(messageTitle,
                "Are you sure you want to delete this notification?",
                "Yes", "No");

            if (isConfirmed)
            {
                isDeleted = await AzureHttpClient.Instance.DeleteNotificationAsync(notification.ID);

                if (isDeleted)
                {
                    onNotificationsRefreshClicked();
                }
                else
                {
                    await App.Current.MainPage.DisplayAlert(messageTitle,
                        "Failed to delete notification",
                        "OK");
                }
            }
        }

        private async void onEditNotificationButtonClicked(Notification notification)
        {
            Debug.WriteLine("edit notification button clicked");
            Shell.Current.Navigation.PushAsync(new NotificationCreationPage(SelectedNotification));
        }
        
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private async void onRenewNotificationButtonClicked(Notification notification)
        {
            string messageTitle = "Notification Renewal";
            string messageBody, username;
            bool isRenewed, isConfirmed;

            if (notification.IsRenewable)
            {
                isConfirmed = await App.Current.MainPage.DisplayAlert(messageTitle,
                    "Are you sure you want to renew this notification?",
                    "Yes", "No");

                if (isConfirmed)
                {
                    username = Preferences.Get(Constants.PREFERENCES_USERNAME, string.Empty);
                    isRenewed = await AzureHttpClient.Instance.RenewNotificationAsync(username, notification.ID);

                    if (isRenewed)
                    {
                        messageBody = $"Notification {notification.Name} renewed successfully";
                        onNotificationsRefreshClicked();
                    }
                    else
                    {
                        messageBody = $"Failed to renew notification {notification.Name}";
                    }

                    await App.Current.MainPage.DisplayAlert(messageTitle, messageBody, "OK");
                }
            }
            else
            {
                messageBody = $"Notification {notification.Name} is not renewable";
                await App.Current.MainPage.DisplayAlert(messageTitle, messageBody, "OK");
            }
        }

        private async void onBackButtonClicked()
        {
            await Shell.Current.Navigation.PopAsync();
        }
    }
}

        

