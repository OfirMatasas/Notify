using Notify.Core;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Notify.Views.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class FriendDetailsPage : ContentPage
    {
        
        public FriendDetailsPage(User selectedFriend)
        {
            InitializeComponent();
            BindingContext = new ViewModels.FriendDetailsPageViewModel(selectedFriend);
        }
    }
}
