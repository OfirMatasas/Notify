using Serilog;

namespace Notify.Helpers
{
    public class IOSLogger : LoggerService
    {
        private static readonly object r_Lock = new object();

        private IOSLogger()
        {
            InitializeLogger();
        }
        
        public static LoggerService Instance
        {
            get
            {
                lock (r_Lock)
                {
                    if (m_Instance == null)
                    {
                        m_Instance = new IOSLogger();
                    }
                    
                    return m_Instance;
                }
            }
        }
        
        public override void InitializeLogger()
        {
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .WriteTo.File("/data/data/com.notify.notify/files/logsFile.txt")
                .WriteTo.Debug(outputTemplate: "[{Timestamp:dd-MM-yyy HH:mm:ss}] [{Level:u3}] {Message:lj}{NewLine}{Exception}")
                .CreateLogger();
        }
    }
}