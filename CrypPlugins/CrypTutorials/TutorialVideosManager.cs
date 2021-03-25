using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Xml.Linq;

namespace CrypTool.CrypTutorials
{
    public class TutorialVideosManager
    {
        private readonly string _url = PluginBase.Properties.Settings.Default.CrypVideoTutorials_URL;
        private readonly string _catUrl = PluginBase.Properties.Settings.Default.CrypVideoTutorials_CatURL;
        private List<VideoInfo> _videoInfos;
        private List<Category> _catInfos;

        public TutorialVideosManager()
        {
            
        }

        /// <summary>
        /// Constructs a new TutorialVideosManager which asks the Video Web Server for
        /// Tutorial Videos Information
        /// </summary>
        /// <param name="url"></param>

        /// <summary>
        /// Fired if video Informations are fetched
        /// </summary>
        public event EventHandler<VideosFetchedEventArgs> OnVideosFetched;

        public event EventHandler<CategoriesFetchedEventArgs> OnCategoriesFetched;

        /// <summary>
        /// Fired in case of an error
        /// </summary>
        public event EventHandler<ErrorEventArgs> OnVideosFetchErrorOccured;

        /// <summary>
        ///  Helper to generate Test Data for Gui Testing
        /// Fires OnVideosFetched in case of success (amount more than 0)
        /// Fires OnVideosFetchErrorOccured in case of an error (amount less or equal to 0)
        /// </summary>
        /// <param name="videoUrl"></param>
        /// <param name="amount"></param>
        public void GenerateTestData(String videoUrl, int amount)
        {
            if (amount <= 0)
            {
                if (OnVideosFetchErrorOccured != null)
                {
                    OnVideosFetchErrorOccured.Invoke(this, null);
                }
                return;
            }
            _videoInfos = new List<VideoInfo>();
            for (int i = 0; i < amount; i++)
            {
                _videoInfos.Add(new VideoInfo
                {
                    Description =
                        "A tutorial is a method of transferring knowledge and may be used as a part of a " +
                        "learning process. More interactive and specific than a book or a lecture; a " +
                        "tutorial seeks to teach by example and supply the information to complete a " +
                        "certain task.",
                    Id = i.ToString(CultureInfo.InvariantCulture),
                    Timestamp = DateTime.Now,
                    Title = "Tutorial - From Wikipedia, the free encyclopedia",
                    Url = videoUrl
                });
            }
            if (OnVideosFetched != null)
            {
                OnVideosFetched.Invoke(this, new VideosFetchedEventArgs(_videoInfos));
            }
        }

        private static readonly DateTime UnixEpoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        public static long GetCurrentUnixTimestampSeconds()
        {
            return (long)(DateTime.UtcNow - UnixEpoch).TotalSeconds;
        }

        public static DateTime DateTimeFromUnixTimestampSeconds(string sec)
        {
            double seconds = double.Parse(sec, CultureInfo.InvariantCulture);
            return UnixEpoch.AddSeconds(seconds);
        }

        private List<Category> parseSubCat(string s)
        {
            if (s == "")
                return null;

            var _catInfos = new List<Category>();
            XElement xroot = XElement.Parse(s);
            _catInfos =
                (from item in xroot.Elements("category")
                 let id = item.Element("id")
                 let name = item.Element("name")
                 let subcat = item.Element("subcategories")
                 select new Category
                 {
                     Id = int.Parse(id.Value),
                     Name = name.Value,
                     Children = parseSubCat(subcat.ToString()),
                 }).ToList();
            return _catInfos;
        }

        private void evalParents(List<Category> cat, Category parent)
        {
            if (cat.Count == 0)
                return;

            foreach (var element in cat)
            {
                element.Parent = parent;
                evalParents(element.Children, element);
            }
        }

        /// <summary>
        /// Retrieve Video Informations from Server asynchronously
        /// Fires OnVideosFetched in case of success
        /// Fires OnVideosFetchErrorOccured in case of an error
        /// </summary>
        public void GetVideoInformationFromServer()
        {
            try
            {
                _catInfos = new List<Category>();
                XElement xraw = XElement.Load(_catUrl);
                XElement xroot = XElement.Parse(xraw.ToString());
                _catInfos =
                    (from item in xroot.Elements("category")
                     let id = item.Element("id")
                     let name = item.Element("name")
                     let subcat = item.Element("subcategories")
                     select new Category
                     {
                         Id = int.Parse(id.Value),
                         Name = name.Value,
                         Children = parseSubCat(subcat.ToString()),
                     }).ToList();

                foreach (var element in _catInfos)
                {
                    evalParents(element.Children, element);
                }

                if (OnCategoriesFetched != null)
                {
                    OnCategoriesFetched.Invoke(this, new CategoriesFetchedEventArgs(_catInfos));
                }

                ////////////////////////////////////////////////////////////////
                _videoInfos = new List<VideoInfo>();
                xraw = XElement.Load(_url);
                xroot = XElement.Parse(xraw.ToString());
                List<VideoInfo> links =
                    (from item in xroot.Descendants("video")
                     let id = item.Element("id")
                     let title = item.Element("title")
                     let description = item.Element("description")
                     let icon = item.Element("icon")
                     let url = item.Element("url")
                     let timestamp = item.Element("timestamp")
                     let cat = item.Element("category")
                     select new VideoInfo
                     {
                         Id = id.Value,
                         Title = title.Value,
                         Description = description.Value,
                         Icon = icon.Value,
                         Url = url.Value,
                         Timestamp = DateTimeFromUnixTimestampSeconds(timestamp.Value),
                         Category = int.Parse(cat.Value)
                     }).ToList();

                _videoInfos = links;
                if (OnVideosFetched != null)
                {
                    OnVideosFetched.Invoke(this, new VideosFetchedEventArgs(links));
                }
            }
            catch (Exception exception)
            {
                if (OnVideosFetchErrorOccured != null)
                {
                    OnVideosFetchErrorOccured.Invoke(this,new ErrorEventArgs(exception));
                }
            }
        }
    }

    public class VideosFetchedEventArgs : EventArgs
    {
        public VideosFetchedEventArgs(List<VideoInfo> videoInfos)
        {
            VideoInfos = videoInfos;
        }

        public List<VideoInfo> VideoInfos { get; private set; }
    }

    public class CategoriesFetchedEventArgs : EventArgs
    {
        public CategoriesFetchedEventArgs(List<Category> categories)
        {
            Categories = categories;
        }

        public List<Category> Categories { get; private set; }
    }
}