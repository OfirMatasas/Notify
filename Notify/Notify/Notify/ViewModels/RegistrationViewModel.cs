using System.ComponentModel;
using System.Diagnostics;
using System.Text.RegularExpressions;
using Xamarin.Forms;

namespace Notify.ViewModels
{
    public class RegistrationPageViewModel : INotifyPropertyChanged
    {
        private string m_Telephone;
        private bool m_IsFormValid;

        public RegistrationPageViewModel()
        {
            SignUpCommand = new Command(OnSignUpClicked);
            BackCommand = new Command(OnBackClicked);
        }
        
        public Command SignUpCommand { get; set; }
        
        public Command BackCommand { get; set; }

        public string Name { get; set; }

        public string UserName { get; set; }

        public string Password { get; set; }

        public string ConfirmPassword { get; set; }

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
        
        private bool ValidateName()
        {
            bool isValid = !string.IsNullOrEmpty(Name) && Regex.IsMatch(Name, @"^[a-zA-Z ]+$");

            if (!isValid)
            {
                DisplayError("Please enter a valid name consisting only of letters.");
            }

            return isValid;
        }

        private bool ValidatePassword()
        {
            bool isValid = !string.IsNullOrEmpty(Password) && Password == ConfirmPassword;

            if (!isValid)
            {
                DisplayError("Please enter matching passwords.");
            }

            return isValid;
        }
        
        private bool ValidateTelephone()
        {
            if (!string.IsNullOrEmpty(Telephone))
            {
                if (Telephone.StartsWith("0"))
                {
                    if (!Telephone.StartsWith("05"))
                    {
                        DisplayError("Please enter a valid 10-digit telephone number starting with '05'.");
                        return false;
                    }
                }

                bool isValid = Regex.IsMatch(Telephone, @"^\d{10}$");

                if (!isValid)
                {
                    DisplayError("Please enter a valid 10-digit telephone number starting with '05'.");
                }

                return isValid;
            }

            return true;
        }
        
        private void DisplayError(string message)
        {
            Application.Current.MainPage.DisplayAlert("Error", message, "OK");
        }

        private async void OnSignUpClicked()
        {
            if (string.IsNullOrEmpty(Name) || string.IsNullOrEmpty(UserName) || string.IsNullOrEmpty(Password) || string.IsNullOrEmpty(ConfirmPassword) || string.IsNullOrEmpty(Telephone))
            {
                DisplayError("Please fill in all required fields.");
                return;
            }
    
            // ValidateName();
            // ValidatePassword();
            // ValidateTelephone();

            if (ValidateName() && ValidatePassword() && ValidateTelephone())
            {
                Debug.WriteLine($"You have successfully signed up!\nName: {Name}\nUserName: {UserName}\nPassword: {Password}\nTelephone: {Telephone}");
                await Application.Current.MainPage.DisplayAlert("Success", "You have successfully signed up!", "OK");
                await Shell.Current.GoToAsync("///welcome");
            }
        }
        
        private async void OnBackClicked()
        {
            await Shell.Current.GoToAsync("///welcome");
        }
        
        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
