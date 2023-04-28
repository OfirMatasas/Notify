using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Notify.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class LocationSettingsPage : ContentPage
    {
        public LocationSettingsPage()
        {
            InitializeComponent();
            BindingContext = new ViewModels.LocationSettingsPageViewModel();
        }
    }
}
