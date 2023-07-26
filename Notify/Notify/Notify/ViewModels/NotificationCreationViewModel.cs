using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows.Input;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using Notify.Azure.HttpClient;
using Notify.Core;
using Notify.Helpers;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace Notify.ViewModels
{
    public class NotificationCreationViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
    
        public Command BackCommand { get; set; }
        public string NotificationName { get; set; }

        public List<string> NotificationOptions { get; set; } = Constants.NOTIFICATION_OPTIONS_LIST;
    
        private string m_SelectedNotificationOption;
        public string SelectedNotificationOption
        {
            get => m_SelectedNotificationOption;
            set
            {
                m_SelectedNotificationOption = value;
                OnPropertyChanged(nameof(IsTimeOptionSelected));
                OnPropertyChanged(nameof(IsLocationOptionSelected));
                OnPropertyChanged(nameof(IsDynamicOptionSelected));
                OnPropertyChanged(nameof(ShowActivationOptions));
            }
        }
        
        public bool IsTimeOptionSelected => SelectedNotificationOption == Constants.TIME;
        public bool IsLocationOptionSelected => SelectedNotificationOption == Constants.LOCATION;
        public bool IsDynamicOptionSelected => SelectedNotificationOption == Constants.DYNAMIC;
        public bool ShowActivationOptions => IsLocationOptionSelected;
        
        public List<string> LocationOptions { get; set; } = Constants.LOCATIONS_LIST;
        public List<string> DynamicOptions { get; set; } = Constants.DYNAMIC_PLACE_LIST;
        public List<string> ActivationOptions { get; set; } = Constants.ACTIVATION_OPTIONS_LIST;
        
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

        private string m_SelectedActivationOption = string.Empty;
        public string SelectedActivationOption
        {
            get => m_SelectedActivationOption;
            set => m_SelectedActivationOption = value;
        }
        
        private static DateTime m_SelectedDateOption = DateTime.Today;
        public static DateTime SelectedDateOption
        {
            get => m_SelectedDateOption;
            set => m_SelectedDateOption = value;
        }
        
        private string m_SelectedLocationOption;
        public string SelectedLocationOption
        {
            get => m_SelectedLocationOption;
            set => m_SelectedLocationOption = value;
        }
        
        private string m_SelectedDynamicOption;
        public string SelectedDynamicOption
        {
            get => m_SelectedDynamicOption;
            set => m_SelectedDynamicOption = value;
        }

        private List<Friend> m_Friends;
        public List<Friend> Friends 
        { 
            get => m_Friends;
            set
            {
                m_Friends = value;
                OnPropertyChanged(nameof(Friends));
            }
        }

        public ICommand CreateNotificationCommand { get; set; }
    
        public NotificationCreationViewModel()
        {
            CreateNotificationCommand = new Command(OnCreateNotification);
            BackCommand = new Command(onBackClicked);
            
            RefreshFriendsList();
        }

        public async void RefreshFriendsList()
        {
            string friendsJson = Preferences.Get(Constants.PREFERENCES_FRIENDS, string.Empty);
            string myUsername = Preferences.Get(Constants.PREFERENCES_USERNAME, string.Empty);
            
            if (friendsJson.Equals(string.Empty))
            {
                Friends = await AzureHttpClient.Instance.GetFriends();
            }
            else
            {
                Friends = JsonConvert.DeserializeObject<List<Friend>>(friendsJson);
            }

            Friends.Add(new Friend(string.Empty, myUsername, string.Empty));
            Friends.Sort((friend1, friend2) => string.Compare(friend1.Name, friend2.Name, StringComparison.Ordinal));
        }

        private async void OnCreateNotification()
        {
            List<string> selectedRecipients, errorMessages;
            string completeErrorMessage;
            DateTime selectedDateTime = SelectedDateOption.Date.Add(SelectedTimeOption);
            bool isCreated;

            if (checkIfRecipientSelectionsIsValid(out selectedRecipients, out errorMessages))
            {
                if (IsTimeOptionSelected)
                {
                    isCreated = AzureHttpClient.Instance.CreateTimeNotification(
                        NotificationName,
                        NotificationDescription,
                        SelectedNotificationOption,
                        selectedDateTime,
                        selectedRecipients);
                }
                else if (IsLocationOptionSelected)
                {
                    isCreated = AzureHttpClient.Instance.CreateLocationNotification(
                        NotificationName,
                        NotificationDescription,
                        SelectedNotificationOption,
                        SelectedLocationOption,
                        SelectedActivationOption,
                        selectedRecipients);
                }
                else if (IsDynamicOptionSelected)
                {
                    isCreated = AzureHttpClient.Instance.CreateDynamicNotification(
                        NotificationName,
                        NotificationDescription,
                        SelectedNotificationOption,
                        SelectedDynamicOption,
                        selectedRecipients);
                }
                else
                {
                    await App.Current.MainPage.DisplayAlert("Invalid notification creation", "Something went wrong...", "OK");
                    isCreated = false;
                }

                if (isCreated)
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

        private bool checkIfRecipientSelectionsIsValid(out List<string> selectedRecipients, out List<string> errorMessages)
        {
            errorMessages = new List<string>();
            selectedRecipients = Friends
                .Where(recipient => recipient.IsSelected)
                .Select(recipient => recipient.UserName)
                .ToList();

            addErrorMessageAccordingToNotificationName(ref errorMessages);
            addErrorMessagesAccordingToNotificationType(ref errorMessages);
            addErrorMessagesAccordingToSelectedRecipients(selectedRecipients, ref errorMessages);

            return errorMessages.IsNullOrEmpty();
        }

        private static void addErrorMessagesAccordingToSelectedRecipients(List<string> selectedRecipients, ref List<string> errorMessages)
        {
            if (selectedRecipients == null || selectedRecipients.Count == 0)
            {
                errorMessages.Add("You must choose at least one recipient");
            }
        }

        private void addErrorMessagesAccordingToNotificationType(ref List<string> errorMessages)
        {
            if (IsTimeOptionSelected)
            {
                if (SelectedDateOption.Date.Add(SelectedTimeOption) < DateTime.Now)
                {
                    errorMessages.Add("You must choose a time in the future");
                }
            }
            else if (IsLocationOptionSelected)
            {
                if (m_SelectedLocationOption.IsNullOrEmpty())
                {
                    errorMessages.Add("You must choose a location");
                }
                if (SelectedActivationOption.IsNullOrEmpty())
                {
                    errorMessages.Add("You must choose an activation option");
                }
            }
            else if (IsDynamicOptionSelected)
            {
                if (m_SelectedDynamicOption.IsNullOrEmpty())
                {
                    errorMessages.Add("You must choose a type of place");
                }
            }
            else
            {
                errorMessages.Add("You must choose a notification type");
            }
        }

        private void addErrorMessageAccordingToNotificationName(ref List<string> errorMessages)
        {
            if (string.IsNullOrEmpty(NotificationName))
            {
                errorMessages.Add("You must name the notification");
            }
        }

        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        
        private async void onBackClicked()
        {
            await Shell.Current.GoToAsync(Constants.SHELL_NAVIGATION_NOTIFICATIONS);
        }
    }
}
