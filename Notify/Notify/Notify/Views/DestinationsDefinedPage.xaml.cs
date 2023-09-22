using Xamarin.Forms;
using Xamarin.Forms.Xaml;
namespace Notify.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class DestinationsDefinedPage : ContentPage
    {
        public DestinationsDefinedPage()
        {
            InitializeComponent();
            BindingContext = new ViewModels.DestinationsDefinedViewModel();
        }
    }
}