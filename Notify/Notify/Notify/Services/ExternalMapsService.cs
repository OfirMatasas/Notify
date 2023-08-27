using System.Diagnostics;

namespace Notify.Services
{
    public abstract class ExternalMapsService
    {
        protected static ExternalMapsService m_Instance;
        private static readonly object r_Lock = new object();

        public static ExternalMapsService Instance
        {
            get
            {
                if (m_Instance == null)
                {
                    lock (r_Lock)
                    {
                        if (m_Instance == null)
                        {
                            Debug.Write("ERROR to initialize ExternalMapsService.");
                        }
                    }
                }
                
                return m_Instance;
            }
        }
        public abstract void OpenExternalMap();
    }
}