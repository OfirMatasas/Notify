using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Notify.Views.Fragments
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class LoadingView : StackLayout
    {
        public LoadingView()
        {
            InitializeComponent();
        }
    }
}