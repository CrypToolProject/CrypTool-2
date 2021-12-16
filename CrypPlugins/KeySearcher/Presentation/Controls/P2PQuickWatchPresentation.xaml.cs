using CrypTool.CrypAnalysisViewControl;
using KeySearcher;
using KeySearcher.CrypCloud;
using System;
using System.Globalization;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace KeySearcherPresentation.Controls
{
    [CrypTool.PluginBase.Attributes.Localization("KeySearcher.Properties.Resources")]
    public partial class P2PQuickWatchPresentation : UserControl
    {
        public KeySearcher.KeySearcher.UpdateOutput UpdateOutputFromUserChoice { get; set; }

        public static readonly DependencyProperty IsVerboseEnabledProperty = DependencyProperty.Register("IsVerboseEnabled", typeof(bool), typeof(P2PQuickWatchPresentation), new PropertyMetadata(false));
        public bool IsVerboseEnabled
        {
            get => (bool)GetValue(IsVerboseEnabledProperty);
            set => SetValue(IsVerboseEnabledProperty, value);
        }

        public NumberFormatInfo nfi = (NumberFormatInfo)CultureInfo.InvariantCulture.NumberFormat.Clone();

        public TaskFactory UiContext { get; set; }
        public P2PPresentationVM ViewModel { get; set; }

        public P2PQuickWatchPresentation()
        {
            InitializeComponent();
            ViewModel = DataContext as P2PPresentationVM;
            try
            {
                UiContext = new TaskFactory(TaskScheduler.FromCurrentSynchronizationContext());
            }
            catch (Exception)
            {
                //do nothing
            }
            ViewModel.UiContext = UiContext;
        }

        public void UpdateSettings(KeySearcher.KeySearcher keySearcher, KeySearcherSettings keySearcherSettings)
        {
            ViewModel.UpdateSettings(keySearcher, keySearcherSettings);
        }

        private void HandleResultItemAction(ICrypAnalysisResultListEntry item)
        {
            if (item is KeyResultEntry resultItem)
            {
                UpdateOutputFromUserChoice(resultItem.Key, resultItem.Plaintext);
            }
        }
    }
}
