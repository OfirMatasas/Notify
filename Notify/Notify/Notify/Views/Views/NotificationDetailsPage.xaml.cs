using Notify.Core;
using Notify.ViewModels;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Notify.Views.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class NotificationDetailsPage : ContentPage
    {
        public NotificationDetailsPage(Notification selectedNotification)
        {
            InitializeComponent();
            BindingContext = new NotificationDetailsPageViewModel(selectedNotification);
        }
    }
}
