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
        
        public EditFriendPopupPage()
        {
            Label titleLabel;
            Button updatePermissionsButton;
            Frame locationFrame;
            Frame timeFrame;
            Frame dynamicFrame;
            StackLayout stackLayout;
            Frame frame;
            
            LocationPermissionPicker = CreatePicker("Location");
            TimePermissionPicker = CreatePicker("Time");  
            DynamicPermissionPicker = CreatePicker("Dynamic");

            titleLabel = new Label
            {
                Text = "Edit Friend Permissions",
                FontSize = 24,
                FontAttributes = FontAttributes.Bold,
                VerticalOptions = LayoutOptions.Center,
                HorizontalOptions = LayoutOptions.Center
            };

            updatePermissionsButton = new Button
            {
                Text = "Update Permissions",
                VerticalOptions = LayoutOptions.End,
                HorizontalOptions = LayoutOptions.FillAndExpand
            };
            
            updatePermissionsButton.Clicked += UpdatePermissionsButton_Clicked;

            locationFrame = new Frame { Content = LocationPermissionPicker, CornerRadius = 5, Padding = 10, BackgroundColor = Color.LightGray };
            timeFrame = new Frame { Content = TimePermissionPicker, CornerRadius = 5, Padding = 10, BackgroundColor = Color.LightGray };
            dynamicFrame = new Frame { Content = DynamicPermissionPicker, CornerRadius = 5, Padding = 10, BackgroundColor = Color.LightGray };

            stackLayout = new StackLayout
            {
                Children =
                {
                    titleLabel,
                    locationFrame,
                    timeFrame,
                    dynamicFrame,
                    updatePermissionsButton
                },
                Padding = new Thickness(20),
                Spacing = 20
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
