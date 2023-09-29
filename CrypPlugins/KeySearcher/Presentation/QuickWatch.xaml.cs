using CrypTool.PluginBase.Attributes;
using KeySearcherPresentation.Controls;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;

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
                                            bool),
                                        typeof(
                                            QuickWatch), new PropertyMetadata(false));

        public bool IsP2PEnabled
        {
            get => (bool)GetValue(IsP2PEnabledProperty);
            set => SetValue(IsP2PEnabledProperty, value);
        }      

        public bool ShowStatistics
        {
            get => (bool)GetValue(ShowStatisticsProperty);
            set => SetValue(ShowStatisticsProperty, value);
        }

        public static DependencyProperty ShowStatisticsProperty =
            DependencyProperty.Register("ShowStatistics",
                                        typeof(
                                            bool),
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
