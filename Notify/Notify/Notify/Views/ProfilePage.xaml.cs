using System;
using Notify.ViewModels;
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
            BindingContext = new ProfilePageViewModel();
            ((ProfilePageViewModel)BindingContext).LocationButtonCommand.CanExecuteChanged += (s, e) => profileCarouselView.IsVisible = true;
            ((ProfilePageViewModel)BindingContext).BlueToothButtonCommand.CanExecuteChanged += (s, e) => profileCarouselView.IsVisible = true;
            ((ProfilePageViewModel)BindingContext).WifiButtonCommand.CanExecuteChanged += (s, e) => profileCarouselView.IsVisible = true;
        }

    }
}
