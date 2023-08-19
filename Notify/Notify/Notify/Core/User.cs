using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Notify.Core
{
    public sealed class User : INotifyPropertyChanged
    {
        #region Members

        private string m_Name;
        private string m_UserName;
        private string m_Telephone;
        private bool m_IsSelected;
        private string m_ProfilePicture;

        #endregion

        #region Constructor

        public User(string name, string username, string telephone)
        {
            Name = name;
            UserName = username;
            Telephone = telephone;
            IsSelected = false;
            ProfilePicture = string.Empty;
        }

        #endregion

        #region Properties

        public string Name
        {
            get => m_Name;
            set
            {
                if (m_Name != value)
                {
                    m_Name = value;
                    OnPropertyChanged();
                }
            }
        }

        public string UserName
        {
            get => m_UserName;
            set
            {
                if (m_UserName != value)
                {
                    m_UserName = value;
                    OnPropertyChanged();
                }
            }
        }

        public string Telephone
        {
            get => m_Telephone;
            set
            {
                if (m_Telephone != value)
                {
                    m_Telephone = value;
                    OnPropertyChanged();
                }
            }
        }

        public bool IsSelected
        {
            get => m_IsSelected;
            set
            {
                if (m_IsSelected != value)
                {
                    m_IsSelected = value;
                    OnPropertyChanged();
                }
            }
        }

        public string ProfilePicture
        {
            get => m_ProfilePicture;
            set
            {
                if (m_ProfilePicture != value)
                {
                    m_ProfilePicture = value;
                    OnPropertyChanged();
                }
            }
        }

        #endregion

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}