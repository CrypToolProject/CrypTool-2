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
using System.Collections.ObjectModel;
using System.Threading;
using System.Windows.Controls;
using System.Windows.Threading;

namespace CrypTool.Plugins.DECRYPTTools
{
    /// <summary>
    /// Interaktionslogik für DECRYPTParserTestPresentation.xaml
    /// </summary>
    public partial class DECRYPTParserTestPresentation : UserControl
    {
        private ObservableCollection<BestListEntry> BestList
        {
            get;
            set;
        }
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="decoderClusterer"></param>
        public DECRYPTParserTestPresentation()
        {
            InitializeComponent();
            BestList = new ObservableCollection<BestListEntry>();
            ListView.ItemsSource = BestList;
        }

        /// <summary>
        /// Add new entry to best list of parsers
        /// </summary>
        /// <param name="bestListEntry"></param>
        public void AddNewBestlistEntry(BestListEntry bestListEntry)
        {
            Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
            {
                BestList.Add(bestListEntry);
            }, null);
        }

        /// <summary>
        /// Clears the best list of parsers
        /// </summary>
        public void ClearBestlist()
        {
            Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
            {
                BestList.Clear();
            }, null);
        }
    }
}
