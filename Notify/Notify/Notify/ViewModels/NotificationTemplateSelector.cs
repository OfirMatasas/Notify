using Notify.Core;
using Xamarin.Forms;

namespace Notify.ViewModels
{
    public class NotificationTemplateSelector : DataTemplateSelector
    {
        public DataTemplate RegularTemplate { get; set; }
        public DataTemplate PendingTemplate { get; set; }

        protected override DataTemplate OnSelectTemplate(object item, BindableObject container)
        {
            if (item is Notification notification)
            {
                return notification.IsPending ? PendingTemplate : RegularTemplate;
            }
            return null;
        }
    }
}