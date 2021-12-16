using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Input;

namespace PKCS1.OnlineHelp
{
    public delegate void Close();

    /// <summary>
    /// Interaktionslogik für WindowOnlineHelp.xaml
    /// </summary>
    public partial class WindowOnlineHelp : Window
    {
        private static readonly string HELPPROTOCOL = "help://";
        //private static readonly string IMGREGEX = "<img src=\"(.+)\".+>";
        //private static readonly string IMGSRCREGEX = "src=\".+\" ";

        private int m_actualPage;
        private readonly List<string> m_History;
        private readonly System.Windows.Forms.WebBrowser m_Browser = null;
        public event Close OnClose;


        public WindowOnlineHelp()
        {
            InitializeComponent();
            m_Browser = new System.Windows.Forms.WebBrowser();
            //m_Browser.Dock = DockStyle.Fill;
            m_Browser.Navigating += new System.Windows.Forms.WebBrowserNavigatingEventHandler(m_Browser_Navigating);
            m_Browser.DocumentCompleted += new System.Windows.Forms.WebBrowserDocumentCompletedEventHandler(m_Browser_SetScrollbar);
            m_Browser.SizeChanged += new EventHandler(m_Browser_SetScrollbar);
            webbrowserhost.Child = m_Browser;
            m_actualPage = -1;
            m_History = new List<string>();
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
                catch { }
            }
            SetEnableNavigationButtons();
        }

        private void ShowHelp(string text)
        {
            string htmltemplate = PKCS1.Properties.Resources.template;
            text = htmltemplate.Replace("#content#", text);
            //text = SetImages(text);
            m_Browser.DocumentText = text;
            Show();
            Activate();
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

        protected override void OnKeyDown(System.Windows.Input.KeyEventArgs e)
        {
            base.OnKeyDown(e);
            if (e.Key == Key.Escape)
            {
                Close();
            }
        }
    }
}
