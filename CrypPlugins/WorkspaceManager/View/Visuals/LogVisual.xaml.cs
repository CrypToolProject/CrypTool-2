using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Collections.ObjectModel;
using CrypTool.PluginBase;
using System.ComponentModel;
using System.Windows.Controls.Primitives;

namespace WorkspaceManager.View.Visuals
{
    /// <summary>
    /// Interaction logic for LogPresentation.xaml
    /// </summary>
    [CrypTool.PluginBase.Attributes.Localization("WorkspaceManager.Properties.Resources")]
    public partial class LogVisual : UserControl, INotifyPropertyChanged
    {
        #region events
        public event PropertyChangedEventHandler PropertyChanged;
        #endregion

        #region Fields
        private HashSet<NotificationLevel> filter = new HashSet<NotificationLevel>();
        #endregion

        #region DependencyProperties
        public static readonly DependencyProperty LogMessagesProperty = DependencyProperty.Register("LogMessages",
            typeof(ObservableCollection<Log>), typeof(LogVisual), new FrameworkPropertyMetadata(null, new PropertyChangedCallback(OnLogMessagesChanged)));

        public ObservableCollection<Log> LogMessages
        {
            get { return (ObservableCollection<Log>)base.GetValue(LogMessagesProperty); }
            set { base.SetValue(LogMessagesProperty, value); }
        }
        #endregion

        #region Properties

        private IEnumerable<Log> selectedLogs;
        public IEnumerable<Log> SelectedLogs
        {
            get
            {
                return selectedLogs;
            }
            set
            {
                selectedLogs = value;
                if (selectedLogs != null)
                {
                    LogList.SelectedItems.Clear();
                    foreach (var element in selectedLogs)
                    {
                        LogList.SelectedItems.Add(element);
                    }
                }
            }
        }

        public int ErrorCount
        {
            get
            {
                if (LogMessages == null)
                    return 0;

                return LogMessages.Where(a => a.Level == NotificationLevel.Error).Count();
            }
        }

        public int DebugCount
        {
            get
            {
                if (LogMessages == null)
                    return 0;

                return LogMessages.Where(a => a.Level == NotificationLevel.Debug).Count();
            }
        }

        public int InfoCount
        {
            get
            {
                if (LogMessages == null)
                    return 0;

                return LogMessages.Where(a => a.Level == NotificationLevel.Info).Count();
            }
        }

        public int WarningCount
        {
            get
            {
                if (LogMessages == null)
                    return 0;

                return LogMessages.Where(a => a.Level == NotificationLevel.Warning).Count();
            }
        }

        public ComponentVisual Parent { get; private set; }
        #endregion

        #region constructors
        public LogVisual(ComponentVisual Parent)
        {
            this.Parent = Parent;
            SetBinding(LogVisual.LogMessagesProperty, new Binding() { Source = Parent.LogMessages });
            filter.Add(NotificationLevel.Error);
            filter.Add(NotificationLevel.Warning);
            filter.Add(NotificationLevel.Info);
            ICollectionView view = CollectionViewSource.GetDefaultView(LogMessages);
            view.Filter = new Predicate<object>(FilterCallback);
            InitializeComponent();
        }
        #endregion

        #region protected

        protected void OnPropertyChanged(string name)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
                handler(this, new PropertyChangedEventArgs(name));
        }
        #endregion

        #region Handler

        private bool FilterCallback(object item)
        {
            Log log = (Log)item;
            return filter.Contains(log.Level);
        }

        private void FilteringHandler(object sender, RoutedEventArgs e)
        {
            ToggleButton b = sender as ToggleButton;
            if (sender == null)
                return;

            ICollectionView view = CollectionViewSource.GetDefaultView(LogMessages);
            if (b.IsChecked == true) filter.Add((NotificationLevel)b.Content); else filter.Remove((NotificationLevel)b.Content);
            view.Filter = new Predicate<object>(FilterCallback);
        }

        private void ButtonDeleteMessages_Click(object sender, RoutedEventArgs e)
        {
            LogMessages.Clear();
            //Parent.LogMessages
        }

        private static void OnLogMessagesChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            LogVisual l = (LogVisual)d;
            ObservableCollection<Log> newCollection = (ObservableCollection<Log>)e.NewValue;
            ObservableCollection<Log> oldCollection = (ObservableCollection<Log>)e.OldValue;

            if (newCollection != null)
                newCollection.CollectionChanged += new System.Collections.Specialized.NotifyCollectionChangedEventHandler(l.LogCollectionChangedHandler);

            if (oldCollection != null)
                newCollection.CollectionChanged -= new System.Collections.Specialized.NotifyCollectionChangedEventHandler(l.LogCollectionChangedHandler);
        }

        private void LogCollectionChangedHandler(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            //1.Event 2.Data 3. ??? 4.Profit
            OnPropertyChanged("ErrorCount");
            OnPropertyChanged("DebugCount");
            OnPropertyChanged("InfoCount");
            OnPropertyChanged("WarningCount");
            Scroll.ScrollToEnd();
        }
        #endregion

        private void SelectionChangedHandler(object sender, SelectionChangedEventArgs e)
        {
        
        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {

        }
    }

    #region Custom Class
    public class CustomToggleButton : ToggleButton
    {
        public static readonly DependencyProperty TextProperty = DependencyProperty.Register("Text",
            typeof(string), typeof(CustomToggleButton), new FrameworkPropertyMetadata(string.Empty, null));

        public string Text
        {
            get { return (string)base.GetValue(TextProperty); }
            set { base.SetValue(TextProperty, value); }
        }

        public static readonly DependencyProperty CountProperty = DependencyProperty.Register("Count",
            typeof(int), typeof(CustomToggleButton), new FrameworkPropertyMetadata(0, null));

        public int Count
        {
            get { return (int)base.GetValue(CountProperty); }
            set { base.SetValue(CountProperty, value); }
        }

        public static readonly DependencyProperty CheckedTrueToolTipProperty = DependencyProperty.Register("CheckedTrueToolTip",
            typeof(string), typeof(CustomToggleButton), new FrameworkPropertyMetadata(string.Empty, null));

        public string CheckedTrueToolTip
        {
            get { return (string)base.GetValue(CheckedTrueToolTipProperty); }
            set { base.SetValue(CheckedTrueToolTipProperty, value); }
        }

        public static readonly DependencyProperty CheckedFalseToolTipProperty = DependencyProperty.Register("CheckedFalseToolTip",
            typeof(string), typeof(CustomToggleButton), new FrameworkPropertyMetadata(string.Empty, null));

        public string CheckedFalseToolTip
        {
            get { return (string)base.GetValue(CheckedFalseToolTipProperty); }
            set { base.SetValue(CheckedFalseToolTipProperty, value); }
        }
    }
    #endregion

    #region Converter

    #endregion
}
