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
using CrypTool.PluginBase;
using CrypTool.PluginBase.Miscellaneous;
using CrypTool.Plugins.DECRYPTTools.Util;
using System;
using System.ComponentModel;
using System.Reflection;
using System.Windows.Controls;

namespace CrypTool.Plugins.DECRYPTTools
{
    [Author("Nils Kopal", "nils.kopal@CrypTool.org", "CrypTool 2 Team", "https://www.CrypTool.org")]
    [PluginInfo("CrypTool.Plugins.DECRYPTTools.Properties.Resources", "DECRYPTSymbolHeatmapCaption", "DECRYPTSymbolHeatmapTooltip", "DECRYPTTools/userdoc.xml", "DECRYPTTools/icon.png")]
    [ComponentCategory(ComponentCategory.DECRYPTProjectComponent)]
    public class DECRYPTSymbolHeatmap : ICrypComponent
    {
        private string _DECRYPTTextDocument;
        private string _alphabet;
        private readonly DECRYPTSymbolHeatmapPresentation _presentation = new DECRYPTSymbolHeatmapPresentation();
        private readonly DECRYPTSymbolHeatmapSettings _settings = new DECRYPTSymbolHeatmapSettings();

        public ISettings Settings => _settings;

        public UserControl Presentation => _presentation;

        public event StatusChangedEventHandler OnPluginStatusChanged;
        public event PluginProgressChangedEventHandler OnPluginProgressChanged;
        public event GuiLogNotificationEventHandler OnGuiLogNotificationOccured;
        public event PropertyChangedEventHandler PropertyChanged;

        public DECRYPTSymbolHeatmap()
        {

        }

        /// <summary>
        /// Input of a DECRYPTTextDocument (cipher file)
        /// </summary>
        [PropertyInfo(Direction.InputData, "DECRYPTTextDocumentCaption", "DECRYPTTextDocumentTooltip")]
        public string DECRYPTTextDocument
        {
            get => _DECRYPTTextDocument;
            set => _DECRYPTTextDocument = value;
        }

        /// <summary>
        /// Input of a DECRYPTTextDocument (cipher file)
        /// </summary>
        [PropertyInfo(Direction.InputData, "AlphabetCaption", "AlphabetTooltip")]
        public string Alphabet
        {
            get => _alphabet;
            set => _alphabet = value;
        }

        public void Dispose()
        {

        }

        public void Execute()
        {
            //we only generate the heatmap if we have at minimum two grams besides each other in the text
            if (_DECRYPTTextDocument.Length < (int)(_settings.FirstGrams) + (int)(_settings.SecondGrams) + 2)
            {
                return;
            }

            NoNomenclatureParser parser = new NoNomenclatureParser(1, null)
            {
                DECRYPTTextDocument = _DECRYPTTextDocument
            };
            TextDocument textDocument = parser.GetTextDocument();

            NoNomenclatureParser parser2 = new NoNomenclatureParser(1, null)
            {
                DECRYPTTextDocument = _alphabet
            };
            System.Collections.Generic.List<Token> alphabetTokens = parser2.GetTextDocument().ToList();

            try
            {
                _presentation.GenerateNewHeatmap(textDocument, alphabetTokens, _settings);
            }
            catch (Exception ex)
            {
                GuiLogMessage(string.Format("Exception occured during generation of heatmap: {0}", ex.Message), NotificationLevel.Error);
            }

        }

        public void Initialize()
        {

        }

        public void PostExecution()
        {

        }

        public void PreExecution()
        {

        }

        public void Stop()
        {

        }

        private void OnPropertyChanged(string name)
        {
            EventsHelper.PropertyChanged(PropertyChanged, this, new PropertyChangedEventArgs(name));
        }

        private void GuiLogMessage(string message, NotificationLevel logLevel)
        {
            EventsHelper.GuiLogMessage(OnGuiLogNotificationOccured, this, new GuiLogEventArgs(message, this, logLevel));
        }
    }
}
