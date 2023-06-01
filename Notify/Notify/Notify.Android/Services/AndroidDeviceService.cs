using Android.Provider;
using Notify.Droid.Services;
using Notify.Services;
using Xamarin.Forms;

[assembly: Dependency(typeof(AndroidDeviceService))]
namespace Notify.Droid.Services
{
    public class AndroidDeviceService : IDeviceService
    {
        public string GetDeviceId()
        {
            return Settings.Secure.GetString(Android.App.Application.Context.ContentResolver, Settings.Secure.AndroidId);
        }
    }
}