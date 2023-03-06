﻿using Notify.Views.Templates;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Notify.Views.Templates
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class InformationLapRecordTemplate : StackLayout
    {
        public InformationLapRecordTemplate()
        {
            InitializeComponent();
        }

        public static readonly BindableProperty LabelProperty = BindableProperty.Create(nameof(Label), typeof(string), typeof(InformationTemplate), string.Empty);
        public static readonly BindableProperty ValueProperty = BindableProperty.Create(nameof(Value), typeof(string), typeof(InformationTemplate), string.Empty);
        public static readonly BindableProperty SubLabelProperty = BindableProperty.Create(nameof(SubLabel), typeof(string), typeof(InformationTemplate), string.Empty);

        public string Label
        {
            get => (string)GetValue(LabelProperty);
            set => SetValue(LabelProperty, value);
        }

        public string Value
        {
            get => (string)GetValue(ValueProperty);
            set => SetValue(LabelProperty, value);
        }

        public string SubLabel
        {
            get => (string)GetValue(SubLabelProperty);
            set => SetValue(SubLabelProperty, value);
        }
    }
}