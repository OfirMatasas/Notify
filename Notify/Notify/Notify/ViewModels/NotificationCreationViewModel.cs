using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using Xamarin.Forms;

namespace Notify.ViewModels
{
    public class NotificationCreationViewModel : INotifyPropertyChanged
    {
        public sealed class Friend : INotifyPropertyChanged
        {
            private string name;
            private bool isSelected;

            public string Name
            {
                get => name;
                set
                {
                    if (name != value)
                    {
                        name = value;
                        OnPropertyChanged();
                    }
                }
            }

            public bool IsSelected
            {
                get => isSelected;
                set
                {
                    if (isSelected != value)
                    {
                        isSelected = value;
                        OnPropertyChanged();
                    }
                }
            }

            public Friend(string name, bool isSelected)
            {
                Name = name;
                IsSelected = isSelected;
            }

            public event PropertyChangedEventHandler PropertyChanged;

            private void OnPropertyChanged([CallerMemberName] string propertyName = null)
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
    
        public string NotificationName { get; set; }
    
        public List<string> NotificationTypes { get; set; } = new List<string> { "Location", "Time" };
    
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
            get => selectedLocationOption;
            set => selectedLocationOption = value;
        }
    
        private static TimeSpan selectedTimeOption = new TimeSpan();
        public static TimeSpan SelectedTimeOption
        {
            get { return selectedTimeOption; }
            set { selectedTimeOption = value; }
        }

        public List<Friend> Friends { get; set; } = new List<Friend> 
        {  
            new Friend("Friend1", true),
            new Friend("Friend2", true)
        };

        public ICommand CreateNotificationCommand { get; set; }
    
        public NotificationCreationViewModel()
        {
            CreateNotificationCommand = new Command(OnCreateNotification);
        }
    
        private async void OnCreateNotification()
        {
            await App.Current.MainPage.DisplayAlert("Testing", "Empty credentials", "OK");
        }

        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}