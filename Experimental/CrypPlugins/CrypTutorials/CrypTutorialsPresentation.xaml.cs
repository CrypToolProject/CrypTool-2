using System.Collections.Generic;
using System.Collections.ObjectModel;
using CrypTool.PluginBase.Attributes;
using System;
using System.Windows.Data;
using System.Windows.Controls;
using System.ComponentModel;
using System.Globalization;
using System.Windows;
using System.Collections;
using System.Windows.Threading;
using System.Threading;
using System.IO;
using Microsoft.Win32;
using Vlc.DotNet.Core;
using System.Linq;
using System.Diagnostics;

namespace CrypTool.CrypTutorials
{
    [Localization("CrypTool.CrypTutorials.Properties.Resources")]
    public partial class CrypTutorialsPresentation : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        //private readonly CrypTutorials _crypTutorials;
        //private const string _VideoUrl = "http://localhost/ct2/videos.xml";
        private readonly TutorialVideosManager _tutorialVideosManager = new TutorialVideosManager();
        private readonly ObservableCollection<VideoInfo> _videos = new ObservableCollection<VideoInfo>();
        private readonly ObservableCollection<Category> _categories = new ObservableCollection<Category>();

        private VideoInfo playingItem = null;
        private ListCollectionView _videosView;
        private string _filterString = "";

        private Category selectedCategory;
        public Category SelectedCategory
        {
            get { return selectedCategory; }
            set
            {
                selectedCategory = value;
                _videosView.Refresh();
                OnPropertyChanged("SelectedCategory");
            }
        }

        public VideoInfo PlayingItem
        {
            get { return playingItem; }
            set
            {
                if (!VlcContext.IsInitialized)
                    return;

                playingItem = value;
                if (playingItem == null)
                {
                    Player.Visibility = Visibility.Collapsed;
                    Player.Close();
                }
                else
                {
                    Player.Visibility = Visibility.Visible;
                    Player.Play(playingItem.Url);
                }

                OnPropertyChanged("PlayingItem");
            }
        }

        public string FilterString
        {
            get { return _filterString; }
            set
            {
                _filterString = value;
                OnPropertyChanged("FilterString");
            }
        }

        //public CrypTutorialsPresentation(CrypTutorials crypTutorials)
        public CrypTutorialsPresentation()
        {
            DataContext = this;       
            InitializeComponent();

            VLCInitError.Visibility = Visibility.Visible;
            VlcLoad.VlcInitialized += new EventHandler(VlcInitializedHandler);
            //_crypTutorials = crypTutorials;

            _tutorialVideosManager.OnVideosFetched += _tutorialVideosManager_OnVideosFetched;
            _tutorialVideosManager.OnCategoriesFetched += new EventHandler<CategoriesFetchedEventArgs>(_tutorialVideosManager_OnCategoriesFetched);
            _tutorialVideosManager.OnVideosFetchErrorOccured += new EventHandler<ErrorEventArgs>(_tutorialVideosManager_OnVideosFetchErrorOccured);

            //has to be replaced later on by "GetVideoInformationFromServer"
            //_tutorialVideosManager.GenerateTestData("http://localhost/ct2/videos.xml", 16);
            _tutorialVideosManager.GetVideoInformationFromServer();
            _videosView = CollectionViewSource.GetDefaultView(Videos) as ListCollectionView;
            _videosView.CustomSort = new VideoSorter();
            _videosView.Filter = videoFilter;
        }

        void VlcInitializedHandler(object sender, EventArgs e)
        {
             VLCInitError.Visibility = Visibility.Collapsed;
        }

        void _tutorialVideosManager_OnVideosFetchErrorOccured(object sender, ErrorEventArgs e)
        {
            var exception = e.GetException();
            if (exception is System.Net.WebException)
            {
                NetworkErrorPanel.Visibility = Visibility.Visible;
            }
            else 
            {
                XMLParseError.Visibility = Visibility.Visible;
            }
        }


        private Dictionary<int, Category> catMap = new Dictionary<int,Category>();


        void makeHashMap(List<Category> cats)
        {
            if (cats.Count == 0)
                return;

            foreach (var cat in cats)
            {
                catMap.Add(cat.Id, cat);
                makeHashMap(cat.Children);
            }
        }

        void _tutorialVideosManager_OnCategoriesFetched(object sender, CategoriesFetchedEventArgs e)
        {
            _categories.Clear();
            _categories.Add(new Category() { Id = int.MaxValue, Name = "All"});
            _categories.Add(new Category() { Id = int.MinValue, Name = "Misc" });
            foreach (var cat in e.Categories)
            {
                _categories.Add(cat);
            }
            makeHashMap(new List<Category>(_categories));
        }


        void findAllVideos(List<Category> cats, List<Category> allCats)
        {
            if (cats.Count == 0)
                return;

            foreach(var cat in cats)
            {
                allCats.AddRange(cats);
                findAllVideos(cat.Children, allCats);
            }
        }

        private bool videoFilter(object item)
        {
            var videoinfo = item as VideoInfo;
            if (_filterString != string.Empty)
            {
                return videoinfo.Title.Contains(_filterString);
            }

            if (SelectedCategory != null)
            {
                if (SelectedCategory.Id == int.MaxValue)
                {
                    return true;
                }
            }

            if (SelectedCategory != null)
            {
                var list = new List<Category>();
                list.Add(selectedCategory);
                findAllVideos(SelectedCategory.Children, list);
                return list.Contains(catMap[videoinfo.Category]);
            }
            return true;
        }

        internal class VideoSorter : IComparer
        {
            public int Compare(object x, object y)
            {
                var vidx = x as VideoInfo;
                var vidY = y as VideoInfo;
                return vidx.Timestamp.CompareTo(vidY.Timestamp);
            }
        }

        private void SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        public ObservableCollection<VideoInfo> Videos
        {
            get { return _videos; }
        }

        public ObservableCollection<Category> Categories
        {
            get { return _categories; }
        }

        private void _tutorialVideosManager_OnVideosFetched(object sender, VideosFetchedEventArgs videosFetchedEventArgs)
        {
            _videos.Clear();
            foreach (var videoInfo in videosFetchedEventArgs.VideoInfos)
            {
                _videos.Add(videoInfo);

                Category cat;
                if (catMap.TryGetValue(videoInfo.Category, out cat))
                    cat.Count++;
                else {
                    videoInfo.Category = int.MinValue;
                    catMap[int.MinValue].Count++;
                }
 
                catMap[int.MaxValue].Count++;
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (VideoListView.SelectedItem != null)
            {
                PlayingItem = (VideoInfo)VideoListView.SelectedItem;
            }
        }

        protected void OnPropertyChanged(string name)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(name));
            }
        }

        private void CloseClick(object sender, RoutedEventArgs e)
        {
            PlayingItem = null;
        }


        private Category searchCat = new Category() { Name = "Search" };
        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            var text = (sender as TextBox).Text;
            if (_videosView != null) 
            {
                FilterString = text;
                if (text == string.Empty)
                {
                    SelectedCategory = viewsTreeView.SelectedItem as Category;
                    return;
                }
    
                _videosView.Refresh();
            }

        }

        private void viewsTreeView_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            var cat = e.NewValue as Category;
            if(cat == null)
                return;

            SelectedCategory = cat;
        }

        private void Hyperlink_RequestNavigate(object sender, System.Windows.Navigation.RequestNavigateEventArgs e)
        {
            Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri));
            e.Handled = true;
        }

        private void Border_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            string folderPath = "";
            System.Windows.Forms.FolderBrowserDialog folderBrowserDialog1 = new System.Windows.Forms.FolderBrowserDialog();
            if (folderBrowserDialog1.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                folderPath = folderBrowserDialog1.SelectedPath;
                VlcLoad.retry(folderPath);
            }
        }

        private void VideoListView_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (VideoListView.SelectedItem != null)
            {
                PlayingItem = (VideoInfo)VideoListView.SelectedItem;
            }
        }
    }

    public static class VlcLoad
    {
        internal static bool debug = false;
        private static string vlcRegPath64 = @"SOFTWARE\Wow6432Node\VideoLAN\VLC";
        private static string vlcRegPath32 = @"SOFTWARE\VideoLAN\VLC";
        private static readonly DispatcherTimer _timer = new DispatcherTimer() { Interval = new TimeSpan(0, 0, 1) };


        private static EventHandler vlcInitialized;
        public static event EventHandler VlcInitialized
        {
            add
            {
                vlcInitialized += value;
                if (VlcContext.IsInitialized) {
                    value(null, null);
                }
            }

            remove
            {
                if (vlcInitialized == null || !vlcInitialized.GetInvocationList().Contains(value))
                    throw new InvalidOperationException("No Subs");
                vlcInitialized -= value;
            }
        }

        private static void InvokeVlcInitializedEvent()
        {
            if (vlcInitialized != null)
                vlcInitialized.Invoke(null, null);
        }

        static string checkVlcRegistries()
        {
            //64bit
            using (Microsoft.Win32.RegistryKey key = Registry.LocalMachine.OpenSubKey(vlcRegPath64))
            {
                if (key != null)
                    return key.GetValue("InstallDir") as string;
            }

            //32bit
            using (Microsoft.Win32.RegistryKey key = Registry.LocalMachine.OpenSubKey(vlcRegPath32))
            {
                if (key != null)
                    return key.GetValue("InstallDir") as string;
                else
                    return null;
            }
        }

        public static bool retry()
        {
            if (VlcContext.IsInitialized)
                return true;

            return Init(checkVlcRegistries());
        }

        public static bool retry(string path)
        {
            if (VlcContext.IsInitialized)
                return true;

            return Init(path);
        }

        static bool Init(string path)
        {
            if (path == null)
            {
                if(!_timer.IsEnabled)
                    _timer.Start();
                return false;
            }

            try
            {
                //Set libvlc.dll and libvlccore.dll directory path
                VlcContext.LibVlcDllsPath = path;
                //Set the vlc plugins directory path
                VlcContext.LibVlcPluginsPath = path + @"\plugins";

                if (debug)
                {
                    VlcContext.StartupOptions.LogOptions.LogInFile = true;
                    VlcContext.StartupOptions.LogOptions.ShowLoggerConsole = true;
                    VlcContext.StartupOptions.LogOptions.Verbosity = VlcLogVerbosities.Debug;
                }

                VlcContext.Initialize();
                InvokeVlcInitializedEvent();
                _timer.Stop();
                return true;
            }
            catch (Exception)
            {
                if (!_timer.IsEnabled)
                    _timer.Start();
                return false;
            }
        }

        static VlcLoad()
        {
            _timer.Tick += delegate(object o, EventArgs args)
            {
                retry();
            };
            Init(checkVlcRegistries());
        }
    }

    public class RandomMaxConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            var x = value.ToString();
            var seed = value.GetHashCode();
            var rand = generateRandomNumber(seed);
            return rand;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        private int generateRandomNumber(int seed)
        {
            Random random = new Random(seed);
            return random.Next(300, 700);
        }
    }

    public class VideoInfo : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public string Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string Icon { get; set; }
        public string Url { get; set; }
        public int Category { get; set; }
        public DateTime Timestamp { get; set; }

        protected void OnPropertyChanged(string name)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(name));
            }
        }

        public override string ToString()
        {
            return Title;
        }
    }

    public class ChildrenCountEventArgs : EventArgs
    {
        public int count { get; set; } 
    }

    public class Category : INotifyPropertyChanged
    {
        public Category Parent { get; set; }
        public event EventHandler<ChildrenCountEventArgs> ChildrenCountChanged;
        public event PropertyChangedEventHandler PropertyChanged;
        public int Id { get; set; }
        public string Name { get; set; }
        private List<Category> children;
        public List<Category> Children 
        {
            get { return children; }
            set 
            {
                children = value;
            } 
        }


        private int count = 0;
        private void incrementCount(Category parent)
        {
            if (Parent == null)
                return;

            parent.Count++;
        }

        public int Count
        {
            get { return count; }
            set
            {
                count = value;
                incrementCount(Parent);
                OnPropertyChanged("Count");
            }
        }

        public Category()
        {
            Children = new List<Category>();
        }
        protected void OnPropertyChanged(string name)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(name));
            }
        }

        public override string ToString()
        {
            return Name;
        }
    }

    public class IsNullConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (value == null);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new InvalidOperationException("IsNullConverter can only be used OneWay.");
        }
    } 
}