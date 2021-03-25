using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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

namespace SigabaPhaseII
{
    /// <summary>
    /// Interaction logic for SigabaPhaseIIPresentation.xaml
    /// </summary>
    
    [global::CrypTool.PluginBase.Attributes.Localization("SigabaPhaseII.Properties.Resources")]
    public partial class SigabaPhaseIIPresentation : UserControl
    {
        public ObservableCollection<ResultEntry> entries = new ObservableCollection<ResultEntry>();
        public event EventHandler doppelClick;

        public SigabaPhaseIIPresentation()
        {
            InitializeComponent();
            this.DataContext = entries;
        }

        public void HandleDoubleClick(Object sender, EventArgs eventArgs)
        {
            doppelClick(sender, eventArgs);
        }
    }
}
