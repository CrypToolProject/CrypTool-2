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
using System;
using System.IO;
using System.Net;
using System.Text;

namespace CrypTool.Core
{
    public static class Mailer
    {
        public const int MINIMUM_DIFF = 5;

        public const string ACTION_DEVMAIL = "DEVMAIL"; // server will send mail to coredevs
        public const string ACTION_TICKET = "TICKET"; // server will create a trac ticket (and send a mail to coredevs)

        private static DateTime _lastMailTime;

        /// <summary>
        /// Send mail to developers via CT2 DEVMAIL web interface.
        /// Server-side spam protection is not handled differently from other server errors.
        /// </summary>
        /// <exception cref="SpamException">Thrown when client-side spam protection triggers</exception>
        /// <param name="action">Either DEVMAIL or TICKET</param>
        /// <param name="title">Subject (without any "CrypTool" prefixes, will be added at server-side)</param>
        /// <param name="text">Message body</param>
        /// <param name="sender">Mail from (optional)</param>
        public static void SendMailToCoreDevs(string action, string title, string text, string sender = null)
        {
            // Client-side spam check. Will fail if client changes system time.
            TimeSpan diff = DateTime.Now - _lastMailTime;
            if (diff < TimeSpan.FromSeconds(MINIMUM_DIFF))
            {
                // +1 to avoid confusing "0 seconds" text message
                throw new SpamException(string.Format(Properties.Resources.Please_wait_seconds_before_trying_again, Math.Round(MINIMUM_DIFF - diff.TotalSeconds + 1)));
            }

            WebClient client = new WebClient();
            client.Headers["User-Agent"] = "CrypTool";
            Stream stream = client.OpenWrite("https://www.CrypTool.org/cgi/ct2devmail");

            string postMessage = string.Format("action={0}&title={1}&text={2}", Uri.EscapeDataString(action), Uri.EscapeDataString(title), Uri.EscapeDataString(text));
            if (!string.IsNullOrWhiteSpace(sender))
            {
                postMessage += string.Format("&sender={0}", Uri.EscapeDataString(sender));
            }

            byte[] postEncoded = Encoding.ASCII.GetBytes(postMessage);
            stream.Write(postEncoded, 0, postEncoded.Length);
            stream.Close();

            client.Dispose();

            _lastMailTime = DateTime.Now;
        }

        public class SpamException : Exception
        {
            public SpamException(string message) : base(message)
            {
            }
        }
    }
}
