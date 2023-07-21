using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Notify.Core;
using Notify.Helpers;
using Xamarin.Forms;

namespace Notify.ViewModels
{
    public class NotificationDetailsPageViewModel: INotifyPropertyChanged
    {
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
        }
        
        private async void onBackButtonClicked()
        {
            await Shell.Current.Navigation.PopAsync();
        }

        private void setSelectedNotificationDetails(Notification notification)
        {
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
