using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;
using Notify.Forms;

namespace Notify
{
    public partial class MainPage : TabbedPage
    {
        private readonly Page r_FormProfile;
        private readonly Page r_FormFriends;
        private readonly Page r_FormGroups;
        private readonly Page r_FormNotifications;

        public MainPage()
        {
            InitializeComponent();

            Children.Add(r_FormProfile = new FormProfile());
            Children.Add(r_FormFriends = new FormFriends());
            Children.Add(r_FormGroups = new FormGroups());
            Children.Add(r_FormNotifications = new FormNotifications());

            this.CurrentPageChanged += NavigationBar_Clicked;

            this.CurrentPage = r_FormNotifications;
        }

        private void NavigationBar_Clicked(object sender, EventArgs e)
        {
            Page selectedPage = null;

            switch (this.CurrentPage.Title)
            {
                case "Profile":
                    selectedPage = r_FormProfile;
                    break;
                case "Friends":
                    selectedPage = r_FormFriends;
                    break;
                case "Groups":
                    selectedPage = r_FormGroups;
                    break;
                case "Notifications":
                    selectedPage = r_FormNotifications;
                    break;
            }

            this.CurrentPage = selectedPage;
        }
    }
}
