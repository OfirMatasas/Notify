using System.ComponentModel;
using System.Diagnostics;
using Xamarin.Forms;
using System.Text.RegularExpressions;


namespace Notify.ViewModels
{
    public class RegistrationPageViewModel
    {
        public string Name { get; }
        public string UserName { get; }
        public string Password { get; }
        public string ConfirmPassword { get; }
        public string RegisterCommand { get; }
        
        private string m_Telephone;
        public string Telephone
        {
            get => m_Telephone;
            set
            {
                m_Telephone = value;
                OnPropertyChanged(nameof(Telephone));
                ValidateTelephone();
            }
        }
        
        private void ValidateTelephone()
        {
            bool isValid = !string.IsNullOrEmpty(Telephone) && Regex.IsMatch(Telephone, @"^\d{10}$");

            if (!isValid)
            {
                Debug.WriteLine("Please enter a valid 10-digit telephone number.");
            }
        }
        
        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }


    }
}