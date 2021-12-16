/*
   Copyright 2008 Thomas Schmid, University of Siegen

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

using Contains.Aho_Corasick;
using CrypTool.PluginBase;
using System.Text;
using System.Threading;
using System.Windows.Controls;

namespace Contains
{
    /// <summary>
    /// Interaction logic for ContainsPresentation.xaml
    /// </summary>
    [CrypTool.PluginBase.Attributes.Localization("Contains.Properties.Resources")]
    public partial class ContainsPresentation : UserControl
    {
        // private ObservableCollection<StringSearchResult> collection = new ObservableCollection<StringSearchResult>();

        public ContainsPresentation()
        {
            InitializeComponent();
            Width = double.NaN;
            Height = double.NaN;
            // listView.ItemsSource = collection;
        }

        public void SetHits(int value)
        {
            Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal, (SendOrPostCallback)delegate
            {
                hits.Content = typeof(Contains).GetPluginStringResource("LabelHits1") + value;
          //hits.Content = "Hits: " + value;
          //if (canResetListView)
          //{
          //  listView.ItemsSource = null;
          //  canResetListView = false;
          //}
      }, value);
        }

        public int TargetHits { get; set; }

        // [MethodImpl(MethodImplOptions.Synchronized)]
        public void SetData(StringSearchResult[] arr)
        {
            Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal, (SendOrPostCallback)delegate
            {
                if (arr != null)
                {
              //collection.Clear();
              //foreach (StringSearchResult item in arr)
              //{
              //  collection.Add(item);
              //}

              //hits.Content = "Hits: " + arr.Length + " (target: "+ TargetHits.ToString() + ")";
              hits.Content = typeof(Contains).GetPluginStringResource("LabelHits1") + arr.Length + " (" + typeof(Contains).GetPluginStringResource("LabelTarget") + " " + TargetHits.ToString() + ")";
                    StringBuilder sb = new StringBuilder();
                    foreach (StringSearchResult item in arr)
                    {
                        sb.Append("- [" + item.Keyword + "], [" + item.Index + "]\n");
                    }
                    textBox.Text = sb.ToString();
              //listView.ItemsSource = arr;
              //canResetListView = true;
              //foreach (GridViewColumn gvc in ((GridView)listView.View).Columns)
              //{
              //  gvc.Width = gvc.ActualWidth;
              //  gvc.Width = Double.NaN;
              //}
          }
                else
                {
                    hits.Content = typeof(Contains).GetPluginStringResource("LabelHits");
                }
            }, arr);
        }
    }
}
