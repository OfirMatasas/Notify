using Notify.ViewModels.Popups;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Notify.Views.Popups
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class SeasonPopupPage
    {
        public SeasonPopupPage()
        {
            InitializeComponent();

            MessagingCenter.Subscribe<SeasonPopupPageViewModel, string>(this.BindingContext, "Dismiss", (sender, args) =>
            {
                Dismiss(args);
            });
        }
    }
}