using System.Diagnostics;
using Notify.ViewModels;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Notify.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class NotificationsPage : ContentPage
    {
        public NotificationsPage()
        {
            InitializeComponent();
            BindingContext = new NotificationsPageViewModel();
        }

        private void StartDatePicker_OnDateSelected(object sender, DateChangedEventArgs e)
        {
            Debug.WriteLine("Hello");
        }

        private void EndDatePicker_OnDateSelected(object sender, DateChangedEventArgs e)
        {
            Debug.WriteLine("Hello");
        }
    }
}
