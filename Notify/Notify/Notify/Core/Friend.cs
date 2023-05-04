using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Notify.Core
{
    public sealed class Friend : INotifyPropertyChanged
    {
        #region Members
        
        private string m_Name;
        private string m_UserName;
        private string m_Telephone;
        private bool m_IsSelected;
        
        #endregion

        #region Constructor
        
        public Friend(string name, string userName, string telephone)
        {
            Name = name;
            UserName = userName;
            Telephone = telephone;
            IsSelected = false;
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

        #endregion

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
