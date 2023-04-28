using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Notify.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class AccountSettingsPage : ContentPage
    {
        public AccountSettingsPage()
        {
            InitializeComponent();
            BindingContext = new ViewModels.AccountSettingsPageViewModel();
        }
    }
}