using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Text.RegularExpressions;
using Notify.Helpers;
using Xamarin.Forms;
using System.Security.Cryptography;
using Twilio;
using Twilio.Rest.Api.V2010.Account;
using Twilio.Types;
using Xamarin.Essentials;

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
                PasswordBorderColor = ConfirmPasswordBorderColor = Constants.VALID_COLOR;
            }
            else
            {
                PasswordBorderColor = ConfirmPasswordBorderColor = Constants.INVALID_COLOR;

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
            bool isValid = false;

            if (!string.IsNullOrEmpty(Telephone))
            {
                if (Regex.IsMatch(Telephone, @"^05\d{8}$"))
                {
                    isValid = true;
                }
                else
                {
                    displayError("Please enter a valid 10-digit telephone number starting with '05'.");
                }
            }
            else
            {
                displayError("Please enter a telephone number.");
            }

            TelephoneBorderColor = isValid ? Constants.VALID_COLOR : Constants.INVALID_COLOR;

            return isValid;
        }

        
        private void displayError(string message)
        {
            Application.Current.MainPage.DisplayAlert("Error", message, "OK");
        }

        private async void onSignUpClicked()
        {
            // Validate the user input
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
                string verificationCode = generateVerificationCode();
                string twilioPhoneNumber = ConvertToTwilioPhoneNumber(Telephone);
                
                const string accountSid = "AC69d9dfdd4925966544c7fe354872f852";
                const string authToken = "74188da6cd7b6c9ebab0dba30e369ac9";
                TwilioClient.Init(accountSid, authToken);

                var messageOptions = new CreateMessageOptions(
                    new PhoneNumber(twilioPhoneNumber));
                messageOptions.From = new PhoneNumber("+16812068707");
                messageOptions.Body = $"Your Notify verification code: {verificationCode}";
                
                var message = await MessageResource.CreateAsync(messageOptions);

                Debug.WriteLine($"SMS sent successfully to {message.To} with Message SID: {message.Sid}");
                Debug.WriteLine(message.Body);
            }
        }

        private string ConvertToTwilioPhoneNumber(string phoneNumber)
        {
            if (string.IsNullOrEmpty(phoneNumber))
            {
                return string.Empty;
            }

            string twilioPhoneNumber = phoneNumber.Trim();

            if (twilioPhoneNumber.StartsWith("05") && twilioPhoneNumber.Length == 10)
            {
                twilioPhoneNumber = $"+972{twilioPhoneNumber.Substring(1)}";
            }

            return twilioPhoneNumber;
        }

        private async void onBackClicked()
        {
            await Shell.Current.GoToAsync("///login");
        }

        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        
        private string generateVerificationCode()
        {
            using (var randomNumberGenerator = new RNGCryptoServiceProvider())
            {
                var randomBytes = new byte[4];
                randomNumberGenerator.GetBytes(randomBytes);
                int code = BitConverter.ToInt32(randomBytes, 0) & 0x7FFFFFFF; // Ensure the generated number is positive
                return (code % 1000000).ToString("D6"); // Format the number as a 6-digit string
            }
        }
    }
}
