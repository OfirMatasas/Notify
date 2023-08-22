using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.SymbolStore;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Notify.Azure.HttpClient;
using Notify.Core;
using Notify.Helpers;
using Notify.Views;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace Notify.ViewModels
{
    public class NotificationDetailsPageViewModel: INotifyPropertyChanged
    {
        private Notification SelectedNotification { get; set; }
        private string ID { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Status { get; set; }
        public string Creator { get; set; }
        public string Target { get; set; }
        public string Type { get; set; }
        public object TypeInfo { get; set; }
        public string Activation { get; set; }
        public bool IsPermanent { get; set; }
        public DateTime CreationDateTime { get; set; }

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

        public Command BackCommand { get; set; }
        
        public NotificationDetailsPageViewModel(Notification selectedNotification)
        {
            BackCommand = new Command(onBackButtonClicked);
            Task.Run(() => setSelectedNotificationDetails(selectedNotification));
            RenewNotificationCommand = new Command(onRenewNotificationButtonClicked);
            EditNotificationCommand = new Command(onEditNotificationButtonClicked);
            DeleteNotificationCommand = new Command(onDeleteNotificationButtonClicked);
            AcceptNotificationCommand = new Command(onAcceptNotificationButtonClicked);
            DeclineNotificationCommand = new Command(onDeclineNotificationButtonClicked);
            SelectedNotification = selectedNotification;
        }

        private async void onDeleteNotificationButtonClicked()
        {
            bool isDeleted;
            bool isConfirmed = await App.Current.MainPage.DisplayAlert("Notification Deletion", 
                "Are you sure you want to delete this notification?", 
                "Yes", "No");
            
            if (isConfirmed)
            {
                isDeleted = await AzureHttpClient.Instance.DeleteNotificationAsync(ID);

                if (isDeleted)
                {
                    await Shell.Current.Navigation.PopAsync();
                }
                else
                {
                    await App.Current.MainPage.DisplayAlert("Notification Deletion", 
                        "Failed to delete notification", 
                        "OK");
                }
            }
        }

        private async void onEditNotificationButtonClicked()
        {
            Shell.Current.Navigation.PushAsync(new NotificationCreationPage(SelectedNotification));
        }

        private async void onRenewNotificationButtonClicked()
        {
            string username, messageBody;
            bool isRenewed;
            bool isConfirmed = await App.Current.MainPage.DisplayAlert("Notification Renewal", 
                "Are you sure you want to renew this notification?", 
                "Yes", "No");

            if (isConfirmed)
            {
                username = Preferences.Get(Constants.PREFERENCES_USERNAME, string.Empty);
                isRenewed = await AzureHttpClient.Instance.RenewNotificationAsync(username , ID);
                
                if (isRenewed)
                {
                    messageBody = $"Notification {Name} renewed successfully";
                }
                else
                {
                    messageBody = $"Failed to renew notification {Name}";
                }
                
                await App.Current.MainPage.DisplayAlert("Notification Renewal", 
                    messageBody, 
                    "OK");
            }
        }
        
        private async void onDeclineNotificationButtonClicked()
        {
            bool isDeleted = await AzureHttpClient.Instance.DeleteNotificationAsync(ID);
            
            if (isDeleted)
            {
                await Shell.Current.Navigation.PopAsync();
            }
            else
            {
                await App.Current.MainPage.DisplayAlert("Notification Deletion", 
                    "Failed to delete notification", 
                    "OK");
            }
        }

        private void onAcceptNotificationButtonClicked()
        {
            bool isAccepted = AzureHttpClient.Instance.UpdateNotificationsStatus(new List<Notification> { SelectedNotification }, Constants.NOTIFICATION_STATUS_ACTIVE);
            
            if (isAccepted)
            {
                App.Current.MainPage.DisplayAlert("Notification Acceptance", 
                    "Notification accepted successfully", 
                    "OK");
                
                Status = Constants.NOTIFICATION_STATUS_ACTIVE;
            }
            else
            {
                App.Current.MainPage.DisplayAlert("Notification Acceptance", 
                    "Failed to accept notification", 
                    "OK");
            }
        }

        private async void onBackButtonClicked()
        {
            await Shell.Current.Navigation.PopAsync();
        }

        private void setSelectedNotificationDetails(Notification notification)
        {
            ID = notification.ID;
            Name = notification.Name;
            Description = notification.Description;
            Status = notification.Status;
            Target = notification.Target;
            Creator = notification.Creator;
            Type = Enum.GetName(typeof(NotificationType), notification.Type);
            TypeInfo = notification.TypeInfo;
            Activation = notification.Activation;
            IsActivationType = Activation != string.Empty;
            IsPermanent = notification.IsPermanent;
            IsLocationType = notification.Type == NotificationType.Location;
            CreationDateTime = notification.CreationDateTime;
        }
        
        public Command DeleteNotificationCommand { get; set; }
        public Command EditNotificationCommand { get; set; }
        public Command RenewNotificationCommand { get; set; }
        public Command AcceptNotificationCommand { get; set; }
        public Command DeclineNotificationCommand { get; set; }

        public bool IsRenewable => Status == "Expired" && Type != Constants.TIME;
        public bool IsEditable => Status != "Expired";
        public bool IsDeletable => Status != "Expired";
        public bool IsPending => Status == "Pending";

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
