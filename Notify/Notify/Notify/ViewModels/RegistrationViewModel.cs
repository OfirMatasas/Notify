using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
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
        }

        public Command SignUpCommand { get; set; }

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

        private void ValidateName()
        {
            bool isValid = !string.IsNullOrEmpty(Name) && Regex.IsMatch(Name, @"^[a-zA-Z]+$");

            if (!isValid)
            {
                DisplayError("Please enter a valid name consisting only of letters.");
            }
        }

        private void ValidatePassword()
        {
            bool isValid = !string.IsNullOrEmpty(Password) && Password == ConfirmPassword;

            if (!isValid)
            {
                DisplayError("Please enter matching passwords.");
            }
        }

        private void ValidateTelephone()
        {
            if (!string.IsNullOrEmpty(Telephone))
            {
                if (Telephone.StartsWith("0"))
                {
                    if (!Telephone.StartsWith("05"))
                    {
                        DisplayError("Please enter a valid 10-digit telephone number starting with '05'.");
                        return;
                    }
                }

                bool isValid = Regex.IsMatch(Telephone, @"^\d{10}$");

                if (!isValid)
                {
                    DisplayError("Please enter a valid 10-digit telephone number starting with '05'.");
                }
            }
        }

        private void DisplayError(string message)
        {
            Application.Current.MainPage.DisplayAlert("Error", message, "OK");
        }

        private void OnSignUpClicked()
        {
            ValidateName();
            ValidatePassword();
            ValidateTelephone();

            if (!string.IsNullOrEmpty(Name) && !string.IsNullOrEmpty(UserName) && !string.IsNullOrEmpty(Password) && !string.IsNullOrEmpty(ConfirmPassword) && !string.IsNullOrEmpty(Telephone))
            {
                IsFormValid = true;

                var registrationData = new
                {
                    Name,
                    UserName,
                    Password,
                    Telephone
                };

                // Convert the registration data to JSON and write it to a file
                string jsonData = JsonConvert.SerializeObject(registrationData);
                string fileName = "registration_data.json";
                File.WriteAllText(fileName, jsonData);

                Application.Current.MainPage.DisplayAlert("Success", "You have successfully signed up!", "OK");
            }
            else
            {
                DisplayError("Please fill in all required fields.");
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
