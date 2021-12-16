using System.Collections.ObjectModel;


namespace CrypTool.KasiskiTest
{
    public class DataSource
    {


        private ObservableCollection<CollectionElement> valueCollection;

        public ObservableCollection<CollectionElement> ValueCollection
        {
            get => valueCollection;
            set => valueCollection = value;
        }


        public DataSource()
        {

            valueCollection = new ObservableCollection<CollectionElement>();
            //  CollectionElement z= new CollectionElement(30,30,30);
            // valueCollection.Add(z);
            // CollectionElement y = new CollectionElement(30, 40, 50);
            // CollectionElement s = new CollectionElement(30, 200, 180);
            // valueCollection.Add(s);
            // valueCollection.Add(y);
            // CollectionElement q = new CollectionElement(30, 30, 150);
            // valueCollection.Add(q);

        }
    }
}
