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
using System.Collections.ObjectModel;

namespace CrypTool.PluginBase.Utils.Graphics.Diagrams.Histogram
{
    public class HistogramDataSource
    {
        private ObservableCollection<HistogramElement> valueCollection;

        public ObservableCollection<HistogramElement> ValueCollection
        {
            get => valueCollection;
            set => valueCollection = value;
        }

        public HistogramDataSource()
        {
            valueCollection = new ObservableCollection<HistogramElement>();
        }
    }
}
