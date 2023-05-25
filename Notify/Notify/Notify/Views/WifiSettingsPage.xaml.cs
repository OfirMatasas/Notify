using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Notify.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class WifiSettingsPage : ContentPage
    {
        public WifiSettingsPage()
        {
            InitializeComponent();
            BindingContext = new ViewModels.WifiSettingsPageViewModel();
        }
    }
}
