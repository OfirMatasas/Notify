using System.ComponentModel;
using System.Diagnostics;
using System.Text.RegularExpressions;
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
        
        public string Telephone
        {
            get => m_Telephone;
            set
            {
                m_Telephone = value;
                OnPropertyChanged(nameof(Telephone));
            }
        }

        public bool IsFormValid
        {
            get => m_IsFormValid;
            set
            {
                m_IsFormValid = value;
                OnPropertyChanged(nameof(IsFormValid));
            }
        }
        
        private bool validateName()
        {
            bool isValid = !string.IsNullOrEmpty(Name) && Regex.IsMatch(Name, @"^[a-zA-Z ]+$");

            if (!isValid)
            {
                displayError("Please enter a valid name consisting only of letters.");
            }

            return isValid;
        }

        private bool validatePassword()
        {
            bool isValid = !string.IsNullOrEmpty(Password) && 
                           Regex.IsMatch(Password, @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]{8,}$") &&
                           Password == ConfirmPassword;

            if (!isValid)
            {
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
        
        private bool validateTelephone()
        {
            if (!string.IsNullOrEmpty(Telephone))
            {
                if (Telephone.StartsWith("0"))
                {
                    if (!Telephone.StartsWith("05"))
                    {
                        displayError("Please enter a valid 10-digit telephone number starting with '05'.");
                        return false;
                    }
                }

                bool isValid = Regex.IsMatch(Telephone, @"^\d{10}$");

                if (!isValid)
                {
                    displayError("Please enter a valid 10-digit telephone number starting with '05'.");
                }

                return isValid;
            }

            return true;
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

            if (validateName() && validatePassword() && validateTelephone())
            {
                Debug.WriteLine($"You have successfully signed up!\nName: {Name}\nUserName: {UserName}\nPassword: {Password}\nTelephone: {Telephone}");
                await Application.Current.MainPage.DisplayAlert("Success", "You have successfully signed up!", "OK");
                await Shell.Current.GoToAsync("///welcome");
            }
        }
        
        private async void onBackClicked()
        {
            await Shell.Current.GoToAsync("///welcome");
        }

        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
