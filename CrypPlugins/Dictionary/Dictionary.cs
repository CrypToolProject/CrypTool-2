/*                              
   Copyright 2023 Nils Kopal, CrypTool Project

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

using CrypTool.PluginBase;
using CrypTool.PluginBase.IO;
using CrypTool.PluginBase.Miscellaneous;
using LanguageStatisticsLib;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows.Controls;

namespace Dictionary
{
    [Author("Nils Kopal", "kopal@cryptool.org", "CrypTool 2 Team", "https://www.cryptool.org")]
    [PluginInfo("Dictionary.Properties.Resources", "PluginCaption", "PluginTooltip", "Dictionary/DetailedDescription/doc.xml", "Dictionary/icon.png")]
    [ComponentCategory(ComponentCategory.ToolsDataInputOutput)]
    public class CrypToolDictionary : ICrypComponent
    {
        #region private_variables

        private readonly CrypToolDictionarySettings _settings = new CrypToolDictionarySettings();      
        private readonly Dictionary<int, string[]> _dictionaryCache = new Dictionary<int, string[]>();

        # endregion private_variables

        public CrypToolDictionary()
        {
            
        }

        [PropertyInfo(Direction.OutputData, "OutputStringCaption", "OutputStringTooltip", false)]
        public string OutputString
        {
            get;
            set;
        }

        [PropertyInfo(Direction.OutputData, "OutputListCaption", "OutputListTooltip", false)]
        public string[] OutputList
        {
            get;
            set;
        }

        #region IPlugin Members

#pragma warning disable 67
        public event StatusChangedEventHandler OnPluginStatusChanged;
#pragma warning restore

        public event GuiLogNotificationEventHandler OnGuiLogNotificationOccured;

        private void GuiLogMessage(string message, NotificationLevel logLevel)
        {
            EventsHelper.GuiLogMessage(OnGuiLogNotificationOccured, this, new GuiLogEventArgs(message, this, logLevel));
        }

        public event PluginProgressChangedEventHandler OnPluginProgressChanged;

        public ISettings Settings => _settings;

        public UserControl Presentation => null;

        public void PreExecution()
        {
        }

        /// <summary>
        /// Executes the plugin. and outputs the selected dictionary as a string and as a list.
        /// </summary>
        public void Execute()
        {
            OnProgressChanged(0, 1);

            if (!_dictionaryCache.ContainsKey(_settings.Language))
            {
                List<string> dictionary = LanguageStatistics.LoadDictionary(LanguageStatistics.LanguageCode(_settings.Language), DirectoryHelper.DirectoryLanguageStatistics);
                _dictionaryCache.Add(_settings.Language, dictionary.ToArray());
            }

            OutputList = _dictionaryCache[_settings.Language].ToArray();
            if(_settings.Capitalization == Case.Lowercase)
            {
                OutputList = OutputList.Select(x => x.ToLower()).ToArray();
            }          
            OnPropertyChanged(nameof(OutputList));

            OutputString = string.Join(",", OutputList);
            if(_settings.Capitalization == Case.Uppercase)
            {
                OutputString = OutputString.ToUpper();
            }
            OnPropertyChanged(nameof(OutputString));

            OnProgressChanged(1, 1);
        }

        public void PostExecution()
        {
        }

        public void Stop()
        {
        }
     
        public void Initialize()
        {            
        }

        public void Dispose()
        {
        }

        private void OnProgressChanged(double value, double max)
        {
            EventsHelper.ProgressChanged(OnPluginProgressChanged, this, new PluginProgressEventArgs(value, max));
        }


        private void OnPropertyChanged(string name)
        {
            EventsHelper.PropertyChanged(PropertyChanged, this, new PropertyChangedEventArgs(name));
        }

        #endregion

        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion
    }
}
