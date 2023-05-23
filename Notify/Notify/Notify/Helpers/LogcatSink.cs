using Android.Util;
using Serilog.Core;
using Serilog.Events;

namespace Notify.Helpers
{
    public class LogcatSink : ILogEventSink
    {
        public void Emit(LogEvent logEvent)
        {
            LogPriority logLevel = ConvertToLogLevel(logEvent.Level);
            string tag = "notify_logger";
            string message = logEvent.RenderMessage();

            Log.WriteLine(logLevel, tag, message);
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