using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Notify.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class NotificationSettingsPage : ContentPage
    {
        public NotificationSettingsPage()
        {
            InitializeComponent();
            BindingContext = new ViewModels.NotificationSettingsPageViewModel();
        }
    }
}
