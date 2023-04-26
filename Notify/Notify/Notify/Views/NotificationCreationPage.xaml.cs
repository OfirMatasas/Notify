using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Notify.ViewModels;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Notify.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class NotificationCreationPage : ContentPage
    {
        public NotificationCreationPage()
        {
            InitializeComponent();
            BindingContext = new NotificationCreationViewModel();
        }

        private void SelectableItemsView_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            IEnumerable<object> removedItems = e.PreviousSelection.Except(e.CurrentSelection);
            IEnumerable<object> addedItems = e.CurrentSelection.Except(e.PreviousSelection);

            removedItems.ToList().ForEach(item => ((NotificationCreationViewModel.Friend)item).IsSelected = false);
            addedItems.ToList().ForEach(item => ((NotificationCreationViewModel.Friend)item).IsSelected = true);
        }

        private void DatePicker_OnDateSelected(object sender, DateChangedEventArgs e)
        {
            DateTime datePicked = e.NewDate.Add(TimePicker.Time);

            if (datePicked < DateTime.Now)
            {
                DisplayAlert("Invalid Date", "Please select a time in the future", "OK");
            }
            
            NotificationCreationViewModel.SelectedDateOption = e.NewDate;
        }

        private void TimePicker_OnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            DateTime timePicked;

            if (e.PropertyName.Equals(TimePicker.TimeProperty.PropertyName))
            {
                timePicked = DatePicker.Date.Add(TimePicker.Time);

                if (timePicked < DateTime.Now)
                {
                    DisplayAlert("Error", "Please select a time in the future.", "OK");
                }

                NotificationCreationViewModel.SelectedTimeOption = TimePicker.Time;
            }
        }
    }
}
