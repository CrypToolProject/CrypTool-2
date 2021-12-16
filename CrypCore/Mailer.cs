using System;
using System.Net;
using System.Text;

namespace CrypTool.Core
{
    public static class Mailer
    {
        public const int MINIMUM_DIFF = 5;

        public const string ACTION_DEVMAIL = "DEVMAIL"; // server will send mail to coredevs
        public const string ACTION_TICKET = "TICKET"; // server will create a trac ticket (and send a mail to coredevs)

        private static DateTime lastMailTime;

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
            TimeSpan diff = DateTime.Now - lastMailTime;
            if (diff < TimeSpan.FromSeconds(MINIMUM_DIFF))
            {
                // +1 to avoid confusing "0 seconds" text message
                throw new SpamException(string.Format(Properties.Resources.Please_wait_seconds_before_trying_again, Math.Round(MINIMUM_DIFF - diff.TotalSeconds + 1)));
            }

            WebClient client = new WebClient();
            client.Headers["User-Agent"] = "CrypTool";
            System.IO.Stream stream = client.OpenWrite("https://www.CrypTool.org/cgi/ct2devmail");

            string postMessage = string.Format("action={0}&title={1}&text={2}", Uri.EscapeDataString(action), Uri.EscapeDataString(title), Uri.EscapeDataString(text));
            if (!string.IsNullOrWhiteSpace(sender))
            {
                postMessage += string.Format("&sender={0}", Uri.EscapeDataString(sender));
            }

            byte[] postEncoded = Encoding.ASCII.GetBytes(postMessage);
            stream.Write(postEncoded, 0, postEncoded.Length);
            stream.Close();

            client.Dispose();

            lastMailTime = DateTime.Now;
        }

        public class SpamException : Exception
        {
            public SpamException(string message) : base(message)
            {
            }
        }
    }
}
