using System;
using Notify.Views.TabViews;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Notify.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class WelcomePage : ContentPage
    {
        private string UserName { get; set; }
        private string Password { get; set; }
        public WelcomePage()
        {
            InitializeComponent();
        }


        private async void SignInButton_OnClicked(object sender, EventArgs e)
        {
            UserName = userName.Text;
            Password = password.Text;
            if (UserName.Equals("lin") && Password.Equals("123"))
            {
                Console.WriteLine("logged successfully!");
                await Navigation.PushAsync(new HomeView());
            }
        }
    }
}