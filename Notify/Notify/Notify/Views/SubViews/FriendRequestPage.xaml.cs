using System;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Notify.Views.SubViews
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class FriendRequestPage : ContentPage
    {
        private ViewModels.FriendRequestPageViewModel m_FriendRequestPageViewModel;
        
        public FriendRequestPage()
        {
            InitializeComponent();
            m_FriendRequestPageViewModel = new ViewModels.FriendRequestPageViewModel();
            BindingContext = m_FriendRequestPageViewModel;
        }

        private void SearchEntry_OnTextChanged(object sender, EventArgs e)
        {
            m_FriendRequestPageViewModel.SearchTextChangedCommand.Execute(null);
        }
    }
}
