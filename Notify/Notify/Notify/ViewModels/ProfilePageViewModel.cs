using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Xamarin.Essentials;
using Xamarin.Forms;
using Constants = Notify.Helpers.Constants;

namespace Notify.ViewModels
{
    public class ProfilePageViewModel : INotifyPropertyChanged
    { 
        public Command GoSettingsPageCommand { get; set; }
        private string m_UserName;
        public string UserName 
        { 
            get => m_UserName;
            set
            {
                m_UserName = value;
                OnPropertyChanged(nameof(UserName));
            }
        }

        public ProfilePageViewModel()
        {
            GoSettingsPageCommand = new Command(onSettingsButtonClicked);
            UserName = Preferences.Get(Constants.PREFERENCES_USERNAME, string.Empty);
        }

        private async void onSettingsButtonClicked()
        {
            await Shell.Current.GoToAsync(Constants.SHELL_NAVIGATION_REGISTER);
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        protected bool SetField<T>(ref T field, T value, [CallerMemberName] string propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(field, value)) return false;
            field = value;
            OnPropertyChanged(propertyName);
            return true;
        }
    }
}
