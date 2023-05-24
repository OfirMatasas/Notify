using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using Notify.Helpers;
using Xamarin.Forms;
using System.Security.Cryptography;
using System.Threading.Tasks;
using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using Notify.Azure.HttpClient;
using Twilio;
using Twilio.Rest.Api.V2010.Account;
using Twilio.Types;

namespace Notify.ViewModels
{
    public sealed class RegistrationPageViewModel : INotifyPropertyChanged
    {
        private readonly LoggerService r_logger = LoggerService.Instance;
        private string m_Telephone;

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

        public string VerificationCode { get; set; }

        public List<string> ErrorMessages { get; } = new List<string>();
        
        public string Telephone
        {
            get => m_Telephone;
            set
            {
                m_Telephone = value;
                OnPropertyChanged(nameof(Telephone));
            }
        }

        private void validateName()
        {
            if (string.IsNullOrEmpty(Name))
            {
                NameBorderColor = Constants.INVALID_COLOR;
                displayError("Please fill in your name.");
            }
            else
            {
                bool isValid = Regex.IsMatch(Name, @"^[a-zA-Z ]+$");

                if (isValid)
                {
                    NameBorderColor = Constants.VALID_COLOR;
                }
                else
                {
                    NameBorderColor = Constants.INVALID_COLOR;
                    displayError("Please enter a valid name consisting only of letters.");
                }
            }
        }
        
        
        private void validatePassword()
        {
            if (string.IsNullOrEmpty(Password))
            {
                PasswordBorderColor = ConfirmPasswordBorderColor = Constants.INVALID_COLOR;
                displayError("Please fill in your password.");
            }
            else if (Password != ConfirmPassword)
            {
                PasswordBorderColor = ConfirmPasswordBorderColor = Constants.INVALID_COLOR;
                displayError("Passwords do not match.");
            }
            else
            {
                bool isValid = Regex.IsMatch(Password, @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]{8,}$");

                if (isValid)
                {
                    PasswordBorderColor = ConfirmPasswordBorderColor = Constants.VALID_COLOR;
                }
                else
                {
                    PasswordBorderColor = ConfirmPasswordBorderColor = Constants.INVALID_COLOR;
                    displayError("Please enter a password containing at least 8 characters, with at least one uppercase letter, one lowercase letter, one number, and one special character.");
                }
            }
        }
        
        private void validateUserName()
        {
            if (string.IsNullOrEmpty(UserName))
            {
                UserNameBorderColor = Constants.INVALID_COLOR;
                displayError("Please fill in your username.");
            }
            else
            {
                bool isValid = Regex.IsMatch(UserName, @"^[a-zA-Z0-9]+$");

                if (!isValid)
                {
                    UserNameBorderColor = Constants.INVALID_COLOR;
                    displayError("Please enter a valid username consisting only of letters and numbers.");
                }
                else
                {
                    UserNameBorderColor = Constants.VALID_COLOR;
                }
            }
        }

        private void validateTelephone()
        {
            if (string.IsNullOrEmpty(Telephone))
            {
                TelephoneBorderColor = Constants.INVALID_COLOR;
                displayError("Please fill in your telephone number.");
            }
            else
            {
                bool isValid = Regex.IsMatch(Telephone, @"^05\d{8}$");

                if (!isValid)
                {
                    TelephoneBorderColor = Constants.INVALID_COLOR;
                    displayError("Please enter a valid 10-digit telephone number starting with '05'.");
                }
                else
                {
                    TelephoneBorderColor = Constants.VALID_COLOR;
                }
            }
        }


        private void displayError(string message)
        {
            ErrorMessages.Add(message);
        }

        private async void onSignUpClicked()
        {
            string israeliPhoneNumber;
            bool successfulSMSSent;
            bool validationSuccessful;
            bool userExists;
            string errorMessage;

            ErrorMessages.Clear();

            validateName();
            validateUserName();
            validatePassword();
            validateTelephone();

            userExists = AzureHttpClient.Instance.CheckUserExists(UserName, Telephone, out errorMessage);

            if (userExists)
            {
                r_logger.LogDebug(errorMessage);
                displayError(errorMessage);
            }

            if (ErrorMessages.Count > 0)
            {
                string completeErrorMessage = string.Join(Environment.NewLine,
                    ErrorMessages.Select(message => $"- {message}"));
                await Application.Current.MainPage.DisplayAlert("Invalid sign up", completeErrorMessage, "OK");
            }
            else
            {
                israeliPhoneNumber = convertToIsraelPhoneNumber(Telephone);

                if (string.IsNullOrEmpty(VerificationCode))
                {
                    VerificationCode = generateVerificationCode();
                }

                successfulSMSSent =
                    AzureHttpClient.Instance.SendSMSVerificationCode(israeliPhoneNumber, VerificationCode);

                if (successfulSMSSent)
                {
                    validationSuccessful = await validateVerificationCodeWithUser();
                    if (validationSuccessful)
                    {
                        bool registered =
                            AzureHttpClient.Instance.RegisterUser(Name, UserName, Password, Telephone);
                        if (registered)
                        {
                            await Application.Current.MainPage.DisplayAlert("Registration Success",
                                "You have successfully registered to Notify.", "OK");
                            await Shell.Current.GoToAsync("///login");
                        }
                        else
                        {
                            r_logger.LogWarning("Failed to register user.");
                            displayError($"Failed to register user.");
                        }
                    }
                    else
                    {
                        r_logger.LogWarning("Failed to validate verification code.");
                        displayError($"Failed to validate verification code.");
                    }
                }
                else
                {
                    r_logger.LogWarning("Failed to send SMS message.");
                    displayError($"Failed to send SMS message.");
                }
            }
        }

        private async Task<bool> validateVerificationCodeWithUser()
        {
            string userEnteredCode;
            bool isValidationSuccessful = false;

            do
            {
                userEnteredCode = await Application.Current.MainPage.DisplayPromptAsync(
                    "Verify Your Phone Number", $"Please enter the verification code sent to {Telephone}",
                    maxLength: Constants.VERIFICATION_CODE_MAX_LENGTH);

                if (userEnteredCode != VerificationCode)
                {
                    bool tryAgain = await Application.Current.MainPage.DisplayAlert("Verification Error",
                        "The verification code you entered is invalid. Do you want to try again?", "Yes", "No");

                    if (!tryAgain)
                    {
                        isValidationSuccessful = false;
                        break;
                    }
                }
                else
                {
                    isValidationSuccessful = true;
                }
            } while (!isValidationSuccessful);

            return isValidationSuccessful;
        }

        private string convertToIsraelPhoneNumber(string phoneNumber) => $"+972{phoneNumber.Substring(1)}";
        
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
                int code = BitConverter.ToInt32(randomBytes, 0) & 0x7FFFFFFF;
                return (code % 1000000).ToString("D6"); 
            }
        }
    }
}