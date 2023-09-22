using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Notify.Azure.HttpClient;
using Notify.Core;
using Notify.Services;
using Xamarin.Essentials;
using Xamarin.Forms;
using Constants = Notify.Helpers.Constants;

namespace Notify.ViewModels
{
    public sealed class ProfilePageViewModel : INotifyPropertyChanged
    {
        private readonly LoggerService r_Logger = LoggerService.Instance;
        private string m_UserName;
        private ImageSource m_ProfilePicture;
        private ObservableCollection<Destination> m_ScrollViewContent;
        private User m_CurrentUser;
        
        public Command LocationButtonCommand { get; set; }
        public Command BlueToothButtonCommand { get; set; }
        public Command WifiButtonCommand { get; set; }
        public Command LoadProfilePictureCommand { get; set; }
        
        private List<Destination> Destinations { get; set; }
  
        public ObservableCollection<Destination> ScrollViewContent
        {
            get => m_ScrollViewContent;
            set
            {
                m_ScrollViewContent = value;
                OnPropertyChanged(nameof(ScrollViewContent));
            }
        }       
        
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
        
        public ProfilePageViewModel()
        {
            setUser();

            LocationButtonCommand = new Command(onLocationButtonPressed);
            BlueToothButtonCommand = new Command(onBlueToothButtonPressed);
            WifiButtonCommand = new Command(onWifiButtonPressed);
            LoadProfilePictureCommand = new Command(onLoadProfilePicture);
            
            ScrollViewContent = new ObservableCollection<Destination>();
            
            string destinationsJson = Preferences.Get(Constants.PREFERENCES_DESTINATIONS, String.Empty);
            Destinations = JsonConvert.DeserializeObject<List<Destination>>(destinationsJson);
        }

        private async void setUser()
        {
            UserName = Preferences.Get(Constants.PREFERENCES_USERNAME, string.Empty);
            m_CurrentUser = await AzureHttpClient.Instance.GetUserByUsernameAsync(UserName);
            setProfilePicture();
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

        private void onLocationButtonPressed()
        {
            ScrollViewContent.Clear();
            
            foreach (Destination destination in Destinations)
            {
                ScrollViewContent.Add(new Destination(destination.Name, destination.IsDynamic)
                {
                    LastUpdatedLocation = destination.LastUpdatedLocation,
                    Locations = destination.IsDynamic ? null : destination.Locations,
                    Address = destination.Address
                });
            }

            OnPropertyChanged(nameof(ScrollViewContent));
        }

        private void onBlueToothButtonPressed()
        {
            ScrollViewContent.Clear();
            
            foreach (Destination destination in Destinations)
            {
                if (!string.IsNullOrWhiteSpace(destination.Bluetooth))
                {
                    ScrollViewContent.Add(new Destination(destination.Name)
                    {
                        Bluetooth = destination.Bluetooth
                    });
                }
            }
            
            OnPropertyChanged(nameof(ScrollViewContent));
        }

        private void onWifiButtonPressed()
        {
            ScrollViewContent.Clear();
            
            foreach (Destination destination in Destinations)
            {
                if (!string.IsNullOrWhiteSpace(destination.SSID))
                {
                    ScrollViewContent.Add(new Destination(destination.Name)
                    {
                        SSID = destination.SSID
                    });
                }
            }
            
            OnPropertyChanged(nameof(ScrollViewContent));
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
