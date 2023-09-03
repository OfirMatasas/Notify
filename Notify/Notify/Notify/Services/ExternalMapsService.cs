using System;

namespace Notify.Services
{
    public abstract class ExternalMapsService
    {
        protected static ExternalMapsService m_Instance;
        private static readonly LoggerService r_Logger = LoggerService.Instance;
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
                            r_Logger.LogError("Failed to initialize ExternalMapsService.");
                        }
                    }
                }
                return m_Instance;
            }
        }

        public static void Initialize(ExternalMapsService instance)
        {
            if (m_Instance != null)
            {
                throw new InvalidOperationException("ExternalMapsService is already initialized.");
            }
            m_Instance = instance;
        }

        public abstract void OpenExternalMap(string notificationType);
    }
}