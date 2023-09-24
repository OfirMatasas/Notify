﻿using Xamarin.Forms;
using Xamarin.Forms.Xaml;
namespace Notify.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class DefinedDestinationsPage : ContentPage
    {
        public DefinedDestinationsPage()
        {
            InitializeComponent();
            BindingContext = new ViewModels.DefinedDestinationsViewModel();
        }
    }
}