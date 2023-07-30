using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Notify.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ProfilePage : ContentPage
    {
        public ProfilePage()
        {
            InitializeComponent();
            BindingContext = new ViewModels.ProfilePageViewModel();
        }
        
        private void CarouselView_OnPositionChanged(object sender, PositionChangedEventArgs e)
        {
           
        }
    }
}
