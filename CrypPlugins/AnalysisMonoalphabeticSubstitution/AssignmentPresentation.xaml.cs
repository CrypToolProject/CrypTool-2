using CrypTool.CrypAnalysisViewControl;
using System.Collections.ObjectModel;
using System.Threading;
using System.Windows.Controls;
using System.Windows.Threading;

namespace CrypTool.AnalysisMonoalphabeticSubstitution
{
    /// <summary>
    /// Interaktionslogik für AssignmentPresentation.xaml
    /// </summary>
    [CrypTool.PluginBase.Attributes.Localization("CrypTool.AnalysisMonoalphabeticSubstitution.Properties.Resources")]
    public partial class AssignmentPresentation : UserControl
    {

        public ObservableCollection<ResultEntry> Entries { get; } = new ObservableCollection<ResultEntry>();

        #region Variables

        private UpdateOutput updateOutputFromUserChoice;

        #endregion

        #region Properties

        public UpdateOutput UpdateOutputFromUserChoice
        {
            get => updateOutputFromUserChoice;
            set => updateOutputFromUserChoice = value;
        }

        #endregion

        #region constructor

        public AssignmentPresentation()
        {
            InitializeComponent();
            DataContext = Entries;
        }

        #endregion

        #region Main Methods

        public void DisableGUI()
        {
            Dispatcher.BeginInvoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
            {
                IsEnabled = false;
            }, null);
        }

        public void EnableGUI()
        {
            Dispatcher.BeginInvoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
            {
                IsEnabled = true;
            }, null);
        }

        #endregion

        #region Helper Methods

        private void HandleResultItemAction(ICrypAnalysisResultListEntry item)
        {
            if (item is ResultEntry resultItem)
            {
                updateOutputFromUserChoice(resultItem.Key, resultItem.Text);
            }
        }

        #endregion

    }
}
