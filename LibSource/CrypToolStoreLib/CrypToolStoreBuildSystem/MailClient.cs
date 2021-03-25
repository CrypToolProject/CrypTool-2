/*
   Copyright 2019 Nils Kopal <Nils.Kopal<AT>Uni-Kassel.de>

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
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;
using CrypToolStoreLib.Tools;
using System.Net.Mime;

namespace CrypToolStoreBuildSystem
{
    public class MailClient
    {
        private SmtpClient _smtpClient;
        private Logger _logger;
 
        /// <summary>
        /// Creates a new MailClient using the given host address and port
        /// </summary>
        /// <param name="host"></param>
        /// <param name="port"></param>
        public MailClient(string host, int port)
        {
            _smtpClient = new SmtpClient(host, port);
            _logger = Logger.GetLogger();
        }

        /// <summary>
        /// Creates a new MailAddress object using the email and the displayname
        /// </summary>
        /// <param name="email"></param>
        /// <param name="displayname"></param>
        public static MailAddress CreateMailAddress(string email, string displayname)
        {
            return new MailAddress(email, displayname, System.Text.Encoding.UTF8);
        }

        /// <summary>
        /// Sends a mail "from" to "to" with the given "subject" and "body"
        /// </summary>
        /// <param name="from"></param>
        /// <param name="to"></param>
        /// <param name="subject"></param>
        /// <param name="body"></param>
        public bool SendEmail(MailAddress from, MailAddress to, string subject, string body, string attachmentTextFileName, string attachmentTextFile)
        {
            lock (this)
            {
                try
                {
                    MailMessage mailMessage = new MailMessage(from, to);
                    mailMessage.Body = body;
                    mailMessage.BodyEncoding = System.Text.Encoding.UTF8;
                    mailMessage.Subject = subject;
                    mailMessage.SubjectEncoding = System.Text.Encoding.UTF8;
                    if (attachmentTextFile != null)
                    {
                        using (MemoryStream memoryStream = new MemoryStream())
                        {
                            using (StreamWriter streamWriter = new StreamWriter(memoryStream))
                            {                                
                                streamWriter.Write(attachmentTextFile);
                                streamWriter.Flush();
                                memoryStream.Position = 0;
                                ContentType contentType = new ContentType(MediaTypeNames.Text.Plain);
                                Attachment attachment = new Attachment(memoryStream, contentType);
                                attachment.ContentDisposition.FileName = attachmentTextFileName;
                                mailMessage.Attachments.Add(attachment);
                                _smtpClient.Send(mailMessage);
                            }
                        }
                    }                                        
                    _logger.LogText(string.Format("Successfully sent email from {0} to {1}", from, to), this, Logtype.Info);
                    return true;
                }
                catch (Exception ex)
                {
                    _logger.LogText(string.Format("Exception occured during sending of build message: {0}",ex.Message), this, Logtype.Error);
                    return false;
                }
            }
        }
    }
}
