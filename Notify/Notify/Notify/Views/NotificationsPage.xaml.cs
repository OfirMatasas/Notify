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
        
        protected override void OnAppearing()
        {
            base.OnAppearing();
            (BindingContext as NotificationsPageViewModel)?.OnNotificationsRefreshClicked();
        }

    }
}
