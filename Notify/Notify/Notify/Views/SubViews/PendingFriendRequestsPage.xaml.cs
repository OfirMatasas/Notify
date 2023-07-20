using Notify.ViewModels;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Notify.Views.SubViews
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class PendingFriendRequestsPage : ContentPage
    {
        public PendingFriendRequestsPage()
        {
            InitializeComponent();
            BindingContext = new PendingFriendRequestsPageViewModel();
        }
    }
}