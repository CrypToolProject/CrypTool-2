using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using KeySearcher;
using KeySearcher.KeyPattern;

namespace KeySearcherPresentation.Controls
{
    /// <summary>
    /// This grid performs uniform resizing on a vertical stretch an uses normal behaviour on a horizontal stretch
    /// </summary>
    internal class GuttenbergGrid : Grid
    {
        private double scale;

        protected override Size MeasureOverride(Size availableSize)
        {
            if (!(availableSize.Height == double.PositiveInfinity || availableSize.Width == double.PositiveInfinity))
            {
                double height = 0;
                Size unlimitedSize = new Size(double.PositiveInfinity, double.PositiveInfinity);
                foreach (UIElement child in Children)
                {
                    child.Measure(unlimitedSize);
                    height += child.DesiredSize.Height;
                }

                scale = availableSize.Height/height;

                foreach (UIElement child in Children)
                {
                    unlimitedSize.Width = availableSize.Width/scale;
                    child.Measure(unlimitedSize);
                }

                return availableSize;
            }

            return DesiredSize;
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            if (scale != 0)
            {
                Transform scaleTransform = new ScaleTransform(scale, scale);
                double height = 0;
                foreach (UIElement child in Children)
                {
                    if ((child.DesiredSize.Height != 0) || (child.DesiredSize.Width != 0))
                    {
                        child.RenderTransform = scaleTransform;
                        child.Arrange(new Rect(new Point(0, scale*height),
                                               new Size(finalSize.Width/scale, child.DesiredSize.Height)));
                        height += child.DesiredSize.Height;
                    }
                }
            }
            return finalSize;
        }

    }

    /// <summary>
    /// Interaction logic for StatisticsPresentation.xaml
    /// </summary>
    [CrypTool.PluginBase.Attributes.Localization("KeySearcher.Properties.Resources")]
    public partial class StatisticsPresentation : UserControl
    {
        public StatisticsPresentation()
        {
            InitializeComponent();
            ((InformationToProgressConverter)Resources["InformationToProgressConverter"]).StatisticsPresentation = this;
            ((InformationToProgressConverter2)Resources["InformationToProgressConverter2"]).StatisticsPresentation = this;
            ((ChunkSumConverter)Resources["ChunkSumConverter"]).StatisticsPresentation = this;
            ((MachineSumToProgressConverter)Resources["MachineSumToProgressConverter"]).StatisticsPresentation = this;
            ((MaxDateConverter)Resources["MaxDateConverter"]).StatisticsPresentation = this;
            ((TimeConverter)Resources["TimeConverter"]).StatisticsPresentation = this;
            ((StringLengthConverter)Resources["StringLengthConverter"]).StatisticsPresentation = this;
            //---
            ((CurrTrueVisibleConverter1)Resources["CurrTrueVisibleConverter1"]).StatisticsPresentation = this;
            ((DateTrueVisibleConverter1)Resources["DateTrueVisibleConverter1"]).StatisticsPresentation = this;
            ((CurrTrueVisibleConverter2)Resources["CurrTrueVisibleConverter2"]).StatisticsPresentation = this;
            ((DateTrueVisibleConverter2)Resources["DateTrueVisibleConverter2"]).StatisticsPresentation = this;
            ((DateToColorConverter1)Resources["DateToColorConverter1"]).StatisticsPresentation = this;
            ((DateToColorConverter2)Resources["DateToColorConverter2"]).StatisticsPresentation = this;
            ((HideDeadMachineConverter)Resources["HideDeadMachineConverter"]).StatisticsPresentation = this;
            ((HideDeadUserConverter)Resources["HideDeadUserConverter"]).StatisticsPresentation = this;
            
        }

        private Dictionary<string, Dictionary<long, Information>> statistics = null;
        public Dictionary<string, Dictionary<long, Information>> Statistics
        {
            get { return statistics; }
            set
            {
                lock (this)
                {
                    statistics = value;
                }
                if (statistics != null)
                    Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                                               {
                                                   try
                                                   {
                                                       if (statistics != null)
                                                       {
                                                           statisticsTree.DataContext = statistics;
                                                           statisticsTree.Items.Refresh();
                                                       }
                                                   }
                                                   catch (Exception)
                                                   {    
                                                   }
                                               }, null);

            }
        }

        private Dictionary<long, MachInfo> machineHierarchy = null;
        public Dictionary<long, MachInfo> MachineHierarchy
        {
            get { return machineHierarchy; }
            set
            {
                lock (this)
                {
                    machineHierarchy = value;
                }
                Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                                                {
                                                    try
                                                    {
                                                        if (machineHierarchy != null)
                                                        {
                                                            machineTree.DataContext = machineHierarchy;
                                                            machineTree.Items.Refresh();
                                                        }
                                                    }
                                                    catch (Exception)
                                                    {
                                                    }
                                                }, null);
            }
        }

        #region Information

        private string days = "??? Days";
        public string Days
        {
            get { return days; }
            set
            {
                lock (this)
                {
                    days = value;
                }

                Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                {
                    if (days != null)
                        WorkingDays.Content = days + KeySearcher.Properties.Resources._days_;
                }, null);
            }
        }

        private DateTime updatetime;
        public DateTime UpdateTime
        {
            get { return updatetime; }
            set
            {
                lock (this)
                {
                    updatetime = value;
                }

                Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                {
                    if (updatetime != null)
                        LastUpdateTime.Content = updatetime.ToLocalTime().ToString("g");
                }, null);
            }
        }

        private string nextupdatetime = "-";
        public string NextUpdateTime
        {
            get { return nextupdatetime; }
            set
            {
                lock (this)
                {
                    nextupdatetime = value;
                }

                Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                {
                    if (days != null)
                        LastUpdateTime.ToolTip = KeySearcher.Properties.Resources._Next_Update_Time___ + nextupdatetime;
                    LastUpdateTimeText.ToolTip = KeySearcher.Properties.Resources._Next_Update_Time___ + nextupdatetime;
                }, null);
            }
        }

        private BigInteger totalBlocks = 0;
        public BigInteger TotalBlocks
        {
            get { return totalBlocks; }
            set
            {
                lock (this)
                {
                    totalBlocks = value;
                }

                Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                {
                    TotalAmountOfBlocks.Content = string.Format("{0:0,0}", totalBlocks);
                }, null);
            }
        }

        private BigInteger calculatedBlocks = 0;
        public BigInteger CalculatedBlocks
        {
            get { return calculatedBlocks; }
            set
            {
                lock (this)
                {
                    calculatedBlocks = value;
                }

                Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                {
                    TotalBlocksTested.Content = string.Format("{0:0,0}", calculatedBlocks);
                }, null);
            }
        }

        private BigInteger totalKeys = 0;
        public BigInteger TotalKeys
        {
            get { return totalKeys; }
            set
            {
                lock (this)
                {
                    totalKeys = value;
                }

                Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                {
                    TotalAmountOfKeys.Content = string.Format("{0:0,0}", totalKeys);
                }, null);
            }
        }

        private BigInteger calculatedKeys = 0;
        public BigInteger CalculatedKeys
        {
            get { return calculatedKeys; }
            set
            {
                lock (this)
                {
                    calculatedKeys = value;
                }

                Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                {
                    TotalKeysTested.Content = string.Format("{0:0,0}", calculatedKeys);
                }, null);
            }
        }

        private double percent = 0;
        public double Percent
        {
            get { return percent; }
            set
            {
                lock (this)
                {
                    if (totalBlocks != 0)
                    {
                        percent = Math.Round((value/(double) totalBlocks)*Math.Pow(10, totalKeys.ToString().Length)) / Math.Pow(10, totalKeys.ToString().Length-2);
                    }
                    else
                    {
                        percent = 0;
                    }
                }

                Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                {
                     PercentsComplete.Content = string.Format("{0:0."+ getCommaPlaces() +"} %", percent);
                }, null);
            }
        }

        private string getCommaPlaces()
        {
            var l = totalKeys.ToString().Length;

            if(l < 5)
            {
                return "####";
            }
            else if(l < 10)
            {
                return "########";
            }
            else
            {
                return "############";
            }           
        }

        private BigInteger users = 1;
        public BigInteger Users
        {
            get { return users; }
            set
            {
                lock (this)
                {
                    users = value;
                }

                Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                {
                    UserCount.Content = users;
                }, null);
            }
        }

        private BigInteger currentusers = 1;
        public BigInteger CurrentUsers
        {
            get { return currentusers; }
            set
            {
                lock (this)
                {
                    if (value > 0)
                    {
                        currentusers = value;
                    }
                }

                Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                {
                    CurrentUserCount.Content = currentusers;
                }, null);
            }
        }

        private string beeusers = "-";
        public string BeeUsers
        {
            get { return beeusers; }
            set
            {
                lock (this)
                {
                    beeusers = value;
                }

                Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                {
                    if (beeusers != null)
                    {
                        if(beeusers.Length < 40)
                        {
                            BestUser.Content = beeusers;
                            BestUser.ToolTip = beeusers;
                        }
                        else
                        {
                            BestUser.Content = string.Format("{0}...", beeusers.Substring(0, 37));
                            BestUser.ToolTip = beeusers;
                        }
                    }
                }, null);
            }
        }

        private BigInteger machines = 1;
        public BigInteger Machines
        {
            get { return machines; }
            set
            {
                lock (this)
                {
                    machines = value;
                }

                Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                {
                    MachineCount.Content = machines;
                }, null);
            }
        }

        private BigInteger currentmachines = 1;
        public BigInteger CurrentMachines
        {
            get { return currentmachines; }
            set
            {
                lock (this)
                {
                    if (value > 0)
                    {
                        currentmachines = value;
                    }
                }

                Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                {
                    CurrentMachineCount.Content = currentmachines;
                }, null);
            }
        }

        private string beemachines = "-";
        public string BeeMachines
        {
            get { return beemachines; }
            set
            {
                lock (this)
                {
                    beemachines = value;
                }

                Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                {
                    if (beemachines != null)
                    {
                        if (beemachines.Length < 40)
                        {
                            BestMachine.Content = beemachines;
                            BestMachine.ToolTip = beemachines;
                        }
                        else
                        {
                            BestMachine.Content = string.Format("{0}...", beemachines.Substring(0, 37));
                            BestMachine.ToolTip = beemachines;
                        }
                    }
                }, null);
            }
        }

        private BigInteger rate = 0;
        public BigInteger SetRate
        {
            get { return rate; }
            set
            {
                lock (this)
                {
                    rate = value;
                }

                Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                {
                    Rate.Content = string.Format(KeySearcher.Properties.Resources.StatisticsPresentation_SetCurrentRate, rate);
                }, null);
            }
        }

        private BigInteger currentrate = 0;
        public BigInteger SetCurrentRate
        {
            get { return currentrate; }
            set
            {
                lock (this)
                {
                    currentrate = value;
                }

                Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                {
                    CurrentRate.Content = string.Format(KeySearcher.Properties.Resources.StatisticsPresentation_SetCurrentRate, currentrate);
                }, null);
            }
        }

        #endregion

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Button b = sender as Button;

            if(statisticsTree.ItemContainerStyle == null)
            {
                b.Content = "Expand";
                statisticsTree.ItemContainerStyle = this.Resources["ItemStyle2"] as Style;
            }

            if (statisticsTree.ItemContainerStyle.Equals(this.Resources["ItemStyle"] as Style))
            {
                b.Content = "Expand";
                statisticsTree.ItemContainerStyle = this.Resources["ItemStyle2"] as Style;
                return;
            }

            if (statisticsTree.ItemContainerStyle.Equals(this.Resources["ItemStyle2"] as Style))
            {
                b.Content = "Collapse";
                statisticsTree.ItemContainerStyle = this.Resources["ItemStyle"] as Style;
                return;
            }
        }

        protected void Checked(object sender, RoutedEventArgs e)
        {
            Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
            {
                try
                {
                    if (statisticsTree != null)
                    {
                        if (statisticsTree.ItemContainerStyle == null)
                        {
                            statisticsTree.ItemContainerStyle = this.Resources["ItemStyle2"] as Style;
                        }
                        statisticsTree.Items.Refresh();
                    }        
                }
                catch (Exception)
                {
                }
            }, null);
        }

        protected void Checked2(object sender, RoutedEventArgs e)
        {
            Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
            {
                try
                {
                    if (machineTree != null)
                    {
                        machineTree.Items.Refresh();
                    }
                }
                catch (Exception)
                {
                }
            }, null);
        }


        private QuickWatch ParentQuickWatch
        {
            get { return (QuickWatch) ((Grid) ((Grid) Parent).Parent).Parent; }
        }

        private void SwitchView(object sender, RoutedEventArgs e)
        {
            ParentQuickWatch.ShowStatistics = false;
        }
    }

    #region Converters
    [ValueConversion(typeof(Dictionary<long, Information>), typeof(Double))]
    class InformationToProgressConverter : IValueConverter
    {
        public StatisticsPresentation StatisticsPresentation { get; set; }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                if (StatisticsPresentation != null && StatisticsPresentation.Statistics != null)
                {
                    lock (StatisticsPresentation)
                    {
                        double allCount = (StatisticsPresentation.Statistics).Sum(i => i.Value.Sum(j => j.Value.Count));
                        double vCount = ((Dictionary<long, Information>)value).Sum(i => i.Value.Count);
                        return vCount / allCount;
                    }
                }
            }
            catch (Exception)
            {
            }
            return 0.0;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    [ValueConversion(typeof(Dictionary<long, Information>), typeof(Double))]
    class MachineSumToProgressConverter : IValueConverter
    {
        public StatisticsPresentation StatisticsPresentation { get; set; }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                if (StatisticsPresentation != null && StatisticsPresentation.MachineHierarchy != null)
                {
                    lock (StatisticsPresentation)
                    {
                        double allCount = (StatisticsPresentation.MachineHierarchy).Sum(i => i.Value.Sum);
                        double vCount = (int)value;
                        return vCount / allCount;
                    }
                }
            }
            catch (Exception)
            {
            }
            return 0.0;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    [ValueConversion(typeof(string), typeof(Double))]
    class ChunkSumConverter : IValueConverter
    {
        public StatisticsPresentation StatisticsPresentation { get; set; }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                if (StatisticsPresentation != null && StatisticsPresentation.Statistics != null)
                {
                    lock (StatisticsPresentation)
                    {
                        string key = (string)value;
                        var data = (StatisticsPresentation.Statistics)[key];
                        return data.Sum(i => i.Value.Count);
                    }
                }
            }
            catch (Exception)
            {
            }
            return 0.0;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    [ValueConversion(typeof(string), typeof(string))]
    class StringLengthConverter : IValueConverter
    {
        public StatisticsPresentation StatisticsPresentation { get; set; }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value != null && value is string)
            {
                string name = (string)value;
                if (name.Length < 13)
                {
                    return name;
                }
                return string.Format("{0}...", name.Substring(0, 9));
            }
            return "";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    [ValueConversion(typeof(Int32), typeof(Double))]
    class InformationToProgressConverter2 : IValueConverter
    {
        public StatisticsPresentation StatisticsPresentation { get; set; }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                if (StatisticsPresentation != null && StatisticsPresentation.Statistics != null)
                {
                    lock (StatisticsPresentation)
                    {
                        double allCount = (StatisticsPresentation.Statistics).Sum(i => i.Value.Sum(j => j.Value.Count));
                        return (int)value / allCount;
                    }
                }
            }
            catch (Exception)
            {
            }
            return 0.0;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    [ValueConversion(typeof(DateTime), typeof(String))]
    class MaxDateConverter : IValueConverter
    {
        public StatisticsPresentation StatisticsPresentation { get; set; }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                if (StatisticsPresentation != null && StatisticsPresentation.Statistics != null)
                {
                    lock (StatisticsPresentation)
                    {
                        var max = DateTime.MinValue;
                        var machines = StatisticsPresentation.Statistics[(string)value];
                        foreach (var id in machines.Keys.Where(id => machines[id].Date > max))
                        {
                            max = machines[id].Date;
                        }
                        return max.ToLocalTime().ToString("g");
                    }
                }
            }
            catch (Exception)
            {
            }
            return DateTime.UtcNow.ToLocalTime().ToString("g");
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    [ValueConversion(typeof(DateTime), typeof(String))]
    class TimeConverter : IValueConverter
    {
        public StatisticsPresentation StatisticsPresentation { get; set; }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                if (StatisticsPresentation != null && StatisticsPresentation.Statistics != null)
                {
                    lock (StatisticsPresentation)
                    {

                        return ((DateTime)value).ToLocalTime().ToString("g");
                    }
                }
            }
            catch (Exception)
            {
            }
            return DateTime.UtcNow.ToLocalTime().ToString("g");
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    [ValueConversion(typeof(DateTime), typeof(String))]
    class DateToColorConverter1 : IValueConverter
    {
        public StatisticsPresentation StatisticsPresentation { get; set; }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                if (StatisticsPresentation != null && StatisticsPresentation.Statistics != null)
                {
                    lock (StatisticsPresentation)
                    {
                        var max = DateTime.MinValue;
                        var machines = StatisticsPresentation.Statistics[(string)value];
                        foreach (var id in machines.Keys.Where(id => machines[id].Date > max))
                        {
                            max = machines[id].Date;
                        }

                        TimeSpan diff = max > StatisticsPresentation.UpdateTime ? max.Subtract(StatisticsPresentation.UpdateTime) : StatisticsPresentation.UpdateTime.Subtract(max);

                        var minutes = diff.TotalMinutes;

                        int g = 255;
                        var r = Math.Round(minutes / 2.5);

                        if (r > 255)
                        {
                            r = 255;
                            g = (int)(-1 * (Math.Round(minutes / 2.5)) + 255);
                        }

                        if (g < 0) g = 0;
                       
                        if(minutes > 2880)
                        {
                            Color cblack = Color.FromRgb((byte) 0, (byte) 0, (byte) 0);
                            return cblack.ToString();
                        }
                        
                        Color c = Color.FromRgb((byte) r, (byte) g, (byte) 0);
                        return c.ToString();
                    }
                }
            }
            catch (Exception)
            {
            }
            return Color.FromRgb(0, 0, 0).ToString();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    [ValueConversion(typeof(DateTime), typeof(String))]
    class DateToColorConverter2 : IValueConverter
    {
        public StatisticsPresentation StatisticsPresentation { get; set; }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                if (StatisticsPresentation != null && StatisticsPresentation.Statistics != null)
                {
                    lock (StatisticsPresentation)
                    {
                        DateTime date = (DateTime)value;
                        TimeSpan diff = date > StatisticsPresentation.UpdateTime ? date.Subtract(StatisticsPresentation.UpdateTime) : StatisticsPresentation.UpdateTime.Subtract(date);

                        var minutes = diff.TotalMinutes;

                        int g = 255;
                        var r = Math.Round(minutes / 2.5);

                        if (r > 255)
                        {
                            r = 255;
                            g = (int)(-1 * (Math.Round(minutes / 2.5)) + 255);
                        }

                        if (g < 0) g = 0;

                        if (minutes > 2880)
                        {
                            Color cblack = Color.FromRgb((byte)0, (byte)0, (byte)0);
                            return cblack.ToString();
                        }

                        Color c = Color.FromRgb((byte)r, (byte)g, 0);
                        return c.ToString();
                    }
                }
            }
            catch (Exception)
            {
            }
            return Color.FromRgb(0, 0, 0).ToString();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    [ValueConversion(typeof(Boolean), typeof(Visibility))]
    class CurrTrueVisibleConverter1 : IValueConverter
    {
        public StatisticsPresentation StatisticsPresentation { get; set; }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                if (StatisticsPresentation != null && StatisticsPresentation.Statistics != null)
                {
                    lock (StatisticsPresentation)
                    {
                        var curr = false;
                        var machines = StatisticsPresentation.Statistics[(string)value];
                        foreach (var id in machines.Keys.Where(id => machines[id].Current))
                        {
                            curr = true;
                        }
                        
                        if (targetType != typeof(Visibility))
                            throw new InvalidOperationException("The target must be of Visibility");

                        if (!curr) //dissapear if not current
                        {
                            return Visibility.Hidden;
                        }

                        return Visibility.Visible;
                    }
                }
            }
            catch (Exception)
            {
            }
            return Visibility.Hidden;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    [ValueConversion(typeof(Boolean), typeof(Visibility))]
    class DateTrueVisibleConverter1 : IValueConverter
    {
        public StatisticsPresentation StatisticsPresentation { get; set; }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                if (StatisticsPresentation != null && StatisticsPresentation.Statistics != null)
                {
                    lock (StatisticsPresentation)
                    {
                        var dead = true;
                        var machines = StatisticsPresentation.Statistics[(string)value];
                        foreach (var id in machines.Keys.Where(id => machines[id].Dead == false))
                        {
                            dead = false;
                        }

                        if (targetType != typeof(Visibility))
                            throw new InvalidOperationException("The target must be of Visibility");

                        if (dead) //after two days X
                        {
                            return Visibility.Visible;
                        }

                        return Visibility.Hidden;
                    }
                }
            }
            catch (Exception)
            {
            }
            return Visibility.Hidden;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    [ValueConversion(typeof(Boolean), typeof(Visibility))]
    class CurrTrueVisibleConverter2 : IValueConverter
    {
        public StatisticsPresentation StatisticsPresentation { get; set; }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                if (StatisticsPresentation != null && StatisticsPresentation.Statistics != null)
                {
                    lock (StatisticsPresentation)
                    {
                        var curr = (bool)value;

                        if (targetType != typeof(Visibility))
                            throw new InvalidOperationException("The target must be of Visibility");

                        if (!curr) //dissapear if not current
                        {
                            return Visibility.Hidden;
                        }

                        return Visibility.Visible;
                    }
                }
            }
            catch (Exception)
            {
            }
            return Visibility.Hidden;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    [ValueConversion(typeof(Boolean), typeof(Visibility))]
    class DateTrueVisibleConverter2 : IValueConverter
    {
        public StatisticsPresentation StatisticsPresentation { get; set; }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                if (StatisticsPresentation != null && StatisticsPresentation.Statistics != null)
                {
                    lock (StatisticsPresentation)
                    {
                        var dead = (bool)value;

                        if (targetType != typeof(Visibility))
                            throw new InvalidOperationException("The target must be of Visibility");

                        if (dead) //after two days X
                        {
                            return Visibility.Visible;
                        }

                        return Visibility.Hidden;
                    }
                }
            }
            catch (Exception)
            {
            }
            return Visibility.Hidden;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    [ValueConversion(typeof(Boolean), typeof(Visibility))]
    class HideDeadMachineConverter : IValueConverter
    {
        public StatisticsPresentation StatisticsPresentation { get; set; }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                    if (StatisticsPresentation != null && StatisticsPresentation.Statistics != null)
                    {
                        lock (StatisticsPresentation)
                        {
                            if (StatisticsPresentation.HideDead2.IsChecked == true)
                            {
                                var dead = (bool) value;

                                if (targetType != typeof (Visibility))
                                    throw new InvalidOperationException("The target must be of Visibility");

                                if (dead) //after two days X
                                {
                                    return Visibility.Collapsed;
                                }

                                return Visibility.Visible;
                            }

                            return Visibility.Visible;
                        }
                    }
            }
            catch (Exception)
            {
            }
            return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    [ValueConversion(typeof(String), typeof(Visibility))]
    class HideDeadUserConverter : IValueConverter
    {
        public StatisticsPresentation StatisticsPresentation { get; set; }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                if (StatisticsPresentation != null && StatisticsPresentation.Statistics != null)
                {
                    lock (StatisticsPresentation)
                    {
                        if (StatisticsPresentation.HideDead.IsChecked == true)
                        {
                            var key = value.ToString();

                            if (StatisticsPresentation.Statistics.ContainsKey(key))
                            {
                                var dead = true;
                                var machines = StatisticsPresentation.Statistics[(string) key];
                                foreach (var id in machines.Keys.Where(id => machines[id].Dead == false))
                                {
                                    dead = false;
                                }

                                if (targetType != typeof (Visibility))
                                    throw new InvalidOperationException("The target must be of Visibility");

                                if (dead) //after two days X
                                {
                                    return Visibility.Collapsed;
                                }
                            }
                            return Visibility.Visible;
                            
                        }

                        return Visibility.Visible;
                    }
                }
            }
            catch (Exception)
            {
            }
            return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class ColorToDateConverter : IMultiValueConverter
    {
        public static SolidColorBrush[] AlternationColors = {Brushes.LimeGreen, Brushes.Red, Brushes.Blue};

        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                SolidColorBrush brush = ColorToDateConverter.AlternationColors[(int)values[1]].Clone();
                return brush;             
            }
            catch (Exception)
            {
            }

            return Brushes.AntiqueWhite;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
    #endregion

}
