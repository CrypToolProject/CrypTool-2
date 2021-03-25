using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BitcoinDownloadServer
{

    public enum Logtype
    {
        Debug = 0,
        Info = 1,
        Warning = 2,
        Error = 3
    }

    public class Logger
    {
        string logfilePath = Properties.Settings.Default.logfile + "bitcoinserver_" + DateTime.Today.ToShortDateString() + ".txt";

        private static Logger Instance = new Logger();
        private static Logtype Loglevel = Logtype.Info;

        //private constructor
        public Logger()
        {

        }

        //Returns the global logger instance
        public static Logger GetLogger()
        {
            return Instance;
        }

        //Set minimum loglevel (default = info)
        //means, that only messages with level info or higher will be logged
        public static void SetLogLevel(Logtype loglevel)
        {
            Loglevel = loglevel;
        }

        private void createLogFile()
        {
            if(!File.Exists(Properties.Settings.Default.logfile + "bitcoinserver_" + DateTime.Today.ToShortDateString() + ".txt"))
            {
                logfilePath = Properties.Settings.Default.logfile + "bitcoinserver_" + DateTime.Today.ToShortDateString() + ".txt";
            }

        }


        public void LogText(string message, Logtype logtype)
        {
            createLogFile();
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
    }

}
