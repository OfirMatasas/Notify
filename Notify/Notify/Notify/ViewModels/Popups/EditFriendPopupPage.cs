using Rg.Plugins.Popup.Pages;
using Xamarin.Forms;

namespace Notify.ViewModels.Popups
{
    public class EditFriendPopupPage : PopupPage
    {
        public Picker LocationPicker { get; set; }
        public Picker TimePicker { get; set; }
        public Picker DynamicPicker { get; set; }

        public EditFriendPopupPage()
        {
            LocationPicker = new Picker { Title = "Location" };
            TimePicker = new Picker { Title = "Time" };
            DynamicPicker = new Picker { Title = "Dynamic" };

            LocationPicker.Items.Add("Allow");
            LocationPicker.Items.Add("Disallow");
            TimePicker.Items.Add("Allow");
            TimePicker.Items.Add("Disallow");
            DynamicPicker.Items.Add("Allow");
            DynamicPicker.Items.Add("Disallow");

            Label titleLabel = new Label
            {
                Text = "Edit Friend Settings",
                FontSize = 24,
                FontAttributes = FontAttributes.Bold,
                VerticalOptions = LayoutOptions.Center,
                HorizontalOptions = LayoutOptions.Center
            };

            Button acceptButton = new Button
            {
                Text = "Accept",
                VerticalOptions = LayoutOptions.End,
                HorizontalOptions = LayoutOptions.FillAndExpand
            };
            acceptButton.Clicked += AcceptButton_Clicked;

            var locationFrame = new Frame { Content = LocationPicker, CornerRadius = 5, Padding = 10, BackgroundColor = Color.LightGray };
            var timeFrame = new Frame { Content = TimePicker, CornerRadius = 5, Padding = 10, BackgroundColor = Color.LightGray };
            var dynamicFrame = new Frame { Content = DynamicPicker, CornerRadius = 5, Padding = 10, BackgroundColor = Color.LightGray };

            var stackLayout = new StackLayout
            {
                Children =
                {
                    titleLabel,
                    locationFrame,
                    timeFrame,
                    dynamicFrame,
                    acceptButton
                },
                Padding = new Thickness(20),
                Spacing = 20
            };

            var frame = new Frame
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

        private void AcceptButton_Clicked(object sender, System.EventArgs e)
        {
            string selectedLocation = LocationPicker.SelectedItem?.ToString();
            string selectedTime = TimePicker.SelectedItem?.ToString();
            string selectedDynamic = DynamicPicker.SelectedItem?.ToString();

            MessagingCenter.Send<EditFriendPopupPage, (string, string, string)>(this, "EditFriendValues", (selectedLocation, selectedTime, selectedDynamic));
        }
    }
}
