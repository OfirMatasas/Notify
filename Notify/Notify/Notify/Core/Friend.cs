using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Xamarin.Forms;

namespace Notify.Core
{
    public sealed class Friend : INotifyPropertyChanged
    {
        #region Members

        private string m_Name;
        private string m_UserName;
        private string m_Telephone;
        private bool m_IsSelected;
        private string m_ProfileImage;

        #endregion

        #region Constructor

        public Friend(string name, string userName, string telephone)
        {
            Name = name;
            UserName = userName;
            Telephone = telephone;
            IsSelected = false;
            ProfileImage = "profile.png"; // Set a default image
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

        public string ProfileImage
        {
            get => m_ProfileImage;
            set
            {
                if (m_ProfileImage != value)
                {
                    m_ProfileImage = value;
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

        public ImageSource GetProfileImageSource()
        {
            if (string.IsNullOrEmpty(ProfileImage))
            {
                return ImageSource.FromFile("profile.png"); // Return the default image if no image is set
            }
            else if (ProfileImage.StartsWith("http"))
            {
                return ImageSource.FromUri(new Uri(ProfileImage)); // Load the image from a URL if it's a remote image
            }
            else
            {
                return ImageSource.FromFile(ProfileImage); // Load the image from a local file if it's a file path
            }
        }
    }
}