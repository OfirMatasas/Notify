using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Xamarin.CommunityToolkit.UI.Views;
using Xamarin.Essentials;

namespace Notify.ViewModels
{
    public class BaseViewModel : INotifyPropertyChanged
    {
        #region Private & Protected
        
        private bool m_IsBusy;
        private string m_Title = string.Empty;

        #endregion

        #region Properties

        public bool IsBusy
        {
            get => m_IsBusy;
            set => SetProperty(ref m_IsBusy, value);
        }

        public string Title
        {
            get => m_Title;
            set => SetProperty(ref m_Title, value);
        }

        protected bool SetProperty<T>(ref T backingStore, T value,
            [CallerMemberName] string propertyName = "",
            Action onChanged = null)
        {
            if (EqualityComparer<T>.Default.Equals(backingStore, value))
                return false;

            backingStore = value;
            onChanged?.Invoke();
            OnPropertyChanged(propertyName);

            return true;
        }

        #region INotifyPropertyChanged
        protected void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChangedEventHandler changed = PropertyChanged;

            changed?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        public event PropertyChangedEventHandler PropertyChanged;
        public LayoutState MainState { get; set; }
        public bool HasNoInternetConnection { get; set; }

        #endregion

        #region Constructor

        public BaseViewModel()
        {
            Connectivity.ConnectivityChanged += ConnectivityChanged;
            HasNoInternetConnection = !Connectivity.NetworkAccess.Equals(NetworkAccess.Internet);
        }

        #endregion

        #region Internet Connection

        private void ConnectivityChanged(object sender, ConnectivityChangedEventArgs e)
        {
            HasNoInternetConnection = !e.NetworkAccess.Equals(NetworkAccess.Internet);
        }

        #endregion
    }
}
