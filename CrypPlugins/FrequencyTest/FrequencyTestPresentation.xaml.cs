/*  
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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;

namespace CrypTool.FrequencyTest
{
    /// <summary>
    /// Interaction logic for FrequencyTestPresentation.xaml
    /// </summary>
    [CrypTool.PluginBase.Attributes.Localization("CrypTool.FrequencyTest.Properties.Resources")]
    public partial class FrequencyTestPresentation : UserControl
    {
        public FrequencyTestPresentation()
        {
            InitializeComponent();
        }


        public void ShowData(DataSource data, bool sort, int maxNumberOfShownNGrams)
        {
            List<CollectionElement> list = data.ValueCollection.ToList();
            //here, we sort by frequency occurrence if the user wants so
            if (sort)
            {
                list.Sort(delegate (CollectionElement a, CollectionElement b) { return (a.Height > b.Height ? -1 : 1); });
            }

            //here, we remove all low frequencies until we only have maxNumberOfShownNGrams left
            List<CollectionElement> sorted_list = data.ValueCollection.ToList();
            sorted_list.Sort(delegate (CollectionElement a, CollectionElement b) { return (a.Height > b.Height ? -1 : 1); });
            for (int i = maxNumberOfShownNGrams; i < sorted_list.Count; i++)
            {
                list.Remove(sorted_list[i]);
            }

            Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
            {
                try
                {
                    DataSource source = (DataSource)Resources["source"];
                    source.ValueCollection.Clear();
                    for (int i = 0; i < list.Count; i++)
                    {
                        source.ValueCollection.Add(list[i]);
                    }
                }
                catch (Exception)
                {
                    //do nothing
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
