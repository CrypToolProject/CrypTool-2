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

using CrypTool.PluginBase.IO;
using System;
using System.Threading;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Markup;
using System.Windows.Threading;

namespace CrypTool.StreamComparator
{
    /// <summary>
    /// Interaction logic for StreamComparatorPresentation.xaml
    /// </summary>
    public partial class StreamComparatorPresentation : UserControl
    {
        public StreamComparatorPresentation()
        {
            InitializeComponent();
            Height = double.NaN;
            Width = double.NaN;
            documentReader.Height = double.NaN;
            documentReader.Width = double.NaN;
            SetNoComparisonYetDocument();
        }

        public void SetContent(ICrypToolStream stream)
        {
            Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
            {
                try
                {
                    using (CStreamReader reader = stream.CreateReader())
                    {
                        FlowDocument flowDocumentNew = (FlowDocument)XamlReader.Load(reader);
                        documentReader.Document = flowDocumentNew;
                    }
                }
                catch (Exception)
                {
                    documentReader.Document = null;
                }
            }, documentReader);
        }

        public void SetBinaryDocument()
        {
            Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
            {
                FlowDocument fd = Resources["documentBinaryInput"] as FlowDocument;
                documentReader.Document = fd;
            }, documentReader);
        }

        public void SetNoComparisonYetDocument()
        {
            Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
            {
                FlowDocument fd = Resources["documentNoComparsionYet"] as FlowDocument;
                documentReader.Document = fd;
            }, documentReader);
        }

    }
}
