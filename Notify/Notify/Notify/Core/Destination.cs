namespace Notify.Core
{
    public class Destination
    {
        private string m_Name;
        private Location m_Location;
        private string m_SSID;
        private string m_Bluetooth;
        
        public Destination(string name)
        {
            Name = name;
        }
        
        public string Name
        {
            get => m_Name;
            set => m_Name = value;
        }
        
        public Location Location
        {
            get => m_Location;
            set => m_Location = value;
        }
        
        public string SSID
        {
            get => m_SSID;
            set => m_SSID = value;
        }
        
        public string Bluetooth
        {
            get => m_Bluetooth;
            set => m_Bluetooth = value;
        }
    }
}
