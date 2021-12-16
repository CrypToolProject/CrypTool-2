/*
   Copyright 2018 Nils Kopal <Nils.Kopal<AT>Uni-Kassel.de>

   Licensed under the Apache License, Version 2.0 (the "License");
   you may not use this file except in compliance with the License.
   You may obtain a copy of the License at

       http://www.apache.org/licenses/LICENSE-2.0

   Unless required by applicable law or agreed to in writing, software
   distributed under the License is distributed on an "AS IS" BASIS,
   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
   See the License for the specific language governing permissions and
   limitations under the License.
*/
using System;

namespace common
{
    /// <summary>
    /// Log types (debug, info, warning, and error).
    /// debug: log for developer.
    /// info: general info log.
    /// warning: something went "wrong"; but still in a state we can handle.
    /// error: something "really bad" happened... an error
    /// </summary>
    public enum Logtype
    {
        Debug = 0,
        Info = 1,
        Warning = 2,
        Error = 3
    }



    public class Logger
    {
        private static readonly string logfilePath = "";
        private static string filename;

        public event EventHandler<LogEventArgs> LoggOccured;

        private static readonly Logger Instance = new Logger();
        private static Logtype Loglevel = Logtype.Info;

        /// <summary>
        /// Singleton, thus private constructor
        /// </summary>
        public Logger()
        {
            //add file handling for logfile
        }

        /// <summary>
        /// Returns the global logger instance
        /// </summary>
        /// <returns></returns>
        public static Logger GetLogger()
        {
            return Instance;
        }

        /// <summary>
        /// Set minimum loglevel (default = info)
        /// means, that only messages with level info or higher will be logged
        /// </summary>
        /// <param name="loglevel"></param>
        public static void SetLogLevel(string part, Logtype loglevel)
        {
            Loglevel = loglevel;
            filename = part;
            //logfilePath = adfgvxCT2.Properties.Settings.Default.logfile + filename + DateTime.Now.ToString("yyyyMMddHHmmss") + ".txt";
        }



        public void LogText(string message, Logtype logtype)
        {
            string line;

            if (logtype < Loglevel)
            {
                return;
            }
            lock (this)
            {
                switch (logtype)
                {
                    case Logtype.Debug:
                        line = DateTime.Now + " Debug: " + message;
                        Console.WriteLine(line);
                        System.IO.File.AppendAllText(logfilePath, line + Environment.NewLine);
                        break;
                    case Logtype.Info:
                        line = DateTime.Now + " Info: " + message;
                        Console.WriteLine(line);
                        System.IO.File.AppendAllText(logfilePath, line + Environment.NewLine);
                        break;
                    case Logtype.Warning:
                        line = DateTime.Now + " Warning: " + message;
                        Console.WriteLine(line);
                        System.IO.File.AppendAllText(logfilePath, line + Environment.NewLine);
                        break;
                    case Logtype.Error:
                        line = DateTime.Now + " Error: " + message;
                        Console.WriteLine(line);
                        System.IO.File.AppendAllText(logfilePath, line + Environment.NewLine);
                        break;
                    default:
                        line = DateTime.Now + " Unknown: " + message;
                        Console.WriteLine(line);
                        System.IO.File.AppendAllText(logfilePath, line + Environment.NewLine);
                        break;
                }
            }
        }

        /// <summary>
        /// Helper method to invoke LoggOccured event
        /// </summary>
        /// <param name="message"></param>
        /// <param name="logtype"></param>
        private void OnLoggOccured(string message, Logtype logtype)
        {
            if (LoggOccured != null)
            {
                LoggOccured.BeginInvoke(this, new LogEventArgs(logtype, message), null, null);
            }
        }

        /// <summary>
        /// Logs a given exception
        /// whoLoggs should be set to a reference to the object that wants to log
        /// logtype is LogType (Debug, Info, Warning, Error)
        /// </summary>
        /// <param name="ex"></param>
        /// <param name="whoLoggs"></param>
        /// <param name="logtype"></param>
        public void LogException(Exception ex, object whoLoggs, Logtype logtype)
        {
            if (logtype < Loglevel)
            {
                return;
            }

            lock (this)
            {
                switch (logtype)
                {
                    case Logtype.Debug:
                        Console.WriteLine("{0} {1} {2}: Stacktrace:", DateTime.Now, "Debug", whoLoggs != null ? whoLoggs.GetType().FullName + "-" + whoLoggs.GetHashCode() : "null");
                        Console.WriteLine(ex.StackTrace);
                        break;
                    case Logtype.Info:
                        Console.WriteLine("{0} {1} {2}: Stacktrace:", DateTime.Now, "Info", whoLoggs != null ? whoLoggs.GetType().FullName + "-" + whoLoggs.GetHashCode() : "null");
                        Console.WriteLine(ex.StackTrace);
                        break;
                    case Logtype.Warning:
                        Console.WriteLine("{0} {1} {2}: Stacktrace:", DateTime.Now, "Warning", whoLoggs != null ? whoLoggs.GetType().FullName + "-" + whoLoggs.GetHashCode() : "null");
                        Console.WriteLine(ex.StackTrace);
                        break;
                    case Logtype.Error:
                        Console.WriteLine("{0} {1} {2}: Stacktrace:", DateTime.Now, "Error", whoLoggs != null ? whoLoggs.GetType().FullName + "-" + whoLoggs.GetHashCode() : "null");
                        Console.Error.WriteLine(ex.StackTrace);
                        break;
                    default:
                        Console.WriteLine("{0} {1} {2}: Stacktrace:", DateTime.Now, "Unknown", whoLoggs != null ? whoLoggs.GetType().FullName + "-" + whoLoggs.GetHashCode() : "null");
                        Console.WriteLine(ex.StackTrace);
                        break;
                }
                OnLoggOccured(string.Format("{0} {1}: Stacktrace: {2}", (whoLoggs != null ? whoLoggs.GetType().FullName : "null"), ex.Message, ex.StackTrace), logtype);
            }
        }
    }

    /// <summary>
    /// EventArgs for a logging event
    /// </summary>
    public class LogEventArgs : EventArgs
    {
        /// <summary>
        /// Type of this log event
        /// </summary>
        public Logtype Logtype { private set; get; }

        /// <summary>
        /// Message of this log event
        /// </summary>
        public string Message { private set; get; }

        /// <summary>
        /// Creates a new LogEventArgs
        /// </summary>
        /// <param name="logtype"></param>
        /// <param name="message"></param>
        public LogEventArgs(Logtype logtype, string message)
        {
            Logtype = logtype;
            Message = message;
        }
    }
}
