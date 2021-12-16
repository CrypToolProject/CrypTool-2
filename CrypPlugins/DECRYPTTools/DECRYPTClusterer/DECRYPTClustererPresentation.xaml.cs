/*
   Copyright 2019 Nils Kopal <Nils.Kopal<at>CrypTool.org

   Licensed under the Apache License, Version 2.0 (the "License");
   you may not use this file except in compliance with the License.
   You may obtain a copy of the License at

       http://www.apache.org/licenses/LICENSE-2.0

   Unless required by applicable law or agreed to in writing, software
   distributed under the License is distributed on an "AS IS" BASIS,
   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
   See the License for the specific language governing permissions and
   limitations under the License.
*/
using CrypTool.PluginBase.Miscellaneous;
using CrypTool.Plugins.DECRYPTTools.Util;
using System.ComponentModel;
using System.Windows.Controls;

namespace CrypTool.Plugins.DECRYPTTools
{
    /// <summary>
    /// Interaktionslogik für DECRYPTClustererPresentation.xaml
    /// </summary>
    public partial class DECRYPTClustererPresentation : UserControl, INotifyPropertyChanged
    {
        private readonly DECRYPTClusterer _decodeClusterer;
        private ClusterSet _currentClusterSet;
        public ClusterSet CurrentClusterSet
        {
            get => _currentClusterSet;
            set
            {
                _currentClusterSet = value;
                OnPropertyChanged("CurrentClusterSet");
            }
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="decoderClusterer"></param>
        public DECRYPTClustererPresentation(DECRYPTClusterer decoderClusterer)
        {
            DataContext = this;
            InitializeComponent();
            _decodeClusterer = decoderClusterer;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(string name)
        {
            EventsHelper.PropertyChanged(PropertyChanged, this, new PropertyChangedEventArgs(name));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ListView_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            object item = (sender as ListView).SelectedItem;
            if (item != null && item is Cluster)
            {
                _decodeClusterer.OutputCluster((Cluster)item);
            }
        }
    }
}
