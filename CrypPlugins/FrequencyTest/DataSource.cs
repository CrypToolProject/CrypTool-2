using System.Collections.ObjectModel;

namespace CrypTool.FrequencyTest
{
    public class DataSource
    {
        private ObservableCollection<CollectionElement> valueCollection;

        public ObservableCollection<CollectionElement> ValueCollection
        {
            get { return valueCollection; }
            set { valueCollection = value; }
        }

        public DataSource()
        {
            valueCollection = new ObservableCollection<CollectionElement>();
        }
    }
}
