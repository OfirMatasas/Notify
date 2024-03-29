using System.Diagnostics;
using Serilog;

namespace Notify.Services
{
    public abstract class LoggerService
    {
        protected static LoggerService m_Instance;
        private static readonly object r_Lock = new object();

        public static LoggerService Instance
        {
            get
            {
                if (m_Instance == null)
                {
                    lock (r_Lock)
                    {
                        if (m_Instance == null)
                        {
                            Debug.Write("ERROR to initialize logger.");
                        }
                    }
                }
                
                return m_Instance;
            }
        }

        public abstract void InitializeLogger();
        
        public void LogVerbose(string message)
        {
            Log.Verbose(message);
        }
        
        public void LogDebug(string message)
        {
            Log.Debug(message);
        }
        
        public void LogInformation(string message)
        {
            Log.Information(message);
        }
        
        public void LogWarning(string message)
        {
            Log.Warning(message);
        }

        public void LogError(string message)
        {
            Log.Error(message);
        }
        
        public void LogFatal(string message)
        {
            Log.Fatal(message);
        }
    }
}
