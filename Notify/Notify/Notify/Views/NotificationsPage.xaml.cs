using System.Diagnostics;
using Notify.ViewModels;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Notify.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class NotificationsPage : ContentPage
    {
        public NotificationsPage() : this(NotificationsPageViewModel.Instance)
        {
        }

        public NotificationsPage(NotificationsPageViewModel viewModel)
        {
            InitializeComponent();
            BindingContext = viewModel ?? NotificationsPageViewModel.Instance;
            Debug.WriteLine($"ViewModel in Page: {BindingContext.GetHashCode()}");
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            (BindingContext as NotificationsPageViewModel)?.OnNotificationsRefreshClicked();
        }
        
        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            (BindingContext as NotificationsPageViewModel)?.ResetExpandedNotification();
        }
    }
}