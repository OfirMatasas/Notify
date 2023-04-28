using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows.Input;
using Android.Icu.Text;
using Notify.Services.Ergast;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace Notify.ViewModels
{
    public class ProfilePageViewModel
    { 
        public Command GoSettingsPageCommand { get; set; }

        public ProfilePageViewModel()
        {
            GoSettingsPageCommand = new Command(onSettingsButtonClicked);
        }

        private async void onSettingsButtonClicked()
        {
            await Shell.Current.GoToAsync("///settings");
        }
    }
}
