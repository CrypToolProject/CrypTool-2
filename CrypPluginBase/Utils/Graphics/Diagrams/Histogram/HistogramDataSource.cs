using System.Collections.ObjectModel;

namespace CrypTool.PluginBase.Utils.Graphics.Diagrams.Histogram
{
    public class HistogramDataSource
    {
        private ObservableCollection<HistogramElement> valueCollection;

        public ObservableCollection<HistogramElement> ValueCollection
        {
            get { return valueCollection; }
            set { valueCollection = value; }
        }

        public HistogramDataSource()
        {
            valueCollection = new ObservableCollection<HistogramElement>();
        }
    }
}
