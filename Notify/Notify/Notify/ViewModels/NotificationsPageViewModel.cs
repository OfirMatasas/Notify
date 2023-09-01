using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using Newtonsoft.Json;
using Notify.Azure.HttpClient;
using Notify.Core;
using Notify.Helpers;
using Notify.Services;
using Notify.Views;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace Notify.ViewModels
{
    public sealed class NotificationsPageViewModel : INotifyPropertyChanged
    {

        private static NotificationsPageViewModel m_Instance;
        private static readonly object r_LockInstanceCreation = new object();

        public static NotificationsPageViewModel Instance 
        {
            get
            {
                if (m_Instance == null)
                {
                    lock (r_LockInstanceCreation)
                    {
                        if (m_Instance == null)
                        {
                            m_Instance = new NotificationsPageViewModel();
                        }
                    }
                }
                return m_Instance;
            }
        }
        
        private readonly LoggerService r_Logger = LoggerService.Instance;
        public List<Notification> Notifications { get; set; }
        public List<Notification> FilteredNotifications { get; set; }
        
        private bool m_IsActivationType;
        private string m_SearchTerm;
        private bool m_IsRefreshing;
        private Color m_Color;
        private string m_SelectedFilter;
        private bool m_IsLocationType;
        private string m_ExpandedNotificationId;
        
        public Command DeleteNotificationCommand { get; set; }
        public Command EditNotificationCommand { get; set; }
        public Command RenewNotificationCommand { get; set; }
        public Command CreateNotificationCommand { get; set; }
        public Command ExecuteSearchCommand { get; set; }
        public Command OpenMapCommand { get; set; } 
        
        public event PropertyChangedEventHandler PropertyChanged;
        
        public bool IsRefreshing { set => SetField(ref m_IsRefreshing, value); }
        
        public Color Color { get => m_Color; set => SetField(ref m_Color, value); }
        
        public bool IsActivationType
        {
            get => m_IsActivationType;
            set
            {
                m_IsActivationType = value;
                OnPropertyChanged(nameof(IsActivationType));
            }
        }
        
        public string SearchTerm
        {
            get => m_SearchTerm;
            set
            {
                SetField(ref m_SearchTerm, value);
                applyFilterAndSearch();
            }
        }
        
        public string SelectedFilter
        {
            get => m_SelectedFilter;
            set
            {
                SetField(ref m_SelectedFilter, value);
                applyFilterAndSearch();
            }
        }
        
        public List<string> FilterTypes { get; } = new List<string>
        {
            Constants.FILTER_TYPE_ALL,
            Constants.FILTER_TYPE_ACTIVE,
            Constants.FILTER_TYPE_PERMANENT,
            Constants.FILTER_TYPE_LOCATION,
            Constants.FILTER_TYPE_DYNAMIC_LOCATION,
            Constants.FILTER_TYPE_TIME,
            Constants.FILTER_TYPE_PENDING,
            Constants.FILTER_TYPE_DECLINED,
            Constants.FILTER_TYPE_EXPIRED
        };
        
        public bool IsLocationType
        {
            get => m_IsLocationType;
            set
            {
                m_IsLocationType = value;
                OnPropertyChanged(nameof(IsLocationType));
            }
        }
        
        public string ExpandedNotificationId
        {
            get => m_ExpandedNotificationId;
            set
            {
                if (m_ExpandedNotificationId != value)
                {
                    m_ExpandedNotificationId = value;
                    OnPropertyChanged(nameof(ExpandedNotificationId));
                }
            }
        }

        private NotificationsPageViewModel()
        {
            string notificationsJson;

            CreateNotificationCommand = new Command(onCreateNotificationClicked);
            RefreshNotificationsCommand = new Command(OnNotificationsRefreshClicked);
            DeleteNotificationCommand = new Command<Notification>(onDeleteNotificationButtonClicked);
            EditNotificationCommand = new Command<Notification>(onEditNotificationButtonClicked);
            RenewNotificationCommand = new Command<Notification>(onRenewNotificationButtonClicked);
            ExecuteSearchCommand = new Command(applyFilterAndSearch);
            OpenMapCommand = new Command<Notification>(onMapClicked);

            DeleteNotificationCommand = new Command<Notification>(onDeleteNotificationButtonClicked);
            EditNotificationCommand = new Command<Notification>(onEditNotificationButtonClicked);
            RenewNotificationCommand = new Command<Notification>(onRenewNotificationButtonClicked);

            try
            {
                FilteredNotifications = new List<Notification>();
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

            SelectedFilter = Constants.FILTER_TYPE_ACTIVE;
            OnNotificationsRefreshClicked();
            applyFilterAndSearch();
        }

        private void onMapClicked(Notification notification)
        {
            r_Logger.LogInformation($"onMapClicked with: {notification.Name}");

            ExternalMapsService.Instance.OpenExternalMap(notification.TypeInfo.ToString());
        }

        private void applyFilterAndSearch()
        {
            IEnumerable<Notification> filteredNotifications = ApplyFilter(Notifications);
    
            if (!string.IsNullOrWhiteSpace(SearchTerm))
            {
                filteredNotifications = filteredNotifications.Where(n => 
                    CultureInfo.CurrentCulture.CompareInfo.IndexOf(n.Name, SearchTerm, CompareOptions.IgnoreCase) >= 0
                );
            }
    
            FilteredNotifications = new List<Notification>(filteredNotifications);
            OnPropertyChanged(nameof(FilteredNotifications));
        }
        
        private IEnumerable<Notification> ApplyFilter(IEnumerable<Notification> notifications)
        {
            switch (SelectedFilter)
            {
                case "Permanent":
                    return notifications.Where(n => n.IsPermanent);

                case "Location":
                    return notifications.Where(n => n.Type.Equals(NotificationType.Location)); 

                case "Dynamic Location":
                    return notifications.Where(n => n.Type.Equals(NotificationType.Dynamic));

                case "Time":
                    return notifications.Where(n => n.Type.Equals(NotificationType.Time));

                case "Active":
                    return notifications.Where(n => n.Status.Equals("Active")); 

                case "Pending":
                    return notifications.Where(n => n.Status.Equals("Pending")); 

                case "Declined":
                    return notifications.Where(n => n.Status.Equals("Declined"));

                case "Expired":
                    return notifications.Where(n => n.Status.Equals("Expired"));

                default:
                    return notifications;
            }
        }
        
        private async void onCreateNotificationClicked()
        {
            await Shell.Current.Navigation.PushAsync(new NotificationCreationPage());
        }

        public Command RefreshNotificationsCommand { get; set; }

        public async void OnNotificationsRefreshClicked()
        {
            IsRefreshing = true;

            Notifications = await AzureHttpClient.Instance.GetNotifications();
            FilteredNotifications = new List<Notification>(Notifications);
            applyFilterAndSearch();

            IsRefreshing = false;
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
                    OnNotificationsRefreshClicked();
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
            if (notification.IsEditable)
            {
                Shell.Current.Navigation.PushAsync(new NotificationCreationPage(notification));
            }
            else
            {
                await App.Current.MainPage.DisplayAlert("Notification Edit",
                    $"Notification {notification.Name} is not editable",
                    "OK");
            }
        }
        
        public void ResetExpandedNotification()
        {
            ExpandedNotificationId = null;
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
                        OnNotificationsRefreshClicked();
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
        
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}

        

