/*
   Copyright 2008 - 2022 CrypTool Team

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
using CrypTool.PluginBase;

namespace CrypWin.Helper
{
    public class LogMessage
    {
        public int Nr { get; set; }
        public NotificationLevel LogLevel { get; set; }
        public string Time { get; set; }
        public string Plugin { get; set; }
        public string Title { get; set; }
        public string Message { get; set; }

        public LogMessage() { }

        public LogMessage(int nr, NotificationLevel logLevel, string plugin, string title, string time, string message)
        {
            Nr = nr;
            LogLevel = logLevel;
            Plugin = plugin;
            Time = time;
            Message = message;
            Title = title;
        }

        public static string Color(NotificationLevel level)
        {
            switch (level)
            {
                case NotificationLevel.Debug:
                    return "00FF21";
                case NotificationLevel.Info:
                    return "5B9AFF";
                case NotificationLevel.Warning:
                    return "FFD800";
                case NotificationLevel.Error:
                    return "FF3C2B";
                case NotificationLevel.Balloon:
                    return "FB2C1B";
                default:
                    return string.Empty;
            }
        }

        public override string ToString()
        {
            return Time + " " + Plugin + " [" + LogLevel + "]: (" + Title + ") " + Message;
        }
    }
}
