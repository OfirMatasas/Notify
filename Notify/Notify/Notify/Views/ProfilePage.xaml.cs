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

        private void TapGestureRecognizer_OnTapped(object sender, EventArgs e)
        {
            throw new NotImplementedException();
        }
    }
}
