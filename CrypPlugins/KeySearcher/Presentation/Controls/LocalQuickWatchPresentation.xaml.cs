using System;
using System.Windows;
using System.Windows.Controls;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using KeySearcher;
using CrypTool.CrypAnalysisViewControl;

namespace KeySearcherPresentation.Controls
{
    [CrypTool.PluginBase.Attributes.Localization("KeySearcher.Properties.Resources")]
    public partial class LocalQuickWatchPresentation
    {
        private KeySearcher.KeySearcher.UpdateOutput _updateOutputFromUserChoice;

        public KeySearcher.KeySearcher.UpdateOutput UpdateOutputFromUserChoice
        {
            get { return _updateOutputFromUserChoice; }
            set { _updateOutputFromUserChoice = value; }
        }

        public ObservableCollection<ResultEntry> Entries { get; } = new ObservableCollection<ResultEntry>();

        private int amountOfDevices;
        public int AmountOfDevices
        {
            get { return amountOfDevices; }
            set
            {
                amountOfDevices = value;
                Devices.Value = amountOfDevices.ToString();
            }
        }

        public static readonly DependencyProperty IsOpenCLEnabledProperty =
            DependencyProperty.Register("IsOpenCLEnabled",
                typeof(Boolean),
                typeof(LocalQuickWatchPresentation), new PropertyMetadata(false));

        public Boolean IsOpenCLEnabled
        {
            get { return (Boolean)GetValue(IsOpenCLEnabledProperty); }
            set { SetValue(IsOpenCLEnabledProperty, value); }
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
