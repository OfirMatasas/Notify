using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Notify.Azure.HttpClient;
using Notify.Core;
using Notify.Services;
using Notify.Views;
using Xamarin.Essentials;
using Xamarin.Forms;
using Constants = Notify.Helpers.Constants;


namespace Notify.ViewModels
{
    public class SettingsPageViewModel :INotifyPropertyChanged
    {
        private readonly LoggerService r_Logger = LoggerService.Instance;
        private string m_UserName;
        private User m_CurrentUser;
        
        public event PropertyChangedEventHandler PropertyChanged;

        public string UserName 
        { 
            get => m_UserName;
            set
            {
                m_UserName = value;
                OnPropertyChanged(nameof(UserName));
            }
        }
        
        public SettingsPageViewModel()
        {
            setUser();
            
            LoadProfilePictureCommand = new Command(onLoadProfilePicture);
            GoLocationSettingsPageCommand = new Command(onLocationSettingsButtonClicked);
            GoNotificationSettingsPageCommand = new Command(onNotificationSettingsButtonClicked);
            GoWifiSettingsPageCommand = new Command(onWifiSettingsButtonClicked);
            GoBluetoothSettingsPageCommand = new Command(onBluetoothSettingsButtonClicked);
            GoDestinationsSettingsPageCommand = new Command(onDestinationsSettingsButtonClicked);
            DarkModeToggleCommand = new Command(DarkModeToggleCommandHandler);

            Init = Initialize();
        }
        
        #region Profile_Picture
        
        private ImageSource m_ProfilePicture;
        public Command LoadProfilePictureCommand { get; set; }

        public ImageSource ProfilePicture
        {
            get => m_ProfilePicture;
            set
            {
                if (m_ProfilePicture != value)
                {
                    m_ProfilePicture = value;
                    OnPropertyChanged(nameof(ProfilePicture));
                }
            }
        }
        
        private void setProfilePicture()
        {
            ProfilePicture = ImageSource.FromUri(new Uri(m_CurrentUser.ProfilePicture));
            Preferences.Set(Constants.PREFERENCES_USER_OBJECT, JsonConvert.SerializeObject(m_CurrentUser));
        }
        
         private async void onLoadProfilePicture()
        {
            string action = await App.Current.MainPage.DisplayActionSheet("Profile Picture", "Cancel", null, "Upload new picture", "Clear picture");
            
            switch (action)
            {
                case "Upload new picture":
                    uploadNewProfilePicture();
                    break;
                case "Clear picture":
                    await clearProfilePicture();
                    break;
            }
        }

        private async void uploadNewProfilePicture()
        {
            Stream stream;
            FileResult fileResult;
            string imageUrl, base64String;
            MemoryStream memoryStream;
            byte[] imageBytes;
                
            try
            {
                fileResult = await FilePicker.PickAsync(new PickOptions
                {
                    FileTypes = FilePickerFileType.Images,
                    PickerTitle = "Pick an image"
                });
                
                if (fileResult == null)
                {
                    r_Logger.LogInformation("No file was picked.");
                    return;
                }
                
                stream = await fileResult.OpenReadAsync();
                
                using (memoryStream = new MemoryStream())
                {
                    await stream.CopyToAsync(memoryStream);
                    imageBytes = memoryStream.ToArray();
                    base64String = Convert.ToBase64String(imageBytes);

                    imageUrl = await AzureHttpClient.Instance.UploadProfilePictureToBLOB(base64String);

                    if (!string.IsNullOrEmpty(imageUrl))
                    {
                        r_Logger.LogInformation("Profile picture uploaded successfully");
                        await AzureHttpClient.Instance.UpdateUserProfilePictureAsync(UserName, imageUrl);
                        
                        m_CurrentUser = await AzureHttpClient.Instance.GetUserByUsernameAsync(UserName);
                        setProfilePicture();
                    }
                    else
                    {
                        r_Logger.LogError("Profile picture upload failed");
                    }
                }
            }
            catch (Exception ex)
            {
                r_Logger.LogError("An error occurred while picking the file: " + ex.Message);
            }
        }
        
        private async Task clearProfilePicture()
        {
            await AzureHttpClient.Instance.UpdateUserProfilePictureAsync(UserName, string.Empty);
            m_CurrentUser = await AzureHttpClient.Instance.GetUserByUsernameAsync(UserName);
            setProfilePicture();
        }

        #endregion
        public Command GoLocationSettingsPageCommand { get; set; }
        public Command GoNotificationSettingsPageCommand { get; set; }
        public Command GoWifiSettingsPageCommand { get; set; }
        public Command GoBluetoothSettingsPageCommand { get; set; }
        public Command GoDestinationsSettingsPageCommand { get; set; }
        public Command DarkModeToggleCommand { get; set; }
        
        public Task Init { get; }
        public bool IsDarkMode { get; set; }
        
        private async void setUser()
        {
            UserName = Preferences.Get(Constants.PREFERENCES_USERNAME, string.Empty);
            m_CurrentUser = await AzureHttpClient.Instance.GetUserByUsernameAsync(UserName);
            setProfilePicture();
        }

        public Task Initialize()
        {
            IsDarkMode = Application.Current.UserAppTheme.Equals(OSAppTheme.Dark);
            return Task.CompletedTask;
        }

        private async void onLocationSettingsButtonClicked()
        {
            await Shell.Current.Navigation.PushAsync(new LocationSettingsPage());
        }
        
        private async void onNotificationSettingsButtonClicked()
        {
            await Shell.Current.Navigation.PushAsync(new NotificationSettingsPage());
        }
        
        private async void onWifiSettingsButtonClicked()
        {
            await Shell.Current.Navigation.PushAsync(new WifiSettingsPage());
        }
        
        private async void onBluetoothSettingsButtonClicked()
        {
            await Shell.Current.Navigation.PushAsync(new BluetoothSettingsPage());
        }
        private async void onDestinationsSettingsButtonClicked()
        {
            await Shell.Current.Navigation.PushAsync(new DefinedDestinationsPage());
        }
        
        private void DarkModeToggleCommandHandler()
        {
            if (IsDarkMode)
            {
                Application.Current.UserAppTheme = OSAppTheme.Dark;
                Preferences.Set("theme", "dark");
            }
            else
            {
                Application.Current.UserAppTheme = OSAppTheme.Light;
                Preferences.Set("theme", "light");
            }
        }
        
        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
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
