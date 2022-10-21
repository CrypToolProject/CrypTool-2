/*
   Copyright 2008 - 2022 CrypTool Team

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
using System.Threading;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;


namespace CrypTool.PluginBase.Utils.Graphics.Diagrams.Histogram
{
    /// <summary>
    /// Interaction logic for FrequencyTestPresentation.xaml
    /// </summary>
    [CrypTool.PluginBase.Attributes.Localization("CrypTool.PluginBase.Properties.Resources")]
    public partial class HistogramChart : UserControl
    {

        public HistogramChart()
        {
            InitializeComponent();
        }


        public void ShowData(HistogramDataSource data)
        {
            Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
            {
                HistogramDataSource source = (HistogramDataSource)Resources["source"];
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
