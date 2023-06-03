using Notify.Services;

namespace Notify.iOS.Services
{
    public class iOSDeviceService : IDeviceService
    {
        public string GetDeviceId()
        {
            return UIKit.UIDevice.CurrentDevice.IdentifierForVendor.AsString();
        }
    }
}