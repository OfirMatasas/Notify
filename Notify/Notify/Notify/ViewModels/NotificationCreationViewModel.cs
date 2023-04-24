using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using Notify.HttpClient;
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

            public Friend(string name)
            {
                Name = name;
                IsSelected = false;
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
            get => selectedNotificationType;
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
    
        private static DateTime selectedTimeOption;
        public static DateTime SelectedTimeOption
        {
            get => selectedTimeOption;
            set => selectedTimeOption = value;
        }
        
        private static DateTime selectedDateOption;
        public static DateTime SelectedDateOption
        {
            get => selectedDateOption;
            set => selectedDateOption = value;
        }

        public List<Friend> Friends { get; set; } = new List<Friend> 
        {  
            new Friend("Ofir"),
            new Friend("Dekel"),
            new Friend("Lin")
        };

        public ICommand CreateNotificationCommand { get; set; }
    
        public NotificationCreationViewModel()
        {
            CreateNotificationCommand = new Command(OnCreateNotification);
        }

        private async void OnCreateNotification()
        {
            List<string> selectedFriends, errorMessages;
            string completeErrorMessage;
            DateTime selectedDateTime = selectedDateOption.Date.Add(selectedTimeOption.TimeOfDay);
            bool created;

            if (checkIfSelectionsAreValid(out selectedFriends, out errorMessages))
            {
                if (IsTimeTypeSelected)
                {
                    created = AzureHttpClient.Instance.CreateTimeNotification(
                        NotificationName,
                        selectedNotificationType,
                        selectedDateTime,
                        selectedFriends);
                }
                else if (IsLocationTypeSelected)
                {
                    created = AzureHttpClient.Instance.CreateLocationNotification(
                        NotificationName,
                        selectedNotificationType,
                        selectedLocationOption,
                        selectedFriends);
                }
                else
                {
                    await App.Current.MainPage.DisplayAlert("Invalid notification creation", "Something went wrong...", "OK");
                    created = false;
                }

                if (created)
                {
                    await App.Current.MainPage.DisplayAlert("Notification created", $"Notification {NotificationName} created successfully!", "OK");
                }
            }
            else
            {
                completeErrorMessage = string.Join(Environment.NewLine, errorMessages.Select(errorMessage => $"- {errorMessage}"));
                await App.Current.MainPage.DisplayAlert("Invalid notification", $"{completeErrorMessage}", "OK");
            }
        }

        private bool checkIfSelectionsAreValid(out List<string> selectedFriends, out List<string> errorMessages)
        {
            errorMessages = new List<string>();
            selectedFriends = Friends
                .Where(friends => friends.IsSelected)
                .Select(friend => friend.Name)
                .ToList();

            if (string.IsNullOrEmpty(NotificationName))
            {
                errorMessages.Add("You must name the notification");
            }

            if (string.IsNullOrEmpty(selectedNotificationType))
            {
                errorMessages.Add("You must choose a notification type");
            }
            else if (IsLocationTypeSelected && string.IsNullOrEmpty(selectedLocationOption))
            {
                errorMessages.Add("You must choose a location");
            }
            else if (IsTimeTypeSelected)
            {
                if (selectedDateOption.Date.Add(selectedTimeOption.TimeOfDay) < DateTime.Now)
                {
                    errorMessages.Add("You must choose a time in the future");
                }
            }
            
            if (selectedFriends == null || selectedFriends.Count == 0)
            {
                errorMessages.Add("You must choose at least one friend");
            }

            return errorMessages.Count == 0;
        }

        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
