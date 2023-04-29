using System;
using Rg.Plugins.Popup.Pages;
using Rg.Plugins.Popup.Services;

public partial class VerificationPopupViewModel : PopupPage
{
    private string m_PhoneNumber;
    private string m_VerificationCode;

    public VerificationPopupViewModel(string phoneNumber, string verificationCode)
    {
        InitializeComponent();

        m_PhoneNumber = phoneNumber;
        m_VerificationCode = verificationCode;

        PhoneNumberLabel.Text = phoneNumber;
        VerificationCodeEntry.Text = verificationCode;
    }

    private async void OnRegisterClicked(object sender, EventArgs e)
    {
        string enteredCode = VerificationCodeEntry.Text;

        if (enteredCode == m_VerificationCode)
        {
            // Code is valid, complete registration
            await DisplayAlert("Registration Success", "You have successfully registered to the system.", "OK");
            await PopupNavigation.Instance.PopAsync();
        }
        else
        {
            // Code is invalid, display error message
            await DisplayAlert("Verification Error", "The verification code you entered is invalid. Please try again.", "OK");
        }
    }
}