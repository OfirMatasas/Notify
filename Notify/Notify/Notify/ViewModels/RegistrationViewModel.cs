using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Text.RegularExpressions;
using Notify.Helpers;
using Xamarin.Forms;

namespace Notify.ViewModels
{
    public sealed class RegistrationPageViewModel : INotifyPropertyChanged
    {
        private string m_Telephone;
        private bool m_IsFormValid;

        public RegistrationPageViewModel()
        {
            SignUpCommand = new Command(onSignUpClicked);
            BackCommand = new Command(onBackClicked);
        }
        
        public Command SignUpCommand { get; set; }
        
        public Command BackCommand { get; set; }

        public string Name { get; set; }

        public string UserName { get; set; }

        public string Password { get; set; }

        public string ConfirmPassword { get; set; }
        
        public event PropertyChangedEventHandler PropertyChanged;
        
        public object NameBorderColor { get; set; }
        
        public object UserNameBorderColor { get; set; }
        
        public object TelephoneBorderColor { get; set; }
        
        public object PasswordBorderColor { get; set; }
        
        public object ConfirmPasswordBorderColor { get; set; }
        
        public string Telephone
        {
            get => m_Telephone;
            set
            {
                m_Telephone = value;
                OnPropertyChanged(nameof(Telephone));
            }
        }

        private bool validateName()
        {
            bool isValid = !string.IsNullOrEmpty(Name) && Regex.IsMatch(Name, @"^[a-zA-Z ]+$");

            if (isValid)
            {
                NameBorderColor = Constants.VALID_COLOR;
            }
            else
            {
                NameBorderColor = Constants.INVALID_COLOR;
                displayError("Please enter a valid name consisting only of letters.");
            }
            
            return isValid;
        }

        private bool validatePassword()
        {
            bool isValid = !string.IsNullOrEmpty(Password) && 
                           Regex.IsMatch(Password, @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]{8,}$") &&
                           Password == ConfirmPassword;

            if (isValid)
            {
                PasswordBorderColor = Constants.VALID_COLOR;
                ConfirmPasswordBorderColor = Constants.VALID_COLOR;
            }
            else
            {
                PasswordBorderColor = Constants.INVALID_COLOR;
                ConfirmPasswordBorderColor = Constants.INVALID_COLOR;

                if (string.IsNullOrEmpty(Password))
                {
                    displayError("Please enter a password.");
                }
                else if (Password != ConfirmPassword)
                {
                    displayError("Passwords do not match.");
                }
                else
                {
                    displayError("Please enter a password containing at least 8 characters, with at least one uppercase letter, one lowercase letter, one number, and one special character.");
                }
            }

            return isValid;
        }
        
        private bool validateUserName()
        {
            bool isValid = !string.IsNullOrEmpty(UserName) && Regex.IsMatch(UserName, @"^[a-zA-Z0-9]+$");
    
            if (isValid)
            {
                UserNameBorderColor = Constants.VALID_COLOR;
            }
            else
            {
                UserNameBorderColor = Constants.INVALID_COLOR;
                displayError("Please enter a valid username consisting only of letters and numbers.");
            }
    
            return isValid;
        }
        
        private bool validateTelephone()
        {
            if (string.IsNullOrEmpty(Telephone))
            {
                return true;
            }

            if (Telephone.StartsWith("0"))
            {
                if (Telephone.StartsWith("05"))
                {
                    bool isValid = Regex.IsMatch(Telephone, @"^\d{10}$");

                    if (!isValid)
                    {
                        TelephoneBorderColor = Constants.INVALID_COLOR;
                        displayError("Please enter a valid 10-digit telephone number starting with '05'.");
                    }
                    else
                    {
                        TelephoneBorderColor = Constants.VALID_COLOR;
                    }

                    return isValid;
                }

                TelephoneBorderColor = Constants.INVALID_COLOR;
                displayError("Please enter a valid 10-digit telephone number starting with '05'.");
                return false;
            }

            TelephoneBorderColor = Constants.INVALID_COLOR;
            displayError("Please enter a valid 10-digit telephone number starting with '05'.");
            return false;
        }
        
        private void displayError(string message)
        {
            Application.Current.MainPage.DisplayAlert("Error", message, "OK");
        }

        private async void onSignUpClicked()
        {
            if (string.IsNullOrEmpty(Name) || string.IsNullOrEmpty(UserName) || string.IsNullOrEmpty(Password) || string.IsNullOrEmpty(ConfirmPassword) || string.IsNullOrEmpty(Telephone))
            {
                displayError("Please fill in all required fields.");
                return;
            }

            bool isNameValid = validateName();
            bool isUserNameValid = validateUserName();
            bool isPasswordValid = validatePassword();
            bool isTelephoneValid = validateTelephone();

            if (isNameValid && isUserNameValid && isPasswordValid && isTelephoneValid)
            {
                Debug.WriteLine($"You have successfully signed up!{Environment.NewLine}Name: {Name}{Environment.NewLine}UserName: {UserName}{Environment.NewLine}Password: {Password}{Environment.NewLine}Telephone: {Telephone}");
                await Application.Current.MainPage.DisplayAlert("Success", "You have successfully signed up!", "OK");
                await Shell.Current.GoToAsync("///login");
            }
        }
        
        private async void onBackClicked()
        {
            await Shell.Current.GoToAsync("///login");
        }

        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
