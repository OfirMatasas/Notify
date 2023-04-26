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
            private string m_Name;
            private bool m_IsSelected;

            public string Name
            {
                get => m_Name;
                set
                {
                    if (m_Name != value)
                    {
                        m_Name = value;
                        OnPropertyChanged();
                    }
                }
            }

            public bool IsSelected
            {
                get => m_IsSelected;
                set
                {
                    if (m_IsSelected != value)
                    {
                        m_IsSelected = value;
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
    
        private string m_SelectedNotificationType;
        public string SelectedNotificationType
        {
            get => m_SelectedNotificationType;
            set
            {
                m_SelectedNotificationType = value;
                OnPropertyChanged(nameof(IsLocationTypeSelected));
                OnPropertyChanged(nameof(IsTimeTypeSelected));
            }
        }
    
        public bool IsLocationTypeSelected => SelectedNotificationType == "Location";
        public bool IsTimeTypeSelected => SelectedNotificationType == "Time";
    
        public List<string> LocationOptions { get; set; } = new List<string> { "Home", "Work" };
    
        private string m_SelectedLocationOption;
        public string SelectedLocationOption
        {
            get => m_SelectedLocationOption;
            set => m_SelectedLocationOption = value;
        }
        
        private string m_NotificationInfo;
        public string NotificationInfo
        {
            get => m_NotificationInfo;
            set => m_NotificationInfo = value;
        }
    
        private static TimeSpan m_SelectedTimeOption = DateTime.Today.TimeOfDay;
        public static TimeSpan SelectedTimeOption
        {
            get => m_SelectedTimeOption;
            set => m_SelectedTimeOption = value;
        }
        
        private static DateTime m_SelectedDateOption = DateTime.Today;
        public static DateTime SelectedDateOption
        {
            get => m_SelectedDateOption;
            set => m_SelectedDateOption = value;
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
            DateTime selectedDateTime = SelectedDateOption.Date.Add(SelectedTimeOption);
            bool created;

            if (checkIfSelectionsAreValid(out selectedFriends, out errorMessages))
            {
                if (IsTimeTypeSelected)
                {
                    created = AzureHttpClient.Instance.CreateTimeNotification(
                        NotificationName,
                        NotificationInfo,
                        SelectedNotificationType,
                        selectedDateTime,
                        selectedFriends);
                }
                else if (IsLocationTypeSelected)
                {
                    created = AzureHttpClient.Instance.CreateLocationNotification(
                        NotificationName,
                        NotificationInfo,
                        SelectedNotificationType,
                        SelectedLocationOption,
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

            if (string.IsNullOrEmpty(m_SelectedNotificationType))
            {
                errorMessages.Add("You must choose a notification type");
            }
            else if (IsLocationTypeSelected && string.IsNullOrEmpty(m_SelectedLocationOption))
            {
                errorMessages.Add("You must choose a location");
            }
            else if (IsTimeTypeSelected)
            {
                if (SelectedDateOption.Date.Add(SelectedTimeOption) < DateTime.Now)
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
