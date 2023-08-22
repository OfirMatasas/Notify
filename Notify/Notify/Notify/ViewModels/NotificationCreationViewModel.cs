using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
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
        
        public string PageTitle => IsCreating ? "Create Notification" : "Edit Notification";
        public Command BackCommand { get; set; }

        private string NotificationID { get; set; }
        private string m_NotificationName;
        public string NotificationName
        {
            get => m_NotificationName;
            set
            {
                m_NotificationName = value;
                OnPropertyChanged(nameof(NotificationName));
            }
        }

        public List<string> NotificationTypes { get; set; } = Constants.NOTIFICATION_OPTIONS_LIST;
    
        private string m_SelectedNotificationType;
        public string SelectedNotificationType
        {
            get => m_SelectedNotificationType;
            set
            {
                m_SelectedNotificationType = value;
                OnPropertyChanged(nameof(IsTimeTypeSelected));
                OnPropertyChanged(nameof(IsLocationTypeSelected));
                OnPropertyChanged(nameof(IsDynamicTypeSelected));
                OnPropertyChanged(nameof(ShowActivationOptions));
            }
        }
        
        public bool IsTimeTypeSelected => SelectedNotificationType == Constants.TIME;
        public bool IsLocationTypeSelected => SelectedNotificationType == Constants.LOCATION;
        public bool IsDynamicTypeSelected => SelectedNotificationType == Constants.DYNAMIC;
        public bool ShowActivationOptions => IsLocationTypeSelected;
        
        public List<string> LocationOptions { get; set; } = Constants.LOCATIONS_LIST;
        public List<string> DynamicOptions { get; set; } = Constants.DYNAMIC_PLACE_LIST;
        public List<string> ActivationOptions { get; set; } = Constants.ACTIVATION_OPTIONS_LIST;
        
        private bool m_IsEditing;
        public bool IsEditing
        {
            get => m_IsEditing;
            set
            {
                m_IsEditing = value;
                OnPropertyChanged(nameof(IsEditing));
            }
        }
        
        private bool m_IsCreating;
        public bool IsCreating
        {
            get => m_IsCreating;
            set
            {
                m_IsCreating = value;
                OnPropertyChanged(nameof(m_IsCreating));
            }
        }
        
        private string m_NotificationDescription;
        public string NotificationDescription
        {
            get => m_NotificationDescription;
            set
            {
                m_NotificationDescription = value;
                OnPropertyChanged(nameof(NotificationDescription));
            }
        }

        public static TimeSpan SelectedTime { get; set; } = DateTime.Today.TimeOfDay;

        private string m_SelectedActivationOption = string.Empty;
        public string SelectedActivationOption
        {
            get => m_SelectedActivationOption;
            set
            {
                m_SelectedActivationOption = value;
                OnPropertyChanged(nameof(SelectedActivationOption));
            }
        }

        public static DateTime SelectedDate { get; set; } = DateTime.Today;

        private string m_SelectedLocationOption;
        public string SelectedLocationOption
        {
            get => m_SelectedLocationOption;
            set
            {
                m_SelectedLocationOption = value;
                OnPropertyChanged(nameof(SelectedLocationOption));
            }
        }
        
        private string m_SelectedDynamicOption;
        public string SelectedDynamicOption
        {
            get => m_SelectedDynamicOption;
            set
            {
                m_SelectedDynamicOption = value;
                OnPropertyChanged(nameof(SelectedDynamicOption));
            }
        }
        
        public bool m_IsPermanent;
        public bool IsPermanent
        {
            get => m_IsPermanent;
            set
            {
                m_IsPermanent = value;
                OnPropertyChanged(nameof(IsPermanent));
            }
        }

        private List<User> m_Friends;
        public List<User> Friends 
        { 
            get => m_Friends;
            set
            {
                m_Friends = value;
                OnPropertyChanged(nameof(Friends));
            }
        }

        public Command CreateNotificationCommand { get; set; }
        public Command UpdateNotificationCommand { get; set; }
    
        public NotificationCreationViewModel(Notification notificationToEdit = null)
        {
            CreateNotificationCommand = new Command(OnCreateNotification);
            UpdateNotificationCommand = new Command(OnUpdateNotification);
            BackCommand = new Command(onBackClicked);
            IsCreating = notificationToEdit is null;
            IsEditing = !IsCreating;
            
            if (IsEditing)
            {
                NotificationID = notificationToEdit.ID;
                populateNotificationFields(notificationToEdit);
            }

            RefreshFriendsList();
        }

        private async void OnUpdateNotification()
        {
            bool isUpdated;
            
            if(NotificationName.IsNullOrEmpty() || NotificationDescription.IsNullOrEmpty())
            {
                App.Current.MainPage.DisplayAlert("Error", "Please fill all fields", "OK");
            }
            else if(IsTimeTypeSelected && SelectedDate.Date.Add(SelectedTime) < DateTime.Now)
            {
                App.Current.MainPage.DisplayAlert("Error", "You must choose a time in the future", "OK");
            }
            else
            {
                if (IsTimeTypeSelected)
                {
                    isUpdated = await AzureHttpClient.Instance.UpdateTimeNotificationAsync(
                        NotificationID,
                        NotificationName,
                        NotificationDescription,
                        SelectedNotificationType,
                        SelectedDate.Date.Add(SelectedTime));
                }
                else if (IsLocationTypeSelected)
                {
                    isUpdated = await AzureHttpClient.Instance.UpdateLocationNotificationAsync(
                        NotificationID,
                        NotificationName,
                        NotificationDescription,
                        SelectedNotificationType,
                        SelectedLocationOption,
                        SelectedActivationOption,
                        IsPermanent);
                }
                else if (IsDynamicTypeSelected)
                {
                    isUpdated = await AzureHttpClient.Instance.UpdateDynamicNotificationAsync(
                        NotificationID,
                        NotificationName,
                        NotificationDescription,
                        SelectedNotificationType,
                        SelectedDynamicOption);
                }
                else
                {
                    await App.Current.MainPage.DisplayAlert("Invalid notification update", "Something went wrong...", "OK");
                    isUpdated = false;
                }

                if (isUpdated)
                {
                    await App.Current.MainPage.DisplayAlert("Notification updated", $"Notification {NotificationName} updated updated!", "OK");
                    await Shell.Current.Navigation.PopAsync();
                }
            }
        }

        private void populateNotificationFields(Notification notificationToEdit)
        {
            NotificationName = notificationToEdit.Name;
            SelectedNotificationType = notificationToEdit.Type.ToString();
            NotificationDescription = notificationToEdit.Description;

            if (notificationToEdit.Type.Equals(NotificationType.Time))
            {
                SelectedDate = notificationToEdit.TypeInfo as DateTime? ?? DateTime.Today;
                SelectedTime = notificationToEdit.TypeInfo as TimeSpan? ?? DateTime.Today.TimeOfDay;
            }
            else if (notificationToEdit.Type.Equals(NotificationType.Location))
            {
                SelectedActivationOption = notificationToEdit.Activation;
                SelectedLocationOption = notificationToEdit.TypeInfo as string;
                IsPermanent = notificationToEdit.IsPermanent;
            }
            else if (notificationToEdit.Type.Equals(NotificationType.Dynamic))
            {
                SelectedDynamicOption = notificationToEdit.TypeInfo as string;
            }
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
                Friends = JsonConvert.DeserializeObject<List<User>>(friendsJson);
            }

            Friends.Add(new User(string.Empty, myUsername, string.Empty));
            Friends.Sort((friend1, friend2) => string.Compare(friend1.Name, friend2.Name, StringComparison.Ordinal));
        }

        private async void OnCreateNotification()
        {
            List<string> selectedRecipients, errorMessages;
            string completeErrorMessage;
            DateTime selectedDateTime = SelectedDate.Date.Add(SelectedTime);
            bool isCreated;

            if (checkIfRecipientSelectionsIsValid(out selectedRecipients, out errorMessages))
            {
                if (IsTimeTypeSelected)
                {
                    isCreated = AzureHttpClient.Instance.CreateTimeNotification(
                        NotificationName,
                        NotificationDescription,
                        SelectedNotificationType,
                        selectedDateTime,
                        selectedRecipients);
                }
                else if (IsLocationTypeSelected)
                {
                    isCreated = AzureHttpClient.Instance.CreateLocationNotification(
                        NotificationName,
                        NotificationDescription,
                        SelectedNotificationType,
                        SelectedLocationOption,
                        SelectedActivationOption,
                        selectedRecipients,
                        IsPermanent);
                }
                else if (IsDynamicTypeSelected)
                {
                    isCreated = AzureHttpClient.Instance.CreateDynamicNotification(
                        NotificationName,
                        NotificationDescription,
                        SelectedNotificationType,
                        SelectedDynamicOption,
                        selectedRecipients);
                }
                else
                {
                    await App.Current.MainPage.DisplayAlert("Invalid notification creation", "Invalid notification type", "OK");
                    isCreated = false;
                }

                if (isCreated)
                {
                    await App.Current.MainPage.DisplayAlert("Notification created", $"Notification {NotificationName} created successfully!", "OK");
                    Shell.Current.Navigation.PopAsync();
                }
                else
                {
                    await App.Current.MainPage.DisplayAlert("Invalid notification creation", "Something went wrong...", "OK");
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
            if (IsTimeTypeSelected)
            {
                if (SelectedDate.Date.Add(SelectedTime) < DateTime.Now)
                {
                    errorMessages.Add("You must choose a time in the future");
                }
            }
            else if (IsLocationTypeSelected)
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
            else if (IsDynamicTypeSelected)
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
            await Shell.Current.Navigation.PopAsync();
        }
    }
}
