using Python.Runtime;
using System;
using System.Globalization;
using System.IO;
using System.Reflection;

namespace CadoNFS.Processing
{
    public class LoggingInterception
    {
        private const string loggingInterceptResourceName = "CadoNFS.Python.logging-intercept.py";

        public delegate void LogDelegate(int pid, DateTime time, string level, string name, string message);
        public LogDelegate LogEvent;

        public LoggingInterception()
        {
        }

        public void Attach()
        {
            dynamic loggingIntercept = PythonEngine.ModuleFromString("logging-intercept", GetLoggingInterceptModuleCode());
            loggingIntercept.registerLogger(new Action<string>(HandleLogMessage));
        }

        private void HandleLogMessage(string message)
        {
            if (!string.IsNullOrWhiteSpace(message))
            {
                try
                {
                    var parts = message.Split(new[] { '|' }, 5);
                    var timestamp = DateTimeOffset.FromUnixTimeSeconds((long)double.Parse(parts[1], CultureInfo.InvariantCulture)).LocalDateTime;
                    //var timestamp = new DateTime((long)(TimeSpan.TicksPerSecond * double.Parse(parts[1], CultureInfo.InvariantCulture)));
                    //var timestamp = DateTime.ParseExact(parts[1], "yyyy-MM-dd HH:mm:ss,ff", null);
                    LogEvent?.Invoke(int.Parse(parts[0]), timestamp, parts[2], parts[3], parts[4]);
                }
                catch
                {
                    LogEvent?.Invoke(0, DateTime.Now, "Fatal", "Logger", $"Invalid log format in message: {message}");
                }
            }
        }

        private static string GetLoggingInterceptModuleCode()
        {
            var assembly = Assembly.GetExecutingAssembly();
            using (var stream = assembly.GetManifestResourceStream(loggingInterceptResourceName))
            using (var reader = new StreamReader(stream))
            {
                return reader.ReadToEnd();
            }
        }
    }
}
