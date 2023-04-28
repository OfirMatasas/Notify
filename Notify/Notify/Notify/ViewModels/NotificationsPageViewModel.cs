using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using Xamarin.Forms;

namespace Notify.ViewModels
{
    public class NotificationsPageViewModel : INotifyPropertyChanged
    {
        public NotificationsPageViewModel()
        {
            CreateNotificationCommand = new Command(onCreateNotificationClicked);
        }

        private async void onCreateNotificationClicked()
        {
            await Shell.Current.GoToAsync("///create_notification");
        }

        public Command CreateNotificationCommand { get; set; }
        public bool TimeFilter { get; set; }
        
        private static DateTime m_StartDate = DateTime.Today;
        public static DateTime StartDate
        {
            get => m_StartDate;
            set => m_StartDate = value;
        }
        
        private static DateTime m_EndDate = DateTime.Today;
        public static DateTime EndDate
        {
            get => m_EndDate;
            set => m_EndDate = value;
        }

        public List<string> LocationsList { get; } = new List<string>
        {
            "Home",
            "Work",
            "School",
            "Other"
        };
        
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

        private ObservableCollection<string> m_Locations = new ObservableCollection<string>
        {
            "Home",
            "Work",
            "School",
            "Other"
        };

        private string m_SelectedLocation;
        private bool m_LocationFilter;

        public ObservableCollection<string> Locations
        {
            get { return m_Locations; }
            set { SetProperty(ref m_Locations, value); }
        }

        public string SelectedLocation
        {
            get => m_SelectedLocation;
            set => SetProperty(ref m_SelectedLocation, value);
        }

        public bool LocationFilter
        {
            get => m_LocationFilter;
            set => SetProperty(ref m_LocationFilter, value);
        }

        public ICommand SelectMultipleLocationsCommand => new Command(async (locationPicker) =>
        {
            var selectedLocations = await App.Current.MainPage.DisplayActionSheet("Select Locations", "Cancel", null, Locations.ToArray());

            if (selectedLocations != null && selectedLocations != "Cancel")
            {
                var selectedLocationsList = selectedLocations.Split(',');

                var selectedLocationsCollection = new ObservableCollection<string>(selectedLocationsList);

                SelectedLocation = string.Join(", ", selectedLocationsCollection);

                // Update the picker's selected item
                if (locationPicker is Picker picker)
                {
                    picker.SelectedItem = SelectedLocation;
                }
            }
        });

        private string m_LocationSearchText;

        public string LocationSearchText
        {
            get => m_LocationSearchText;
            set => SetProperty(ref m_LocationSearchText, value);
        }

        public List<string> FilteredLocations { get; set; } = new List<string>
        {
            "Home",
            "Work",
            "School",
            "Other"
        };

        protected bool SetProperty<T>(ref T backingStore, T value, [CallerMemberName] string propertyName = "", Action onChanged = null)
        {
            if (EqualityComparer<T>.Default.Equals(backingStore, value))
                return false;

            backingStore = value;

            onChanged?.Invoke();

            OnPropertyChanged(propertyName);

            return true;
        }
    }
}
