using CrypTool.CrypAnalysisViewControl;
using KeySearcher;
using System.Collections.ObjectModel;
using System.Windows;

namespace KeySearcherPresentation.Controls
{
    [CrypTool.PluginBase.Attributes.Localization("KeySearcher.Properties.Resources")]
    public partial class LocalQuickWatchPresentation
    {
        private KeySearcher.KeySearcher.UpdateOutput _updateOutputFromUserChoice;

        public KeySearcher.KeySearcher.UpdateOutput UpdateOutputFromUserChoice
        {
            get => _updateOutputFromUserChoice;
            set => _updateOutputFromUserChoice = value;
        }

        public ObservableCollection<ResultEntry> Entries { get; } = new ObservableCollection<ResultEntry>();

        private int amountOfDevices;
        public int AmountOfDevices
        {
            get => amountOfDevices;
            set
            {
                amountOfDevices = value;
                Devices.Value = amountOfDevices.ToString();
            }
        }

        public static readonly DependencyProperty IsOpenCLEnabledProperty =
            DependencyProperty.Register("IsOpenCLEnabled",
                typeof(bool),
                typeof(LocalQuickWatchPresentation), new PropertyMetadata(false));

        public bool IsOpenCLEnabled
        {
            get => (bool)GetValue(IsOpenCLEnabledProperty);
            set => SetValue(IsOpenCLEnabledProperty, value);
        }

        public LocalQuickWatchPresentation()
        {
            InitializeComponent();
            this.DataContext = this;
        }

        private void HandleResultItemAction(ICrypAnalysisResultListEntry item)
        {
            if (item is ResultEntry resultItem)
            {
                _updateOutputFromUserChoice(resultItem.Key, resultItem.FullText);
            }
        }
    }
}
