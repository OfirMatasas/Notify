using Notify.Models;
using Notify.Views.Views;
using Xamarin.Forms;

namespace Notify.Helpers.TemplateSelectors
{
    internal class HomeScheduleTemplateSelector : DataTemplateSelector
    {
        public DataTemplate HomeScheduleTemplateView { get; set; }
        public DataTemplate HomeScheduleMoreTemplateView { get; set; }

        public HomeScheduleTemplateSelector()
        {
            HomeScheduleTemplateView = new DataTemplate(typeof(HomeScheduleView));
            HomeScheduleMoreTemplateView = new DataTemplate(typeof(HomeScheduleMoreView));
        }

        protected override DataTemplate OnSelectTemplate(object item, BindableObject container)
        {
            if (item.GetType() == typeof(RaceEventModel))
            {
                var resultsItem = item as RaceEventModel;
                if (resultsItem.RaceName == null)
                {
                    return HomeScheduleMoreTemplateView;
                }
                else
                {
                    return HomeScheduleTemplateView;
                }
            }
            return HomeScheduleTemplateView;
        }
    }
}
