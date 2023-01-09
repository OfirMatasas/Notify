using System;
using System.Collections.Generic;
using Xamarin.Forms;

namespace Notify.Forms
{	
	public partial class FormProfile : ContentPage
	{	
		public FormProfile()
		{
			InitializeComponent();
            Title = "Profile";
        }

        void ButtonLogout_Clicked(Object i_Sender, EventArgs i_Args)
        {
            App.IsUserLoggedIn = false;
            Navigation.InsertPageBefore(new FormLogin(), this.Parent as Page);
            Navigation.PopAsync();
        }
    }
}
