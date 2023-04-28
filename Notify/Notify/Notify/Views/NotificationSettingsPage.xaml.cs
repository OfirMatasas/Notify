﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Notify.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class NotificationSettingsPage : ContentPage
    {
        public NotificationSettingsPage()
        {
            InitializeComponent();
            BindingContext = new ViewModels.NotificationSettingsPageViewModel();
        }
    }
}