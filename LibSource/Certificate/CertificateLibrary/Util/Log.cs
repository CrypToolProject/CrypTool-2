using System;
using System.Diagnostics;
using System.Reflection;
using System.Text;
using System.Threading;

namespace CrypTool.Util.Logging
{
    /// <summary>
    /// This enum provides a collection of possible log-levels.
    /// </summary>
    public enum LoggingSeverityEnum
    {
        /// <summary>
        /// Fatal should be used for error cases, from which no recovery is possible
        /// </summary>
        Fatal,

        /// <summary>
        /// Error should be used for any kind of (possible recovereable) errors.
        /// </summary>
        Error,

        /// <summary>
        /// Warning should be used for situations where an error might occur
        /// </summary>
        Warn,

        /// <summary>
        /// Info should be used for overview information, e.g. this could be shonw in a status bar
        /// </summary>
        Info,

        /// <summary>
        /// Debug can be used for anything left Debug usually produces a lot of output and should be deactivated in release versions.
        /// </summary>
        Debug
    }

    public delegate void LogHook(string str);

    public interface IExternalInfoProvider
    {
        string GetInfo();
    }

    public static class Log
    {
        public const string THREAD_ID_PREFIX = "Thread ID:";
        public const string INSTANCE_ID_PREFIX = "ID:";
        private const string SPLITTER = "; ";
        private const int STACK_FRAME_COUNT = 2;

        public static char Splitter => SPLITTER[0];

        public static bool Enabled { get; set; }
        public static LoggingSeverityEnum Severity { get; set; }

        public static bool AddAppDomain { get; set; }
        public static bool AddInstanceID { get; set; }
        public static bool AddThreadNumber { get; set; }
        public static bool AddAssemblyName { get; set; }
        public static bool AddClassName { get; set; }
        public static bool AddMethodName { get; set; }

        public static string CustomTag { get; set; }

        public static IExternalInfoProvider ExternalInfoProvider { get; set; }

        static Log()
        {
            AddAppDomain = false;
            AddInstanceID = true;
            AddThreadNumber = false;
            AddAssemblyName = true;
            AddClassName = true;
            AddMethodName = true;
        }

        #region Debug Logging Methods
        public static void Debug(string message)
        {
            //internalLog.Debug(GetPrefix() + message);
            Console.WriteLine(DateTime.Now + " - DEBUG " + GetPrefix() + message);
        }
        #endregion

        #region Error Logging Methods
        public static void Error(string message)
        {
            //internalLog.Error(GetPrefix() + message);
            Console.WriteLine(DateTime.Now + " - ERROR " + GetPrefix() + message);
        }

        public static void Error(Exception ex)
        {
            //internalLog.Error(GetPrefix(), ex);
            Console.WriteLine(DateTime.Now + " - ERROR " + GetPrefix() + ex.Message + " - " + ex.StackTrace);
        }

        public static void Error(string message, Exception ex)
        {
            //internalLog.Error(GetPrefix() + message, ex);
            Console.WriteLine(DateTime.Now + " - ERROR " + GetPrefix() + message + " - " + ex.Message + " - " + ex.StackTrace);
        }

        #endregion


        #region Info Logging Methods
        public static void Info(string message)
        {
            //internalLog.Info(GetPrefix() + message);
            Console.WriteLine(DateTime.Now + " - INFO " + GetPrefix() + message);
        }
        #endregion

        #region Warn Logging Methods
        public static void Warn(string message)
        {
            //internalLog.Warn(GetPrefix() + message);
            Console.WriteLine(DateTime.Now + " - WARN " + GetPrefix() + message);
        }

        public static void Warn(string message, Exception ex)
        {
            //internalLog.Warn(GetPrefix() + message, ex);
            Console.WriteLine(DateTime.Now + " - WARN " + GetPrefix() + ex.Message + " - " + ex.StackTrace);
        }

        public static void WarnFormat(string messageFormat, params object[] args)
        {
            //internalLog.WarnFormat(GetPrefix() + messageFormat, args);
            Console.WriteLine(DateTime.Now + " - WARN " + GetPrefix() + messageFormat);
        }
        #endregion

        private static string GetPrefix()
        {
            if (Thread.CurrentContext.ContextID == 0)
            {
            }

            StackTrace stackTrace = new StackTrace();
            MethodBase b = stackTrace.GetFrame(STACK_FRAME_COUNT).GetMethod();

            string methodName = b.Name;
            string className = b.DeclaringType.Name;

            if ((className.Contains("<")) && (className.Contains(">")))
            {
                methodName = className.Substring(className.IndexOf('<') + 1).Substring(0, className.IndexOf('>') - 1);
                className = b.DeclaringType.DeclaringType.Name;
            }

            bool hasPrev = false;
            StringBuilder sb = new StringBuilder();
            sb.Append("[");

            if (ExternalInfoProvider != null)
            {
                sb.Append(ExternalInfoProvider.GetInfo());
                hasPrev = true;
            }
            if (AddAppDomain)
            {
                sb.Append(string.Format("{0}{1}", hasPrev ? SPLITTER : string.Empty, AppDomain.CurrentDomain.FriendlyName));
                hasPrev = true;
            }

            if (AddInstanceID)
            {
                sb.Append(string.Format("{0}{1} {2}", hasPrev ? SPLITTER : string.Empty, INSTANCE_ID_PREFIX, Thread.CurrentContext.ContextID.ToString()));
                hasPrev = true;
            }


            if (AddThreadNumber)
            {
                sb.Append(string.Format("{0}{1} {2}", hasPrev ? SPLITTER : string.Empty, THREAD_ID_PREFIX, Thread.CurrentThread.ManagedThreadId));
                hasPrev = true;
            }

            if (AddAssemblyName)
            {
                sb.Append(string.Format("{0}{1}", hasPrev ? SPLITTER : string.Empty, new AssemblyName(b.DeclaringType.Assembly.FullName).Name));
                hasPrev = true;
            }


            if (AddClassName)
            {
                sb.Append(string.Format("{0}{1}", hasPrev ? SPLITTER : string.Empty, className));
                hasPrev = true;
            }

            if (AddMethodName)
            {
                sb.Append(string.Format("{0}{1}", AddClassName ? "." : hasPrev ? SPLITTER : string.Empty, methodName));
                hasPrev = true;
            }

            sb.Append("] ").AppendLine();
            return sb.ToString();
        }
    }
}
