using Notify.Helpers;
using Rg.Plugins.Popup.Pages;
using Xamarin.Forms;

namespace Notify.ViewModels.Popups
{
    public class EditFriendPopupPage : PopupPage
    {
        public Picker LocationPermissionPicker { get; set; }
        public Picker TimePermissionPicker { get; set; }
        public Picker DynamicPermissionPicker { get; set; }
        
        public EditFriendPopupPage(string currentLocation, string currentTime, string currentDynamic)
        {
            Label titleLabel, locationLabel, timeLabel, dynamicLabel;
            Button updatePermissionsButton;
            Frame locationFrame, timeFrame, dynamicFrame, frame;
            StackLayout stackLayout;
            
            LocationPermissionPicker = CreatePicker("Location");
            TimePermissionPicker = CreatePicker("Time");  
            DynamicPermissionPicker = CreatePicker("Dynamic");
            
            LocationPermissionPicker.SelectedItem = currentLocation;
            TimePermissionPicker.SelectedItem = currentTime;
            DynamicPermissionPicker.SelectedItem = currentDynamic;

            titleLabel = new Label
            {
                Text = "Edit Friend Permissions",
                FontSize = 24,
                FontAttributes = FontAttributes.Bold,
                VerticalOptions = LayoutOptions.Center,
                HorizontalOptions = LayoutOptions.Center,
                TextColor = Color.Black
            };

            locationLabel = new Label
            {
                Text = "Location",
                FontSize = 18,
                FontAttributes = FontAttributes.Bold,
                VerticalOptions = LayoutOptions.Center,
                HorizontalOptions = LayoutOptions.Center,
                TextColor = Color.Black
            };
            
            timeLabel = new Label
            {
                Text = "Time",
                FontSize = 18,
                FontAttributes = FontAttributes.Bold,
                VerticalOptions = LayoutOptions.Center,
                HorizontalOptions = LayoutOptions.Center,
                TextColor = Color.Black
            };
            
            dynamicLabel = new Label
            {
                Text = "Dynamic",
                FontSize = 18,
                FontAttributes = FontAttributes.Bold,
                VerticalOptions = LayoutOptions.Center,
                HorizontalOptions = LayoutOptions.Center,
                TextColor = Color.Black
            };

            updatePermissionsButton = new Button
            {
                Text = "Update Permissions",
                VerticalOptions = LayoutOptions.End,
                HorizontalOptions = LayoutOptions.FillAndExpand
            };
            
            updatePermissionsButton.Clicked += UpdatePermissionsButton_Clicked;

            locationFrame = new Frame { Content = LocationPermissionPicker, CornerRadius = 5, Padding = 5, BackgroundColor = Color.LightGray };
            timeFrame = new Frame { Content = TimePermissionPicker, CornerRadius = 5, Padding = 5, BackgroundColor = Color.LightGray };
            dynamicFrame = new Frame { Content = DynamicPermissionPicker, CornerRadius = 5, Padding = 5, BackgroundColor = Color.LightGray };

            stackLayout = new StackLayout
            {
                Children =
                {
                    titleLabel,
                    locationLabel,
                    locationFrame,
                    timeLabel,
                    timeFrame,
                    dynamicLabel,
                    dynamicFrame,
                    updatePermissionsButton
                },
                Padding = new Thickness(15),
                Spacing = 15
            };

            frame = new Frame
            {
                Content = stackLayout,
                VerticalOptions = LayoutOptions.CenterAndExpand,
                HorizontalOptions = LayoutOptions.CenterAndExpand,
                BackgroundColor = Color.White,
                CornerRadius = 10,
                HasShadow = true,
                Padding = new Thickness(20)
            };

            Content = new ContentView
            {
                Content = frame,
                VerticalOptions = LayoutOptions.CenterAndExpand,
                HorizontalOptions = LayoutOptions.CenterAndExpand,
            };
        }
        
        private Picker CreatePicker(string title)
        {
            Picker picker = new Picker { Title = title };
            picker.Items.Add(Constants.NOTIFICATION_PERMISSION_ALLOW);
            picker.Items.Add(Constants.NOTIFICATION_PERMISSION_DISALLOW);
            
            return picker;
        }

        private void UpdatePermissionsButton_Clicked(object sender, System.EventArgs e)
        {
            string selectedLocation = LocationPermissionPicker.SelectedItem?.ToString();
            string selectedTime = TimePermissionPicker.SelectedItem?.ToString();
            string selectedDynamic = DynamicPermissionPicker.SelectedItem?.ToString();

            MessagingCenter.Send<EditFriendPopupPage, (string, string, string)>(this, "EditFriendValues", (selectedLocation, selectedTime, selectedDynamic));
        }
    }
}
