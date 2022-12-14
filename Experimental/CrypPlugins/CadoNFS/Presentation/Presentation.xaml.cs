using System.Collections;
using System.Collections.Specialized;
using System.Linq;
using System.Windows.Controls;

namespace CadoNFS.Presentation
{
    public partial class Presentation : UserControl
    {
        public Presentation(ViewModel viewModel)
        {
            DataContext = viewModel;
            InitializeComponent();
        }

        private void LogListView_Initialized(object sender, System.EventArgs e)
        {
            var logList = sender as ListView;
            (logList.ItemsSource as INotifyCollectionChanged).CollectionChanged += (_, ev) =>
            {
                logList.ScrollIntoView(ev.NewItems[0]);
            };
        }
    }
}
