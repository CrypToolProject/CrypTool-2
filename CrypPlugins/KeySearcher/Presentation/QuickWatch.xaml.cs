using System;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using CrypTool.PluginBase.Attributes;
using KeySearcherPresentation.Controls;

namespace KeySearcherPresentation
{
    /// <summary>
    /// Interaction logic for QuickWatch.xaml
    /// </summary>
    [TabColor("pink")]
    [CrypTool.PluginBase.Attributes.Localization("KeySearcher.Properties.Resources")]
    public partial class QuickWatch : UserControl
    {
        public static readonly DependencyProperty IsP2PEnabledProperty =
            DependencyProperty.Register("IsP2PEnabled",
                                        typeof(
                                            Boolean),
                                        typeof(
                                            QuickWatch), new PropertyMetadata(false));

        public Boolean IsP2PEnabled
        {
            get { return (Boolean)GetValue(IsP2PEnabledProperty); }
            set { SetValue(IsP2PEnabledProperty, value); }
        }

        public static readonly DependencyProperty IsOpenCLEnabledProperty =
            DependencyProperty.Register("IsOpenCLEnabled",
                                typeof(
                                    Boolean),
                                typeof(
                                    QuickWatch), new PropertyMetadata(false));

        public Boolean IsOpenCLEnabled
        {
            get { return (Boolean)GetValue(IsOpenCLEnabledProperty); }
            set
            {
                SetValue(IsOpenCLEnabledProperty, value);
                LocalQuickWatchPresentation.IsOpenCLEnabled = value;
            }
        }

        public Boolean ShowStatistics
        {
            get { return (Boolean)GetValue(ShowStatisticsProperty); }
            set { SetValue(ShowStatisticsProperty, value); }
        }

        public static DependencyProperty ShowStatisticsProperty =
            DependencyProperty.Register("ShowStatistics",
                                        typeof(
                                            Boolean),
                                        typeof(
                                            QuickWatch), new PropertyMetadata(false));

        public CultureInfo CurrentCulture { get; private set; }

        public QuickWatch()
        {
            InitializeComponent();

            CurrentCulture = CultureInfo.CurrentCulture;
        }
    }
}
