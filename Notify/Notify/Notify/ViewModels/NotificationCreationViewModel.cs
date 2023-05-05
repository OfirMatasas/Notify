using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using Notify.Azure.HttpClient;
using Notify.Core;
using Xamarin.Forms;

namespace Notify.ViewModels
{
    public class NotificationCreationViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
    
        public Command BackCommand { get; set; }
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
        
        private string m_NotificationDescription;
        public string NotificationDescription
        {
            get => m_NotificationDescription;
            set => m_NotificationDescription = value;
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
            new Friend("Ofir", "MaTaSaS", "+972542001717"),
            new Friend("Dekel", "DekelR", "+972525459229"),
            new Friend("Lin", "Linkimos", "+972586555549")
        };

        public ICommand CreateNotificationCommand { get; set; }
    
        public NotificationCreationViewModel()
        {
            CreateNotificationCommand = new Command(OnCreateNotification);
            BackCommand = new Command(onBackClicked);
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
                        NotificationDescription,
                        SelectedNotificationType,
                        selectedDateTime,
                        selectedFriends);
                }
                else if (IsLocationTypeSelected)
                {
                    created = AzureHttpClient.Instance.CreateLocationNotification(
                        NotificationName,
                        NotificationDescription,
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
        
        private async void onBackClicked()
        {
            await Shell.Current.GoToAsync("///teams");
        }
    }
}
