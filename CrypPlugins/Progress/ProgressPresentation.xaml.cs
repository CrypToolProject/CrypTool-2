using System.Threading;
using System.Windows.Controls;

namespace CrypTool.Progress
{
    /// <summary>
    /// Interaction logic for ProgressPresentation.xaml
    /// </summary>
    public partial class ProgressPresentation : UserControl
    {
        public ProgressPresentation()
        {
            InitializeComponent();
        }

        public void Set(int value, int max)
        {
            Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Background, (SendOrPostCallback)delegate
            {
                if (max <= 0)
                {
                    Bar.Maximum = 100;
                }
                else
                {
                    Bar.Maximum = max;
                }
                Bar.Value = value;
            }, null);
        }
    }
}
