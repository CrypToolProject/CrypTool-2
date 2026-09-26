/*
   Copyright (C) CrypTool 2 Team

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
using System.Windows.Threading;


namespace CrypTool.KasiskiTest
{
    [CrypTool.PluginBase.Attributes.Localization("KasiskiTest.Properties.Resources")]
    public partial class KasiskiTestPresentation : UserControl
    {

        //private KasiskiTest kTest;
        public KasiskiTestPresentation(KasiskiTest KasiskiTest)
        {
            //this.kTest = KasiskiTest;
            InitializeComponent();
            //OpenPresentationFile();

        }

        public void OpenPresentationFile()
        {
            Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
            {

                DataSource source = (DataSource)Resources["source"];
                source.ValueCollection.Clear();
                for (int i = 0; i < KasiskiTest.Data.ValueCollection.Count; i++)
                {
                    source.ValueCollection.Add(KasiskiTest.Data.ValueCollection[i]);
                }



            }, null);
        }
    }
}
