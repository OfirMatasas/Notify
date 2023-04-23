using System.Linq;
using Notify.ViewModels;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Notify.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class NotificationCreationPage : ContentPage
    {
        public NotificationCreationPage()
        {
            InitializeComponent();
            BindingContext = new ViewModels.NotificationCreationViewModel();
        }

        private void SelectableItemsView_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var removedItems = e.PreviousSelection.Except(e.CurrentSelection);
            var addedItems = e.CurrentSelection.Except(e.PreviousSelection);

            removedItems.ToList().ForEach(item => ((NotificationCreationViewModel.Friend)item).IsSelected = false);
            addedItems.ToList().ForEach(item => ((NotificationCreationViewModel.Friend)item).IsSelected = true);
        }
    }
}
