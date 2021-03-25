using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.ComponentModel;
using System.Collections.ObjectModel;
using System.Windows.Threading;
using System.Threading;

namespace CrypTool.Plugins.HomophonicAnalyzer
{
    /// <summary>
    /// Interaktionslogik für AssignmentPresentation.xaml
    /// </summary>
    [CrypTool.PluginBase.Attributes.Localization("HomophonicAnalyzer.Properties.Resources")]
    public partial class AssignmentPresentation : UserControl
    {

        public ObservableCollection<ResultEntry> entries = new ObservableCollection<ResultEntry>();
        //public event EventHandler doppelClick;

        #region Variables

        private UpdateOutput updateOutputFromUserChoice;

        #endregion

        #region Properties

        public UpdateOutput UpdateOutputFromUserChoice
        {
            get { return this.updateOutputFromUserChoice; }
            set { this.updateOutputFromUserChoice = value; }
        }

        #endregion

        #region constructor

        public AssignmentPresentation()
        {
            InitializeComponent();
            this.DataContext = entries;

        }

        #endregion

        #region Main Methods

        public void DisableGUI()
        {
            this.Dispatcher.BeginInvoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
            {
                this.ListView.IsEnabled = false;
            }, null);
        }

        public void EnableGUI()
        {
            this.Dispatcher.BeginInvoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
            {
                this.ListView.IsEnabled = true;
            }, null);
        }

        #endregion

        #region Helper Methods

        public void HandleDoubleClick(Object sender, EventArgs eventArgs)
        {
            ListViewItem lvi = sender as ListViewItem;
            ResultEntry r = lvi.Content as ResultEntry;

            if (r != null)
            {
                this.updateOutputFromUserChoice(r.Key, r.Text);
            }
        }

        public void HandleSingleClick(Object sender, EventArgs eventArgs)
        {
            //this.updateOutputFromUserChoice(0);
        }

        // Strings with nul characters are not displayed correctly in the clipboard
        string removeNuls(string s)
        {
            return s.Replace(Convert.ToChar(0x0).ToString(), "");
        }

        string entryToText(ResultEntry entry)
        {
            return "Rank: " + entry.Ranking + "\n" +
                   "Value: " + entry.Value + "\n" +
                   "Attack: " + entry.Attack + "\n" +
                   "Key: " + entry.Key + "\n" +
                   "Text: " + removeNuls(entry.Text);
        }

        public void ContextMenuHandler(Object sender, EventArgs eventArgs)
        {
            try
            {
                MenuItem menu = (MenuItem)((RoutedEventArgs)eventArgs).Source;
                ResultEntry entry = (ResultEntry)menu.CommandParameter;
                if (entry == null) return;
                string tag = (string)menu.Tag;

                if (tag == "copy_text")
                {
                    Clipboard.SetText(removeNuls(entry.Text));
                }
                else if (tag == "copy_value")
                {
                    Clipboard.SetText(entry.Value);
                }
                else if (tag == "copy_key")
                {
                    Clipboard.SetText(entry.Key);
                }
                else if (tag == "copy_line")
                {
                    Clipboard.SetText(entryToText(entry));
                }
                else if (tag == "copy_all")
                {
                    List<string> lines = new List<string>();
                    foreach (var e in entries) lines.Add(entryToText(e));
                    Clipboard.SetText(String.Join("\n\n", lines));
                }
            }
            catch (Exception ex)
            {
                Clipboard.SetText("");
            }
        }

        #endregion
    }
}
