/*
   Copyright 2008-2022 CrypTool Team

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
using System.Globalization;
using System.Linq;
using System.Net;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Threading;
using System.Xml;
using System.Xml.Linq;

namespace Startcenter.Controls
{
    /// <summary>
    /// Interaction logic for RSSViewer.xaml
    /// </summary>
    [CrypTool.PluginBase.Attributes.Localization("Startcenter.Properties.Resources")]
    public partial class RSSViewerControl : UserControl
    {
        private List<RssItem> _rssItems;
        //exchanged svn logs with our YouTube channel
        private const string RSSUrl = "https://www.youtube.com/feeds/videos.xml?channel_id=UC8_FqvQWJfZYxcSoEJ5ob-Q";

        public static readonly DependencyProperty IsUpdatingProperty =
            DependencyProperty.Register("IsUpdating",
                                        typeof(bool),
                                        typeof(RSSViewerControl), new PropertyMetadata(false));

        public bool IsUpdating
        {
            get => (bool)GetValue(IsUpdatingProperty);
            set => SetValue(IsUpdatingProperty, value);
        }

        public RSSViewerControl()
        {
            InitializeComponent();
            IsUpdating = true;
            Timer updateTimer = new Timer(ReadAndFillRSSItems);
            updateTimer.Change(0, 1000 * 60);
        }

        private void ReadAndFillRSSItems(object state)
        {
            try
            {
                _rssItems = ReadRSSItems(RSSUrl);
                Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                {
                    try
                    {
                        IsUpdating = false;
                        rssListBox.DataContext = _rssItems;
                    }
                    catch (Exception)
                    {
                        //Uncritical failure: Do nothing
                    }
                }, null);
            }
            catch (Exception ex)
            {
                Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                {
                    IsUpdating = false;
                    List<RssItem> errorRSSFeed = new List<RssItem>(1)
                   {
                        new RssItem() { Message = Properties.Resources.RSS_error_Message, Title = Properties.Resources.RSS_error_Message },
                        new RssItem() { Message = ex.Message, Title = Properties.Resources.Exception }
                   };
                    rssListBox.DataContext = errorRSSFeed;
                }, null);
            }
        }

        private List<RssItem> ReadRSSItems(string rssFeedURL)
        {
            try
            {
                HttpWebRequest req = (HttpWebRequest)WebRequest.Create(rssFeedURL);
                req.Method = "GET";
                req.UserAgent = "CrypTool 2/Startcenter RSS Viewer";
                WebResponse rep = req.GetResponse();
                XmlReader reader = XmlReader.Create(rep.GetResponseStream());
                XDocument doc = XDocument.Load(reader);
                XNamespace ns = "http://www.w3.org/2005/Atom";
                XNamespace media_ns = "http://search.yahoo.com/mrss/";
                IEnumerable<XElement> entries = doc.Root.Elements(ns + "entry");
                IEnumerable<RssItem> items = from entry in entries
                                             select new RssItem()
                                             {
                                                 Title = entry.Element(ns + "title").Value.Trim(),
                                                 Message = entry.Element(media_ns + "group").Element(media_ns + "description").Value.Trim(),
                                                 PublishingDate = DateTime.Parse(entry.Element(ns + "published").Value.Trim()),
                                                 URL = entry.Element(ns + "link").Attribute("href").Value.Trim(),
                                                 ThumbnailURL = entry.Element(media_ns + "group").Element(media_ns + "thumbnail").Attribute("url").Value.Trim()
                                             };
                return items.ToList();
            }
            catch (Exception)
            {
                return new List<RssItem>();
            }
        }

        private void MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            RssItem rssItem = (RssItem)rssListBox.SelectedItem;
            if (rssItem != null)
            {
                System.Diagnostics.Process.Start((string)(rssItem.URL));
            }            
        }
    }

    internal class RssItem
    {
        public string Title { get; set; }
        public string Message { get; set; }
        public DateTime PublishingDate { get; set; }
        public string URL { get; set; }
        public string ThumbnailURL { get; set; }
    }

    [ValueConversion(typeof(bool), typeof(Visibility))]
    public class FalseToVisibleConverter : IValueConverter
    {
        #region IValueConverter Members

        public object Convert(object value, Type targetType, object parameter,
                              CultureInfo culture)
        {
            if (targetType != typeof(Visibility))
            {
                throw new InvalidOperationException("The target must be of Visibility");
            }

            if ((bool)value)
            {
                return Visibility.Hidden;
            }

            return Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter,
                                  CultureInfo culture)
        {
            throw new NotSupportedException();
        }

        #endregion
    }

    [ValueConversion(typeof(bool), typeof(Visibility))]
    public class TrueToVisibleConverter : IValueConverter
    {
        #region IValueConverter Members

        public object Convert(object value, Type targetType, object parameter,
                              CultureInfo culture)
        {
            if (targetType != typeof(Visibility))
            {
                throw new InvalidOperationException("The target must be of Visibility");
            }

            if ((bool)value)
            {
                return Visibility.Visible;
            }

            return Visibility.Hidden;
        }

        public object ConvertBack(object value, Type targetType, object parameter,
                                  CultureInfo culture)
        {
            throw new NotSupportedException();
        }

        #endregion
    }
}
