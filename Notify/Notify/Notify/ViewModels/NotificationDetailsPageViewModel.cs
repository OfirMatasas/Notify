using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Notify.Azure.HttpClient;
using Notify.Core;
using Notify.Helpers;
using Xamarin.Forms;

namespace Notify.ViewModels
{
    public class NotificationDetailsPageViewModel: INotifyPropertyChanged
    {
        private string ID { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Status { get; set; }
        public string Creator { get; set; }
        public string Target { get; set; }
        public string Type { get; set; }
        public object TypeInfo { get; set; }
        public string Activation { get; set; }
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

        public Command BackCommand { get; set; }
        
        public NotificationDetailsPageViewModel(Notification selectedNotification)
        {
            BackCommand = new Command(onBackButtonClicked);
            Task.Run(() => setSelectedNotificationDetails(selectedNotification));
            RenewNotificationCommand = new Command(onRenewNotificationButtonClicked);
            EditNotificationCommand = new Command(onEditNotificationButtonClicked);
            DeleteNotificationCommand = new Command(onDeleteNotificationButtonClicked);
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

        private void onEditNotificationButtonClicked()
        {
            throw new NotImplementedException();
        }

        private void onRenewNotificationButtonClicked()
        {
            throw new NotImplementedException();
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
            CreationDateTime = notification.CreationDateTime;
        }
        
        public Command DeleteNotificationCommand { get; set; }
        public Command EditNotificationCommand { get; set; }
        public Command RenewNotificationCommand { get; set; }

        public bool IsRenewable
        {
            get => Status == "Expired";
            set => OnPropertyChanged(nameof(IsRenewable));
        }
        
        public bool IsEditable
        {
            get => Status != "Expired";
            set => OnPropertyChanged(nameof(IsEditable));
        }
        
        public bool IsDeletable
        {
            get => Status != "Expired";
            set => OnPropertyChanged(nameof(IsDeletable));
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
