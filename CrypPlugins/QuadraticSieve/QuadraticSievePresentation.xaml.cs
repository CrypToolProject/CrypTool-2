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
using System.Windows.Controls;

namespace CrypTool.Plugins.QuadraticSieve
{
    /// <summary>
    /// Interaction logic for QuadraticSievePresentation.xaml
    /// </summary>
    [CrypTool.PluginBase.Attributes.Localization("QuadraticSieve.Properties.Resources")]
    public partial class QuadraticSievePresentation : UserControl
    {
        private readonly ProgressRelationPackages progressRelationPackages;
        public ProgressRelationPackages ProgressRelationPackages => progressRelationPackages;

        public QuadraticSievePresentation()
        {
            InitializeComponent();

            progressRelationPackages = new ProgressRelationPackages(peer2peerScrollViewer);
            peer2peerScrollViewer.Content = progressRelationPackages;
            progressRelationPackages.MaxWidth = 620 - 30;
        }

        public void SelectFirstComposite()
        {
            foreach (string item in factorList.Items)
            {
                if (item.StartsWith("Composite"))
                {
                    factorList.SelectedItem = item;
                    factorList.ScrollIntoView(item);
                    return;
                }
            }
        }
    }
}
