using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;
using Xamarin.Forms;
using System.Linq;
using static System.Net.Mime.MediaTypeNames;

namespace Notify.Forms
{	
	public partial class FormRegistration : ContentPage
	{
        private Regex r_PasswordRegex = new Regex(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*[0-9])(?=.*[!@#\$%\^&\*])(?=.{8,})");

        public FormRegistration ()
		{
			InitializeComponent ();
			Title = "Registration";
		}

        private void ButtonSignUp_Clicked(object sender, EventArgs args)
        {
            if(allFieldsAreValid())
            {
                //ToDo: Send info to server
            }
        }

        private bool allFieldsAreValid()
        {
            return validatePassword() && validateUsername();
        }

        private bool validateUsername()
        {
            bool valid = false;
            string errorMessage = null;
            string username = Username.Text;

            if (string.IsNullOrEmpty(username))
            {
                errorMessage = "Username must not be empty";
            }
            else if (username.Length < 6 || username.Length > 12)
            {
                errorMessage = "Username must be between 6 and 12 characters";
            }
            else if (!username.All(char.IsLetterOrDigit))
            {
                errorMessage = "Username must contain only alphabet characters or numbers";
            }
            else
            {
                valid = true;
            }

            if (!valid)
            {
                DisplayAlert("Error", errorMessage, "OK");
            }

            return valid;
        }

        private bool validatePassword()
		{
			bool valid = false;
            string errorMessage = null;
            string password = Password.Text;
            Regex r_PasswordRegex = new Regex(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*[0-9])(?=.*[!@#\$%\^&\*])(?=.{8,})");

            if(string.IsNullOrEmpty(password))
            {
                errorMessage = "Password cannot be empty";
            }
            else if (string.IsNullOrEmpty(PasswordConfirmation.Text) || !password.Equals(PasswordConfirmation.Text))
			{
                errorMessage = "Passwords not match";
            }
            else if (!r_PasswordRegex.IsMatch(password))
            {
                errorMessage = "Password must contain at least one digit, one uppercase character and one special symbol";
            }
            else
            {
                valid = true;
            }

            if (!valid)
			{
                DisplayAlert("Error", errorMessage, "OK");
			}

			return valid;
		}
    }
}

