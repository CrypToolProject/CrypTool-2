using System;
using System.IO;

namespace M209AnalyzerLib.Common
{
    public static class Logger
    {
        public static string LogPath;
        public static void WriteLog(string message)
        {
            if (LogPath != String.Empty)
            {
                using (StreamWriter writer = new StreamWriter(LogPath, true))
                {
                    writer.WriteLine($"{DateTime.Now} - {message}");
                }

            }
        }
    }
}
