using System;
using System.Collections.Generic;

using Xamarin.Forms;

namespace Notify.Forms
{
    public partial class FormLogin : ContentPage
    {
        public FormLogin()
        {
            InitializeComponent();

            passwordEntry.Completed += buttonLogin_Clicked;
        }

        private async void buttonSignUp_Clicked(object sender, EventArgs args)
        {
            // await Navigation.PushAsync(new SignUpPage());
        }

        private async void buttonLogin_Clicked(object i_Sender, EventArgs i_Args)
        {
            string username = usernameEntry.Text;
            string password = passwordEntry.Text;

            if (string.IsNullOrEmpty(username))
            {
                messageLabel.Text = "Username cannot be empty";
            }
            else if (string.IsNullOrEmpty(password))
            {
                messageLabel.Text = "Password cannot be empty";
            }
            else
            {
                UserCredentials user = new UserCredentials
                {
                    Username = username,
                    Password = password
                };

                if (AreCredentialsCorrect(user))
                {
                    App.IsUserLoggedIn = true;
                    Navigation.InsertPageBefore(new MainPage(), this);
                    await Navigation.PopAsync();
                }
                else
                {
                    messageLabel.Text = "Login failed - Invalid credentials";
                    passwordEntry.Text = string.Empty;
                }
            }
        }

        private bool AreCredentialsCorrect(UserCredentials i_User)
        {
            return i_User.Username.Equals(Constants.Username)
                && i_User.Password.Equals(Constants.Password);
        }
    }
}
