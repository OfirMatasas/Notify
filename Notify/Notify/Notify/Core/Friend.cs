using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Notify.Core
{
    public sealed class Friend: INotifyPropertyChanged
    {
        private string m_Name;
        private bool m_IsSelected;

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

        public Friend(string name)
        {
            Name = name;
            IsSelected = false;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
