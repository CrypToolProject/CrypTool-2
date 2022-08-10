/*                              
   Copyright 2010-2022 Nils Kopal, Viktor M.

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
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media.Animation;
using System.Windows.Threading;
using CrypTool.PluginBase;
using WorkspaceManager.Model;

namespace WorkspaceManager.View.Visuals
{
    /// <summary>
    /// Interaction logic for BinLogNotifier.xaml
    /// </summary>
    public partial class LogNotifierVisual : UserControl, INotifyPropertyChanged
    {
        #region Events
        public event EventHandler<ErrorMessagesOccuredArgs> ErrorMessagesOccured;
        public event PropertyChangedEventHandler PropertyChanged;
        #endregion

        #region Properties
        public ComponentVisual Parent { get; private set; }
        public BinComponentState CurrentState { get; private set; }
        public BinComponentState LastState { get; private set; }
        #endregion

        #region private vars
        private readonly Queue<Log> logStack = new Queue<Log>();
        private readonly ObservableCollection<Log> logsTillReset = new ObservableCollection<Log>();
        private readonly DispatcherTimer timer = new DispatcherTimer() { Interval = new TimeSpan(0, 0, 5) };
        #endregion

        #region DependencyProperties

        private Log currentLog;
        public Log CurrentLog
        {
            get => currentLog;
            set { currentLog = value; OnPropertyChanged("CurrentLog"); }
        }

        public int ErrorCount => logsTillReset.Count(a => a.Level == NotificationLevel.Error);

        public int WarningCount => logsTillReset.Count(a => a.Level == NotificationLevel.Warning);

        public int InfoCount => logsTillReset.Count(a => a.Level == NotificationLevel.Info);
        #endregion

        #region Constructor
        public LogNotifierVisual(ObservableCollection<Log> Logs, ComponentVisual Parent)
        {
            this.Parent = Parent;
            new ObservableCollection<Log>();
            CurrentState = Parent.State;
            logsTillReset.CollectionChanged += new System.Collections.Specialized.NotifyCollectionChangedEventHandler(CollectionChangedHandler);
            this.Parent.StateChanged += new EventHandler<VisualStateChangedArgs>(StateChangedHandler);
            this.InitializeComponent();
            timer.Tick += new EventHandler(TickHandler);
            timer.Start();
            Logs.CollectionChanged += new System.Collections.Specialized.NotifyCollectionChangedEventHandler(LogCollectionChangedHandler);
        }

        private void StateChangedHandler(object sender, VisualStateChangedArgs e)
        {
            LastState = CurrentState;
            CurrentState = e.State;
            if (LastState == BinComponentState.Log && CurrentState != BinComponentState.Log)
            {
                logsTillReset.Clear();
            }
        }

        private void CollectionChangedHandler(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            OnPropertyChanged("WarningCount");
            OnPropertyChanged("InfoCount");
            OnPropertyChanged("ErrorCount");
        }
        #endregion

        protected override void OnVisualParentChanged(DependencyObject oldParent)
        {
            base.OnVisualParentChanged(oldParent);
            DataContext = this;
        }

        public void Reset()
        {
            logsTillReset.Clear();
        }

        protected void OnPropertyChanged(string name)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(name));
            }
        }

        #region Event Handler
        private void TickHandler(object sender, EventArgs e)
        {
            if (logStack.Count == 0)
            {
                timer.Stop();
                CurrentLog = null;
                return;
            }

            CurrentLog = logStack.Dequeue();
        }

        private void LogCollectionChangedHandler(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Add)
            {
                Log log = (Log)e.NewItems[0];
                if (log.Level != NotificationLevel.Warning &&
                    log.Level != NotificationLevel.Error &
                    log.Level != NotificationLevel.Info)
                {
                    return;
                }

                if (Parent.State == BinComponentState.Log)
                {
                    return;
                }

                logStack.Enqueue(log);
                logsTillReset.Add(log);

                if (!timer.IsEnabled)
                {
                    CurrentLog = logStack.Dequeue();
                    timer.Start();
                }

                if (log.Level == NotificationLevel.Error && ErrorMessagesOccured != null)
                {
                    ErrorMessagesOccured.Invoke(this, new ErrorMessagesOccuredArgs() { HasErrors = true });
                }
            }

#warning if something gets deleted this means all messages has been deleted
            if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Reset)
            {
                logStack.Clear();
                logsTillReset.Clear();
                ErrorMessagesOccured.Invoke(this, new ErrorMessagesOccuredArgs() { HasErrors = false });
            }
        }
        #endregion

        private void MouseLeftButtonDownHandler(object sender, MouseButtonEventArgs e)
        {
            LogBorder border = sender as LogBorder;
            if (border != null)
            {
                Parent.State = BinComponentState.Log;

                UIElement element = Parent.GetPresentationElement(BinComponentState.Log);
                LogVisual logVisual = (LogVisual)element;
                logVisual.SelectedLogs = logsTillReset.Where(a => a.Level == border.NotificationLevel);
            }
        }
    }

    #region Custom Classes

    public class NotifierStackpanel : StackPanel
    {
        //private Log lastLog;
        //private Storyboard storyBoard;
        //private DoubleAnimation plusWidth;
        //private DoubleAnimation minusWidth;

        //public static readonly DependencyProperty CurrentLogProperty = DependencyProperty.Register("CurrentLog",
        //    typeof(Log), typeof(NotifierStackpanel), new FrameworkPropertyMetadata(null, OnCurrentLogChanged));

        //public Log CurrentLog
        //{
        //    get
        //    {
        //        return (Log)base.GetValue(CurrentLogProperty);
        //    }
        //    set
        //    {
        //        base.SetValue(CurrentLogProperty, value);
        //    }
        //}

        //private static void OnCurrentLogChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        //{
        //    NotifierStackpanel s = (NotifierStackpanel)d;
        //    Log newLog = (Log)e.NewValue;
        //    Log oldLog = (Log)e.OldValue;

        //    if (newLog != null)
        //    {
        //        IEnumerable<LogBorder> filter = s.Children.OfType<LogBorder>();
        //        foreach (LogBorder element in filter)
        //        {
        //            if ((s.lastLog == null && s.CurrentLog == null) || (s.lastLog == s.CurrentLog))
        //                break;

        //            if ((s.CurrentLog.Level == NotificationLevel.Info && element.NotificationLevel == NotificationLevel.Info) ||
        //                (s.CurrentLog.Level == NotificationLevel.Error && element.NotificationLevel == NotificationLevel.Error) ||
        //                (s.CurrentLog.Level == NotificationLevel.Warning && element.NotificationLevel == NotificationLevel.Warning))
        //            {
        //                int i = s.Children.IndexOf(element), x;
        //                x = (i + filter.Count() - 1 % filter.Count()) * 16;
        //                GeneralTransform gTransform;
        //                Point point;
        //                gTransform = element.TransformToVisual(s);
        //                point = gTransform.Transform(new Point(0, 0));
        //                s.plusWidth.To = s.ActualWidth - point.X - x;

        //                Storyboard.SetTargetProperty(s.storyBoard, new PropertyPath(FrameworkElement.WidthProperty));
        //                Storyboard.SetTarget(s.storyBoard, element);
        //                s.storyBoard.Begin();
        //            }
        //        }
        //    }
        //}

        //public NotifierStackpanel() : base()
        //{
        //    storyBoard = new Storyboard();
        //    plusWidth = new DoubleAnimation();
        //    minusWidth = new DoubleAnimation();
        //    plusWidth.Duration = minusWidth.Duration = new Duration(new TimeSpan(0,0,1));
        //    minusWidth.BeginTime = new TimeSpan(0, 0, 6);
        //    minusWidth.To = 16;
        //    Timeline.SetDesiredFrameRate(plusWidth, 5);
        //    Timeline.SetDesiredFrameRate(minusWidth, 5);
        //    storyBoard.Children.Add(minusWidth);
        //    storyBoard.Children.Add(plusWidth);
        //}
    }

    internal class GridLengthAnimation : AnimationTimeline
    {
        public GridLengthAnimation()
            : base()
        {
            Duration = new Duration(new TimeSpan(0, 0, 10));
        }

        static GridLengthAnimation()
        {
            FromProperty = DependencyProperty.Register("From", typeof(GridLength),
                typeof(GridLengthAnimation));

            ToProperty = DependencyProperty.Register("To", typeof(GridLength),
                typeof(GridLengthAnimation));
        }

        public override Type TargetPropertyType => typeof(GridLength);

        protected override System.Windows.Freezable CreateInstanceCore()
        {
            return new GridLengthAnimation();
        }

        public static readonly DependencyProperty FromProperty;
        public GridLength From
        {
            get => (GridLength)GetValue(GridLengthAnimation.FromProperty);
            set => SetValue(GridLengthAnimation.FromProperty, value);
        }

        public static readonly DependencyProperty ToProperty;
        public GridLength To
        {
            get => (GridLength)GetValue(GridLengthAnimation.ToProperty);
            set => SetValue(GridLengthAnimation.ToProperty, value);
        }

        public override object GetCurrentValue(object defaultOriginValue, object defaultDestinationValue, AnimationClock animationClock)
        {
            double fromVal = ((GridLength)GetValue(GridLengthAnimation.FromProperty)).Value;
            double toVal = ((GridLength)GetValue(GridLengthAnimation.ToProperty)).Value;

            if (fromVal > toVal)
            {
                return new GridLength((1 - animationClock.CurrentProgress.Value) * (fromVal - toVal) + toVal, GridUnitType.Star);
            }
            else
            {
                return new GridLength(animationClock.CurrentProgress.Value * (toVal - fromVal) + fromVal, GridUnitType.Star);
            }
        }
    }

    public class LogBorder : Border
    {
        public static readonly DependencyProperty NotificationLevelProperty = DependencyProperty.Register("NotificationLevel",
            typeof(NotificationLevel), typeof(LogBorder), new FrameworkPropertyMetadata(null, null));

        public NotificationLevel NotificationLevel
        {
            get => (NotificationLevel)base.GetValue(NotificationLevelProperty);
            set => base.SetValue(NotificationLevelProperty, value);
        }
    }

    #endregion

    #region Converter

    public class IsNotConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            BinComponentState item = (BinComponentState)value;
            if (item != BinComponentState.Min)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class IsGreaterConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (double.TryParse(value.ToString(), out double x))
            {
                if (x > 0)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            return false;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
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
    #endregion

    #region EventArgs

    public class ErrorMessagesOccuredArgs : EventArgs
    {
        public bool HasErrors { get; set; }
    }

    #endregion
}
