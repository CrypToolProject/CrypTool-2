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
using CrypTool.CrypAnalysisViewControl;
using KeySearcher;
using System.Collections.ObjectModel;
using System.Windows;

namespace KeySearcherPresentation.Controls
{
    [CrypTool.PluginBase.Attributes.Localization("KeySearcher.Properties.Resources")]
    public partial class LocalQuickWatchPresentation
    {
        private KeySearcher.KeySearcher.UpdateOutput _updateOutputFromUserChoice;

        public KeySearcher.KeySearcher.UpdateOutput UpdateOutputFromUserChoice
        {
            get => _updateOutputFromUserChoice;
            set => _updateOutputFromUserChoice = value;
        }

        public ObservableCollection<ResultEntry> Entries { get; } = new ObservableCollection<ResultEntry>();
        
        public LocalQuickWatchPresentation()
        {
            InitializeComponent();
            this.DataContext = this;
        }

        private void HandleResultItemAction(ICrypAnalysisResultListEntry item)
        {
            if (item is ResultEntry resultItem)
            {
                _updateOutputFromUserChoice(resultItem.Key, resultItem.FullText);
            }
        }
    }
}
