using Android.Util;
using Serilog;
using Serilog.Core;
using Serilog.Events;
using Andorid_Log = Android.Util.Log;
using Serilog_Log = Serilog.Log;


namespace Notify.Helpers
{
    public class AndroidLogger : LoggerService
    {
        private static readonly object r_Lock = new object();

        private AndroidLogger()
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
                        m_Instance = new AndroidLogger();
                    }
                    
                    return m_Instance;
                }
            }
        }
        
        public override void InitializeLogger()
        {
            Serilog_Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Information()
                .WriteTo.Sink(new LogcatSink())
                .WriteTo.File("/data/data/com.notify.notify/files/logsFile.txt")
                .WriteTo.Debug(outputTemplate: "[{Timestamp:dd-MM-yyy HH:mm:ss}] [{Level:u3}] {Message:lj}{NewLine}{Exception}")
                .CreateLogger();
        }
    }
    
    public class LogcatSink : ILogEventSink
    {
        public void Emit(LogEvent logEvent)
        {
            LogPriority logLevel = ConvertToLogLevel(logEvent.Level);
            string tag = "notify_logger";
            string message = logEvent.RenderMessage();

            Andorid_Log.WriteLine(logLevel, tag, message);
        }

        private LogPriority ConvertToLogLevel(LogEventLevel logEventLevel)
        {
            switch (logEventLevel)
            {
                case LogEventLevel.Verbose:
                    return LogPriority.Verbose;
                case LogEventLevel.Debug:
                    return LogPriority.Debug;
                case LogEventLevel.Information:
                    return LogPriority.Info;
                case LogEventLevel.Warning:
                    return LogPriority.Warn;
                case LogEventLevel.Error:
                    return LogPriority.Error;
                case LogEventLevel.Fatal:
                    return LogPriority.Assert;
                default:
                    return LogPriority.Debug;
            }
        }
    }
}