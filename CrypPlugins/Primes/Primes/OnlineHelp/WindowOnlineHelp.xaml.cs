/*
   Copyright 2008 Timo Eckhardt, University of Siegen

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
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Input;

namespace Primes.OnlineHelp
{
    /// <summary>
    /// Interaction logic for WindowOnlineHelp.xaml
    /// </summary>
    public delegate void Close();
    public partial class WindowOnlineHelp : Window
    {
        private static readonly string HELPPROTOCOL = "help://";
        private static readonly string IMGREGEX = "<img src=\"(.+)\".+>";
        private static readonly string IMGSRCREGEX = "src=\".+\" ";

        private readonly System.Text.RegularExpressions.Regex m_ImgRegEx;
        private readonly System.Text.RegularExpressions.Regex m_ImgSrcRegEx;
        private readonly System.Windows.Forms.WebBrowser m_Browser = null;
        public event Close OnClose;

        private readonly List<string> m_History;
        private int m_actualPage;

        public WindowOnlineHelp()
        {
            InitializeComponent();
            m_Browser = new System.Windows.Forms.WebBrowser
            {
                Dock = DockStyle.Fill
            };
            m_Browser.Navigating += new System.Windows.Forms.WebBrowserNavigatingEventHandler(m_Browser_Navigating);
            m_Browser.DocumentCompleted += new System.Windows.Forms.WebBrowserDocumentCompletedEventHandler(m_Browser_SetScrollbar);
            m_Browser.SizeChanged += new EventHandler(m_Browser_SetScrollbar);
            webbrowserhost.Child = m_Browser;
            m_History = new List<string>();
            m_actualPage = -1;
            SetEnableNavigationButtons();
            m_ImgRegEx = new Regex(IMGREGEX, RegexOptions.IgnoreCase);
            m_ImgSrcRegEx = new Regex(IMGSRCREGEX, RegexOptions.IgnoreCase);
        }

        private void m_Browser_Navigating(object sender, System.Windows.Forms.WebBrowserNavigatingEventArgs e)
        {
            string url = e.Url.OriginalString;
            if (!string.IsNullOrEmpty(url))
            {
                if (url.StartsWith(HELPPROTOCOL))
                {
                    url = url.Substring(HELPPROTOCOL.Length, url.Length - HELPPROTOCOL.Length);
                    if (url.EndsWith("/"))
                    {
                        url = url.Substring(0, url.Length - 1);
                    }

                    NavigateTo(url);
                    e.Cancel = true;
                }
            }
        }

        private void m_Browser_SetScrollbar(object sender, System.EventArgs e)
        {
            try
            {
                int i = m_Browser.Document.Body.ClientRectangle.Height;
                int j = m_Browser.Document.Body.ScrollRectangle.Height;
                m_Browser.ScrollBarsEnabled = (j > i);
            }
            catch (Exception)
            {
            }
        }

        public void NavigateTo(string action)
        {
            string text = OnlineHelpAccess.HelpResourceManager.GetString(action);

            if (!string.IsNullOrEmpty(text))
            {
                try
                {
                    ShowHelp(text);
                    m_actualPage++;
                    m_History.RemoveRange(m_actualPage, m_History.Count - m_actualPage);
                    m_History.Insert(m_actualPage, action);
                }
                catch (Exception)
                {
                }
            }

            SetEnableNavigationButtons();
        }

        private void ShowHelp(string text)
        {
            string htmltemplate = Primes.Properties.Resources.template;
            text = htmltemplate.Replace("#content#", text);
            text = SetImages(text);
            m_Browser.DocumentText = text;
            Show();
            Activate();
        }

        private string SetImages(string content)
        {
            foreach (Match m in m_ImgRegEx.Matches(content))
            {
                Match m2 = m_ImgSrcRegEx.Match(m.Value);
                if (m2.Success)
                {
                    string imgsrc = m2.Value.Remove(0, 5).Trim();
                    imgsrc = imgsrc.Remove(imgsrc.Length - 1, 1);
                    object s = Properties.Resources.ResourceManager.GetObject(imgsrc);
                    if (s != null)
                    {
                        string path = System.IO.Path.GetTempPath() + imgsrc;
                        if (s.GetType() == typeof(System.Drawing.Bitmap))
                        {
                            System.Drawing.Bitmap image = s as System.Drawing.Bitmap;

                            image.Save(path);
                            content = content.Replace(m2.Value, "src=\"file://" + path + "\" ");
                        }
                    }
                    else
                    {
                        content = content.Replace(m.Value, string.Empty);
                    }
                }
            }

            return content;
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            if (OnClose != null)
            {
                OnClose();
            }
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        protected override void OnKeyDown(System.Windows.Input.KeyEventArgs e)
        {
            base.OnKeyDown(e);
            if (e.Key == Key.Escape)
            {
                Close();
            }
        }

        private void btnHistoryBack_Click(object sender, RoutedEventArgs e)
        {
            if (m_actualPage > 0)
            {
                string text = OnlineHelpAccess.HelpResourceManager.GetString(m_History[m_actualPage - 1]);
                if (!string.IsNullOrEmpty(text))
                {
                    ShowHelp(text);
                    m_actualPage--;
                }
            }

            SetEnableNavigationButtons();
        }

        private void btnHistoryForward_Click(object sender, RoutedEventArgs e)
        {
            if (m_actualPage < m_History.Count - 1)
            {
                string text = OnlineHelpAccess.HelpResourceManager.GetString(m_History[m_actualPage + 1]);
                if (!string.IsNullOrEmpty(text))
                {
                    ShowHelp(text);
                    m_actualPage++;
                }
            }

            SetEnableNavigationButtons();
        }

        private void SetEnableNavigationButtons()
        {
            btnHistoryBack.IsEnabled = m_History.Count > 0 && m_actualPage > 0;
            btnHistoryForward.IsEnabled = m_History.Count > 0 && m_actualPage < m_History.Count - 1;
        }
    }
}
