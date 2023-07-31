using System;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Notify.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ProfilePage : ContentPage
    {
        public ProfilePage()
        {
            InitializeComponent();
            BindingContext = new ViewModels.ProfilePageViewModel();
        }
        
        private void onPressed(object sender, EventArgs e)
        {
            var button = (ImageButton)sender;
            button.Opacity = 1.0;
        }

        private void onReleased(object sender, EventArgs e)
        {
            var button = (ImageButton)sender;
            button.Opacity = 0.3;
        }
        
        private void LocationButton_OnPressed(object sender, EventArgs e)
        {
            onPressed(sender, e);
        }

        private void LocationButton_OnReleased(object sender, EventArgs e)
        {
            onReleased(sender, e);
        }

        private void BlueToothButton_OnPressed(object sender, EventArgs e)
        {
            onPressed(sender, e);
        }

        private void BlueToothButton_OnReleased(object sender, EventArgs e)
        {
            onReleased(sender, e);
        }

        private void WifiButton_OnPressed(object sender, EventArgs e)
        {
            onPressed(sender, e);
        }

        private void WifiButton_OnReleased(object sender, EventArgs e)
        {
            onReleased(sender, e);
        }
    }
}
