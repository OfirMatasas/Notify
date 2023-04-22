using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Input;
using Xamarin.Forms;

namespace Notify.ViewModels
{
    public class NotificationCreationViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
    
        public string NotificationName { get; set; }
    
        public List<string> NotificationTypes { get; set; } = new List<string> { "location", "time" };
    
        private string selectedNotificationType;
        public string SelectedNotificationType
        {
            get { return selectedNotificationType; }
            set
            {
                selectedNotificationType = value;
                OnPropertyChanged(nameof(IsLocationTypeSelected));
                OnPropertyChanged(nameof(IsTimeTypeSelected));
            }
        }
    
        public bool IsLocationTypeSelected => SelectedNotificationType == "Location";
        public bool IsTimeTypeSelected => SelectedNotificationType == "Time";
    
        public List<string> LocationOptions { get; set; } = new List<string> { "Home", "Work" };
    
        private string selectedLocationOption;
        public string SelectedLocationOption
        {
            get { return selectedLocationOption; }
            set { selectedLocationOption = value; }
        }
    
        private static TimeSpan selectedTimeOption = new TimeSpan();
        public static TimeSpan SelectedTimeOption
        {
            get { return selectedTimeOption; }
            set { selectedTimeOption = value; }
        }
    
        public List<string> Friends { get; set; } = new List<string> { "Friend 1", "Friend 2", "Friend 3" };
    
        public ICommand CreateNotificationCommand { get; set; }
    
        public NotificationCreationViewModel()
        {
            CreateNotificationCommand = new Command(OnCreateNotification);
        }
    
        private void OnCreateNotification()
        {
            // Implement the logic to create the notification with the selected values.
        }
    
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}