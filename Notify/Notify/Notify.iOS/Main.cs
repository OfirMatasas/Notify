using UIKit;

namespace Notify.iOS
{
    public class Application
    {
        static void Main(string[] args)
        {
            Plugin.MaterialDesignControls.iOS.Renderer.Init();
            UIApplication.Main(args, null, typeof(AppDelegate));
        }
    }
}
