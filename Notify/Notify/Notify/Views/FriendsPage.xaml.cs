using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Notify.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class FriendsPage : ContentPage
    {
        public FriendsPage()
        {
            InitializeComponent();
            BindingContext = new ViewModels.FriendsPageViewModel();
        }
    }
}
