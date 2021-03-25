using System.Windows.Controls;
using System.Windows.Media;
using System.Threading;
using System.Windows.Threading;


namespace CrypTool.PluginBase.Utils.Graphics.Diagrams.Histogram
{
    /// <summary>
    /// Interaction logic for FrequencyTestPresentation.xaml
    /// </summary>
    [CrypTool.PluginBase.Attributes.Localization("CrypTool.PluginBase.Properties.Resources")]
    public partial class HistogramChart : UserControl
    {

        public  HistogramChart()
        {
           InitializeComponent();
        }


        public void ShowData(HistogramDataSource data)
        {
            Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
            {
                HistogramDataSource source = (HistogramDataSource)this.Resources["source"];
                source.ValueCollection.Clear();
                for (int i = 0; i < data.ValueCollection.Count; i++)
                {
                    source.ValueCollection.Add(data.ValueCollection[i]);
                }                
            }, null);
        }


        public void SetHeadline(string text)
        {
            Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
            {
                chartHeadline.Text = text;
            }, null);
        }

        public void SetScaler(double value)
        {
            Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
            {
                chart.LayoutTransform = new ScaleTransform(value, value);
            }, null);
        }

        public void SetBackground(Brush brush)
        {
            Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
            {
                chart.Background = brush;
            }, null);
        }

    }
}
