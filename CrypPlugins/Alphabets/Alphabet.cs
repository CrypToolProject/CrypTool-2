/*
   Copyright 2008 Sebastian Przybylski, University of Siegen

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
using System;
using System.ComponentModel;
using System.Threading;
using System.Windows.Controls;
using System.Windows.Threading;

namespace CrypTool.Alphabets
{
    [Author("Sebastian Przybylski", "sebastian@przybylski.org", "Uni Siegen", "http://www.uni-siegen.de")]
    [PluginInfo("CrypTool.Alphabets.Properties.Resources", "PluginCaption", "PluginTooltip", "Alphabets/DetailedDescription/doc.xml", "Alphabets/icon.gif")]
    [ComponentCategory(ComponentCategory.ToolsDataInputOutput)]
    public class Alphabet : ICrypComponent
    {
        private readonly AlphabetPresentation alphabetPresentation;

        private AlphabetSettings settings;
        public ISettings Settings
        {
            get => settings;
            set => settings = (AlphabetSettings)value;
        }

        public Alphabet()
        {
            settings = new AlphabetSettings();
            alphabetPresentation = new AlphabetPresentation(settings);
            Presentation = alphabetPresentation;
            alphabetPresentation.AlphabetChanged += new EventHandler(alphabetPresentation_AlphabetChanged);
        }

        private void alphabetPresentation_AlphabetChanged(object sender, EventArgs e)
        {
            alphabetPresentation.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
            {
                alphabetString = alphabetPresentation.GetAlphabet();
                OnPropertyChanged("AlphabetOutput");
            }, null);
        }      

        private string alphabetString = string.Empty;

        [PropertyInfo(Direction.OutputData, "AlphabetOutputCaption", "AlphabetOutputTooltip", true)]
        public string AlphabetOutput
        {
            get => alphabetString;
            set { } 
        }


        public UserControl Presentation { get; private set; }

        public void Initialize()
        {

        }

        public void Dispose()
        {
        }

        public void Stop()
        {
        }

        public void PreExecution()
        {
            alphabetPresentation.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
            {
                alphabetString = alphabetPresentation.GetAlphabet();
            }, null);

        }

        public void PostExecution()
        {
        }

        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;

        public void OnPropertyChanged(string name)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(name));
            }
        }

        #region IPlugin Members

#pragma warning disable 67
        public event StatusChangedEventHandler OnPluginStatusChanged;
#pragma warning restore
        public event GuiLogNotificationEventHandler OnGuiLogNotificationOccured;
        public event PluginProgressChangedEventHandler OnPluginProgressChanged;
        private readonly bool stop;

        private void GuiLogMessage(string message, NotificationLevel logLevel)
        {
            if (OnGuiLogNotificationOccured != null)
            {
                OnGuiLogNotificationOccured(this, new GuiLogEventArgs(message, this, logLevel));
            }
        }

        public void Execute()
        {
            OnPropertyChanged("AlphabetOutput");
            ShowProgress(100, 100);
        }

        #endregion

        #region Private
        private void ShowProgress(double value, double max)
        {
            EventsHelper.ProgressChanged(OnPluginProgressChanged, this, new PluginProgressEventArgs(value, max));
        }
        #endregion
    }
}
