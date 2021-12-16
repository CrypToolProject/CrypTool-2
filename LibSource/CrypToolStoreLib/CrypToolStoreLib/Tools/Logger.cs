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
using System.IO;

namespace CrypToolStoreLib.Tools
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

    public class Logger : IDisposable
    {
        private DateTime _LogFileOpenedTime;
        private FileStream _FileStream;
        private StreamWriter _StreamWriter;
        private static Logger Instance;
        private static Logtype Loglevel = Logtype.Info;

        public event EventHandler<LogEventArgs> LoggOccured;

        /// <summary>
        /// Enables log to file
        /// </summary>
        public static bool EnableFileLog
        {
            get;
            set;
        }

        /// <summary>
        /// Prefix of the logfile name
        /// </summary>
        public static string LogFilePrefix
        {
            get;
            set;
        }

        /// <summary>
        /// Interval in days, in which logfiles should be deleted
        /// </summary>
        public static int LogFileDeleteInterval
        {
            get;
            set;
        }

        /// <summary>
        /// Directory, in which logfiles should be written
        /// </summary>
        public static string LogDirectory
        {
            get;
            set;
        }

        /// <summary>
        /// Static constructor
        /// Sets config to default values
        /// </summary>
        static Logger()
        {
            EnableFileLog = false;
            LogFilePrefix = "logfile";
            LogFileDeleteInterval = 31;
            LogDirectory = "Logs";
        }

        /// <summary>
        /// Singleton, thus private constructor
        /// </summary>
        private Logger()
        {
        }

        /// <summary>
        /// Returns the global logger instance
        /// </summary>
        /// <returns></returns>
        public static Logger GetLogger()
        {
            if (Instance == null)
            {
                Instance = new Logger();
            }
            if (EnableFileLog)
            {
                if (!Directory.Exists(LogDirectory))
                {
                    Directory.CreateDirectory(LogDirectory);
                }
            }
            return Instance;
        }

        /// <summary>
        /// Set minimum loglevel (default = info)
        /// means, that only messages with level info or higher will be logged
        /// </summary>
        /// <param name="loglevel"></param>
        public static void SetLogLevel(Logtype loglevel)
        {
            Loglevel = loglevel;
        }

        /// <summary>
        /// Logs a given text
        /// whoLoggs should be set to a reference to the object that wants to log
        /// logtype is LogType (Debug, Info, Warning, Error)
        /// </summary>
        /// <param name="message"></param>
        /// <param name="whoLoggs"></param>
        /// <param name="logtype"></param>
        public void LogText(string message, object whoLoggs, Logtype logtype)
        {
            if (logtype < Loglevel)
            {
                return;
            }
            lock (this)
            {
                if (EnableFileLog)
                {
                    if (_FileStream == null)
                    {
                        CreateAndOpenLogFile();
                    }
                    if (DateTime.Now > _LogFileOpenedTime.AddDays(1))
                    {
                        _StreamWriter.Flush();
                        _StreamWriter.Close();
                        CreateAndOpenLogFile();
                        DeleteOldLogfiles();
                    }
                }
                switch (logtype)
                {
                    case Logtype.Debug:
                        Console.ForegroundColor = ConsoleColor.Blue;
                        Console.WriteLine("{0} {1} {2}: {3}", DateTime.Now, "Debug", whoLoggs != null ? whoLoggs.GetType().FullName + "-" + whoLoggs.GetHashCode() : "null", message);
                        if (EnableFileLog)
                        {
                            _StreamWriter.WriteLine("{0} {1} {2}: {3}", DateTime.Now, "Debug", whoLoggs != null ? whoLoggs.GetType().FullName + "-" + whoLoggs.GetHashCode() : "null", message);
                            _StreamWriter.Flush();
                        }
                        Console.ResetColor();
                        break;
                    case Logtype.Info:
                        Console.WriteLine("{0} {1} {2}: {3}", DateTime.Now, "Info", whoLoggs != null ? whoLoggs.GetType().FullName + "-" + whoLoggs.GetHashCode() : "null", message);
                        if (EnableFileLog)
                        {
                            _StreamWriter.WriteLine("{0} {1} {2}: {3}", DateTime.Now, "Info", whoLoggs != null ? whoLoggs.GetType().FullName + "-" + whoLoggs.GetHashCode() : "null", message);
                            _StreamWriter.Flush();
                        }
                        break;
                    case Logtype.Warning:
                        Console.ForegroundColor = ConsoleColor.Yellow;
                        Console.WriteLine("{0} {1} {2}: {3}", DateTime.Now, "Warning", whoLoggs != null ? whoLoggs.GetType().FullName + "-" + whoLoggs.GetHashCode() : "null", message);
                        if (EnableFileLog)
                        {
                            _StreamWriter.WriteLine("{0} {1} {2}: {3}", DateTime.Now, "Warning", whoLoggs != null ? whoLoggs.GetType().FullName + "-" + whoLoggs.GetHashCode() : "null", message);
                            _StreamWriter.Flush();
                        }
                        Console.ResetColor();
                        break;
                    case Logtype.Error:
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.Error.WriteLine("{0} {1} {2}: {3}", DateTime.Now, "Error", whoLoggs != null ? whoLoggs.GetType().FullName + "-" + whoLoggs.GetHashCode() : "null", message);
                        if (EnableFileLog)
                        {
                            _StreamWriter.WriteLine("{0} {1} {2}: {3}", DateTime.Now, "Error", whoLoggs != null ? whoLoggs.GetType().FullName + "-" + whoLoggs.GetHashCode() : "null", message);
                            _StreamWriter.Flush();
                        }
                        Console.ResetColor();
                        break;
                    default:
                        Console.WriteLine("{0} {1} {2}: {3}", DateTime.Now, "Unknown", whoLoggs != null ? whoLoggs.GetType().FullName + "-" + whoLoggs.GetHashCode() : "null", message);
                        if (EnableFileLog)
                        {
                            _StreamWriter.WriteLine("{0} {1} {2}: {3}", DateTime.Now, "Unknown", whoLoggs != null ? whoLoggs.GetType().FullName + "-" + whoLoggs.GetHashCode() : "null", message);
                            _StreamWriter.Flush();
                        }
                        break;
                }
                OnLoggOccured(string.Format("{0} {1}", (whoLoggs != null ? whoLoggs.GetType().FullName : "null"), message), logtype);
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
                if (EnableFileLog)
                {
                    if (_FileStream == null)
                    {
                        CreateAndOpenLogFile();
                    }
                    if (DateTime.Now > _LogFileOpenedTime.AddDays(1))
                    {
                        _StreamWriter.Flush();
                        _StreamWriter.Close();
                        CreateAndOpenLogFile();
                        DeleteOldLogfiles();
                    }
                }

                switch (logtype)
                {
                    case Logtype.Debug:
                        Console.WriteLine("{0} {1} {2}: Stacktrace:", DateTime.Now, "Debug", whoLoggs != null ? whoLoggs.GetType().FullName + "-" + whoLoggs.GetHashCode() : "null");
                        if (EnableFileLog)
                        {
                            _StreamWriter.WriteLine("{0} {1} {2}: Stacktrace:", DateTime.Now, "Debug", whoLoggs != null ? whoLoggs.GetType().FullName + "-" + whoLoggs.GetHashCode() : "null");
                            _StreamWriter.Flush();
                        }
                        Console.WriteLine(ex.StackTrace);
                        if (EnableFileLog)
                        {
                            _StreamWriter.WriteLine(ex.StackTrace);
                            _StreamWriter.Flush();
                        }
                        break;
                    case Logtype.Info:
                        Console.WriteLine("{0} {1} {2}: Stacktrace:", DateTime.Now, "Info", whoLoggs != null ? whoLoggs.GetType().FullName + "-" + whoLoggs.GetHashCode() : "null");
                        if (EnableFileLog)
                        {
                            _StreamWriter.WriteLine("{0} {1} {2}: Stacktrace:", DateTime.Now, "Info", whoLoggs != null ? whoLoggs.GetType().FullName + "-" + whoLoggs.GetHashCode() : "null");
                            _StreamWriter.Flush();
                        }
                        Console.WriteLine(ex.StackTrace);
                        if (EnableFileLog)
                        {
                            _StreamWriter.WriteLine(ex.StackTrace);
                            _StreamWriter.Flush();
                        }
                        break;
                    case Logtype.Warning:
                        Console.WriteLine("{0} {1} {2}: Stacktrace:", DateTime.Now, "Warning", whoLoggs != null ? whoLoggs.GetType().FullName + "-" + whoLoggs.GetHashCode() : "null");
                        if (EnableFileLog)
                        {
                            _StreamWriter.WriteLine("{0} {1} {2}: Stacktrace:", DateTime.Now, "Warning", whoLoggs != null ? whoLoggs.GetType().FullName + "-" + whoLoggs.GetHashCode() : "null");
                            _StreamWriter.Flush();
                        }
                        Console.WriteLine(ex.StackTrace);
                        if (EnableFileLog)
                        {
                            _StreamWriter.WriteLine(ex.StackTrace);
                            _StreamWriter.Flush();
                        }
                        break;
                    case Logtype.Error:
                        Console.WriteLine("{0} {1} {2}: Stacktrace:", DateTime.Now, "Error", whoLoggs != null ? whoLoggs.GetType().FullName + "-" + whoLoggs.GetHashCode() : "null");
                        if (EnableFileLog)
                        {
                            _StreamWriter.WriteLine("{0} {1} {2}: Stacktrace:", DateTime.Now, "Error", whoLoggs != null ? whoLoggs.GetType().FullName + "-" + whoLoggs.GetHashCode() : "null");
                            _StreamWriter.Flush();
                        }
                        Console.Error.WriteLine(ex.StackTrace);
                        if (EnableFileLog)
                        {
                            _StreamWriter.WriteLine(ex.StackTrace);
                            _StreamWriter.Flush();
                        }
                        break;
                    default:
                        Console.WriteLine("{0} {1} {2}: Stacktrace:", DateTime.Now, "Unknown", whoLoggs != null ? whoLoggs.GetType().FullName + "-" + whoLoggs.GetHashCode() : "null");
                        if (EnableFileLog)
                        {
                            _StreamWriter.WriteLine("{0} {1} {2}: Stacktrace:", DateTime.Now, "Unknown", whoLoggs != null ? whoLoggs.GetType().FullName + "-" + whoLoggs.GetHashCode() : "null");
                            _StreamWriter.Flush();
                        }
                        Console.WriteLine(ex.StackTrace);
                        if (EnableFileLog)
                        {
                            _StreamWriter.WriteLine(ex.StackTrace);
                            _StreamWriter.Flush();
                        }
                        break;
                }
                OnLoggOccured(string.Format("{0} {1}: Stacktrace: {2}", (whoLoggs != null ? whoLoggs.GetType().FullName : "null"), ex.Message, ex.StackTrace), logtype);
            }
        }

        /// <summary>
        /// Deletes old log files from log folder
        /// </summary>
        private void DeleteOldLogfiles()
        {
            foreach (string filename in Directory.GetFiles(LogDirectory))
            {
                DateTime creationdate = File.GetCreationTime(filename);
                if (DateTime.Now > creationdate.AddDays(LogFileDeleteInterval))
                {
                    File.Delete(filename);
                }
            }
        }

        /// <summary>
        /// Creates a new logfile and opens it
        /// </summary>
        private void CreateAndOpenLogFile()
        {
            string logfilename = LogDirectory + Path.DirectorySeparatorChar + LogFilePrefix + "_" + DateTime.Now.Year + "_" + DateTime.Now.Month + "_" + DateTime.Now.Day + "_" + DateTime.Now.Hour + "_" + DateTime.Now.Minute + ".log";
            _FileStream = new FileStream(logfilename, FileMode.Create, FileAccess.Write);
            _StreamWriter = new StreamWriter(_FileStream);
            _LogFileOpenedTime = DateTime.Now;
        }

        /// <summary>
        /// Closes the current open logfile
        /// </summary>
        public void Dispose()
        {
            if (EnableFileLog)
            {
                _StreamWriter.Flush();
                _StreamWriter.Close();
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
