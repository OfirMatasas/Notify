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
        private string destinationsJson;
        private string m_UserName;
        private ImageSource m_ProfilePicture;
        private ObservableCollection<Destination> _scrollViewContent;
        private Task<User> currentUser;
        
        public Command LocationButtonCommand { get; set; }
        public Command BlueToothButtonCommand { get; set; }
        public Command WifiButtonCommand { get; set; }
        public Command LoadProfilePictureCommand { get; set; }
        
        private List<Destination> Destinations { get; set; }
  
        public ObservableCollection<Destination> ScrollViewContent
        {
            get => _scrollViewContent;
            set
            {
                _scrollViewContent = value;
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
            UserName = Preferences.Get(Constants.PREFERENCES_USERNAME, string.Empty);
            currentUser = AzureHttpClient.Instance.GetUserByUserNameAsync(UserName);
            SetProfilePictureAsync(); // TODO save the entire user object in preferences

            LocationButtonCommand = new Command(onLocationButtonPressed);
            BlueToothButtonCommand = new Command(onBlueToothButtonPressed);
            WifiButtonCommand = new Command(onWifiButtonPressed);
            LoadProfilePictureCommand = new Command(onLoadProfilePicture);
            
            ScrollViewContent = new ObservableCollection<Destination>();
            
            destinationsJson = Preferences.Get(Constants.PREFERENCES_DESTINATIONS, String.Empty);
            Destinations = JsonConvert.DeserializeObject<List<Destination>>(destinationsJson);
        }
        
        private async Task SetProfilePictureAsync()
        {
            User user = await currentUser;
            ProfilePicture = ImageSource.FromUri(new Uri(user.ProfilePicture));
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
            string imageUrl;
                
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
                
                using (var memoryStream = new MemoryStream())
                {
                    await stream.CopyToAsync(memoryStream);
                    var imageBytes = memoryStream.ToArray();
                    var base64String = Convert.ToBase64String(imageBytes);

                    imageUrl = await AzureHttpClient.Instance.UploadProfilePictureToBLOB(base64String);

                    if (!string.IsNullOrEmpty(imageUrl))
                    {
                        r_Logger.LogInformation("Profile picture uploaded successfully");
                        await AzureHttpClient.Instance.UpdateUserProfilePictureAsync(UserName, imageUrl);
                        
                        currentUser = AzureHttpClient.Instance.GetUserByUserNameAsync(UserName);
                        SetProfilePictureAsync();
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
            currentUser = AzureHttpClient.Instance.GetUserByUserNameAsync(UserName);
            SetProfilePictureAsync();
        }

        private void onLocationButtonPressed()
        {
            ScrollViewContent.Clear();
            
            foreach (var destination in Destinations)
            {
                ScrollViewContent.Add(new Destination(destination.Name, destination.IsDynamic)
                {
                    LastUpdatedLocation = destination.LastUpdatedLocation,
                });
            }

            OnPropertyChanged(nameof(ScrollViewContent));
        }

        private void onBlueToothButtonPressed()
        {
            ScrollViewContent.Clear();
            
            foreach (var destination in Destinations)
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
            
            foreach (var destination in Destinations)
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
