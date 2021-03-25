using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Threading;
using System.Xml.Linq;
using System.Windows.Threading;
using CrypTool.PluginBase.Attributes;
using CrypTool.PluginBase.Miscellaneous;

namespace CrypTool.CrypWin
{
    /// <summary>
    /// Interaction logic for UpdaterPresentation.xaml
    /// </summary>
    [CrypTool.PluginBase.Attributes.Localization("CrypTool.CrypWin.Properties.Resources")]
    public partial class UpdaterPresentation : UserControl
    {
        public delegate void RestartClickedHandler();
        public event RestartClickedHandler OnRestartClicked;
        private static UpdaterPresentation singleton = null;

        private List<RssItem> _rssItems;
        private bool rssFilled = false;

        private UpdaterPresentation()
        {
            InitializeComponent();
            Tag = FindResource("NoUpdate");

            switch (AssemblyHelper.BuildType)
            {
                // For the developer and the nightly show the SVN-changelog (RSS-feed)
                case Ct2BuildType.Developer:
                case Ct2BuildType.Nightly:
                    ChangelogTextViewer.Visibility = Visibility.Collapsed;
                    ChangelogList.Visibility = Visibility.Visible;
                    break;

                // For Beta and stable show the specific written text as changelog
                case Ct2BuildType.Beta:
                case Ct2BuildType.Stable:
                    ChangelogTextViewer.Visibility = Visibility.Visible;
                    ChangelogList.Visibility = Visibility.Collapsed;
                    break;

                    // This should never happen, just to be on the safe side collapse/hide both controls
                default:
                    ChangelogTextViewer.Visibility = Visibility.Collapsed;
                    ChangelogList.Visibility = Visibility.Collapsed;
                    break;
            }
        }

        public static UpdaterPresentation GetSingleton()
        {
            if (singleton == null)
                singleton = new UpdaterPresentation();
            return singleton;
        }

        private void updateButton_Click(object sender, RoutedEventArgs e)
        {
            switch (AutoUpdater.GetSingleton().CurrentState)
            {
                case AutoUpdater.State.Idle:
                    AutoUpdater.GetSingleton().BeginCheckingForUpdates('M');
                    break;
                case AutoUpdater.State.Checking:
                    break;
                case AutoUpdater.State.UpdateAvailable:
                    AutoUpdater.GetSingleton().Download();
                    break;
                case AutoUpdater.State.Downloading:
                    break;
                case AutoUpdater.State.UpdateReady:
                    if (AutoUpdater.GetSingleton().IsUpdateReady)
                    {
                        OnRestartClicked();
                    }
                    break;
            }
        }

        public void ReadAndFillRSSChangelog(string rssURL)
        {
            if (rssFilled)
                return;
            
            try
            {
                _rssItems = ReadRSSItems(rssURL);
                Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                {
                    try
                    {
                        ChangelogList.DataContext = _rssItems;
                        rssFilled = true;
                    }
                    catch (Exception)
                    {
                        //Uncritical failure: Do nothing
                    }
                }, null);
            }
            catch (Exception)
            {
                //Uncritical failure: Do nothing
            }
        }

        public void FillChangelogText(string changelogText)
        {
            try
            {
                Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                {
                    try
                    {
                        ChangelogText.Html = changelogText;
                    }
                    catch (Exception)
                    {
                        //Uncritical failure: Do nothing
                    }
                }, null);
            }
            catch (Exception)
            {
                //Uncritical failure: Do nothing
            }
        }

        private List<RssItem> ReadRSSItems(string rssFeedURL)
        {
            var items = from x in XDocument.Load(rssFeedURL).Descendants("channel").Descendants("item")
                        select new RssItem()
                        {
                            Title = x.Descendants("title").Single().Value.Split(new char[] {':'},2)[0].Replace("Revision ",""),
                            Message = x.Descendants("description").Single().Value.Replace("[", "(").Replace("]", ")")
                                                                                 .Replace('<', '[').Replace('>', ']'),
                            PublishingDate = DateTime.Parse(x.Descendants("pubDate").Single().Value),
                            URL = x.Descendants("link").Single().Value
                        };
            return items.ToList();
        }
        
        private void MenuItem_Click_1(object sender, RoutedEventArgs e)
        {
            try
            {
                switch (((MenuItem)sender).Name)
                {
                    case "Copy":
                        if (ChangelogList.SelectedItem != null)
                        {
                            var rssItem = (RssItem)ChangelogList.SelectedItem;
                            Clipboard.SetData(DataFormats.Text, rssItem);
                        }
                        break;
                    case "CopyAll":
                        List<RssItem> rssItems = new List<RssItem>();
                        foreach (var item in ChangelogList.Items)
                            rssItems.Add((RssItem)item);
                        string sep = Environment.NewLine + new String('-', 80) + Environment.NewLine;
                        Clipboard.SetData(DataFormats.Text, String.Join(sep, rssItems));
                        break;
                }
            }
            catch (Exception ex)
            {
                // sometimes, we experience "problems" with copying, for example
                // System.Runtime.InteropServices.COMException (0x800401D0): OpenClipboard fehlgeschlagen (Exception from HRESULT: 0x800401D0 (CLIPBRD_E_CANT_OPEN))
                // thus, we can only catch these exceptions here... in that case, the copy just wont work at that moment
            }
        }

        private void MenuItem_Click_2(object sender, RoutedEventArgs e)
        {
            try
            {
                Clipboard.SetData(DataFormats.Text, ChangelogText.Html);
            }
            catch (Exception ex)
            {
                // same here as above
            }
        }
    }

    class RssItem
    {
        public string Title { get; set; }
        public string Message { get; set; }
        public DateTime PublishingDate { get; set; }
        public string URL { get; set; }

        public override string ToString()
        {
            return string.Format("{0}\nPublished: {1}\n\n{2}\nURL: {3}", Title, PublishingDate, Message, URL);
        }
    }
}
