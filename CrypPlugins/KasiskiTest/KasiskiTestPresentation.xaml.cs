using System.Threading;
using System.Windows.Controls;
using System.Windows.Threading;


namespace CrypTool.KasiskiTest
{
    [CrypTool.PluginBase.Attributes.Localization("KasiskiTest.Properties.Resources")]
    public partial class KasiskiTestPresentation : UserControl
    {

        //private KasiskiTest kTest;
        public KasiskiTestPresentation(KasiskiTest KasiskiTest)
        {
            //this.kTest = KasiskiTest;
            InitializeComponent();
            //OpenPresentationFile();

        }

        public void OpenPresentationFile()
        {
            Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
            {

                DataSource source = (DataSource)Resources["source"];
                source.ValueCollection.Clear();
                for (int i = 0; i < KasiskiTest.Data.ValueCollection.Count; i++)
                {
                    source.ValueCollection.Add(KasiskiTest.Data.ValueCollection[i]);
                }



            }, null);
        }
    }
}
