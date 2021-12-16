using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Input;
using KeyEventArgs = System.Windows.Input.KeyEventArgs;

namespace LatticeCrypto.OnlineHelp
{
    public delegate void Close();

    /// <summary>
    /// Interaktionslogik für WindowOnlineHelp.xaml
    /// </summary>
    public partial class WindowOnlineHelp
    {
        private const string HELPPROTOCOL = "help://";
        private const string IMGREGEX = "<img src=\"(.+)\".+>";
        private const string IMGSRCREGEX = "src=\".+\" ";

        private readonly Regex m_ImgRegEx;
        private readonly Regex m_ImgSrcRegEx;
        private readonly WebBrowser m_Browser;
        public event Close OnClose;

        private readonly List<string> m_History;
        private int m_actualPage;

        public WindowOnlineHelp()
        {
            InitializeComponent();
            m_Browser = new WebBrowser { Dock = DockStyle.Fill };
            m_Browser.Navigating += m_Browser_Navigating;
            m_Browser.DocumentCompleted += m_Browser_SetScrollbar;
            m_Browser.SizeChanged += m_Browser_SetScrollbar;
            webbrowserhost.Child = m_Browser;
            m_History = new List<string>();
            m_actualPage = -1;
            SetEnableNavigationButtons();
            m_ImgRegEx = new Regex(IMGREGEX, RegexOptions.IgnoreCase);
            m_ImgSrcRegEx = new Regex(IMGSRCREGEX, RegexOptions.IgnoreCase);
        }

        private void m_Browser_Navigating(object sender, WebBrowserNavigatingEventArgs e)
        {
            string url = e.Url.OriginalString;
            if (string.IsNullOrEmpty(url) || !url.StartsWith(HELPPROTOCOL))
            {
                return;
            }

            url = url.Substring(HELPPROTOCOL.Length, url.Length - HELPPROTOCOL.Length);
            if (url.EndsWith("/"))
            {
                url = url.Substring(0, url.Length - 1);
            }

            NavigateTo(url);
            e.Cancel = true;
        }

        private void m_Browser_SetScrollbar(object sender, EventArgs e)
        {
            try
            {
                if (m_Browser.Document == null || m_Browser.Document.Body == null)
                {
                    return;
                }

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
            string htmltemplate = Properties.HelpLanguages.template;
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
                if (!m2.Success)
                {
                    continue;
                }

                string imgsrc = m2.Value.Remove(0, 5).Trim();
                imgsrc = imgsrc.Remove(imgsrc.Length - 1, 1);
                object s = Properties.HelpLanguages.ResourceManager.GetObject(imgsrc);
                if (s != null)
                {
                    string path = System.IO.Path.GetTempPath() + imgsrc;
                    if (s is Bitmap)
                    {
                        Bitmap image = s as Bitmap;

                        image.Save(path);
                        content = content.Replace(m2.Value, "src=\"file://" + path + "\" ");
                    }
                }
                else
                {
                    content = content.Replace(m.Value, string.Empty);
                }
            }

            return content;
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

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            if (OnClose != null)
            {
                OnClose();
            }
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);
            if (e.Key == Key.Escape)
            {
                Close();
            }
        }
    }
}
