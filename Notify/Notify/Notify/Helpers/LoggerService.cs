using Serilog;
using System;

namespace Notify.Helpers
{
    public sealed class LoggerService
    {
        private static LoggerService m_instance = null;
        private static readonly object r_lock = new object();

        private LoggerService()
        {
            InitializeLogger();
        }

        public static LoggerService Instance
        {
            get
            {
                lock (r_lock)
                {
                    if (m_instance == null)
                    {
                        m_instance = new LoggerService();
                    }
                    
                    return m_instance;
                }
            }
        }

        private void InitializeLogger()
        {
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .WriteTo.Sink(new LogcatSink())
                //.WriteTo.File("/data/data/com.notify.notify/files/log.txt")
                //.WriteTo.Debug(outputTemplate: "{Timestamp:dd-MM-yyy HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception}")
                .CreateLogger();
        }
        
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