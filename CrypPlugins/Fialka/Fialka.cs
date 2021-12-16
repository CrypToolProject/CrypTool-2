/*
   Copyright 2016, Eugen Antal and Tomáš Sovič, FEI STU Bratislava

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
using System.ComponentModel;
using System.Windows.Controls;

namespace CrypTool.Fialka
{
    [Author("Eugen Antal, Tomáš Sovič", "eugen.antal@stuba.sk", "FEI STU Bratislava", "http://www.fei.stuba.sk/")]

    [PluginInfo("CrypTool.Fialka.Properties.Resources", "PluginCaption", "PluginTooltip", "Fialka/DetailedDescription/userdoc.xml", "Fialka/Images/FialkaRot.png")]

    [ComponentCategory(ComponentCategory.CiphersClassic)]

    public class Fialka : ICrypComponent
    {
        #region Private Variables
        private readonly FialkaSettings settings;
        private FialkaCore fialkaCore;
        private FialkaInternalState savedState;
        public bool isrunning;
        #endregion

        #region Constructor
        /// <summary>
        /// Constructor, also creates a new FialkaSettings instance.
        /// </summary>
        public Fialka()
        {
            settings = new FialkaSettings();
        }
        #endregion

        #region Data Properties
        /// <summary>
        /// Input text.
        /// </summary>
        [PropertyInfo(Direction.InputData, "InputTextCaption", "InputTextDescription", true)]
        public string Input
        {
            get;
            set;
        }

        /// <summary>
        /// Output text.
        /// </summary>
        [PropertyInfo(Direction.OutputData, "OutputTextCaption", "OutputTextDescription", false)]
        public string Output
        {
            get;
            set;
        }
        /// <summary>
        /// Second output, contains the key before and after the encryption.
        /// </summary>
        [PropertyInfo(Direction.OutputData, "OutputKeyCaption", "OutputKeyDescription", false)]
        public string Key
        {
            get;
            set;
        }

        #endregion

        #region IPlugin Members
        /// <summary>
        /// Provide plugin-related parameters (per instance) or return null.
        /// </summary>
        public ISettings Settings => settings;

        /// <summary>
        /// Provide custom presentation to visualize the execution or return null.
        /// Presentation not included yet.
        /// </summary>
        public UserControl Presentation => null;

        /// <summary>
        /// Called once when workflow execution starts.
        /// Saves the internal state before execution.
        /// </summary>
        public void PreExecution()
        {
            savedState = settings.internalState.DeepCopy();
            fialkaCore = new FialkaCore(settings.internalState);
            GuiLogMessage("Internal state saved.", NotificationLevel.Debug);
        }


        /// <summary>
        /// Called every time this plugin is run in the workflow execution.
        /// Input and output is handled inside the FialkaCore. 
        /// Output key is assigned after execution.
        /// Based on the settings - the saved internal state can be restored before the execution.
        /// </summary>
        public void Execute()
        {
            if (string.IsNullOrEmpty(Input))
            {
                GuiLogMessage("Input is null or empty.", NotificationLevel.Debug);
                return;
            }

            if (settings.settingsRestore == FialkaSettings.RestoreInitialSettings.WhenInputChanged)
            {
                settings.internalState = savedState.DeepCopy();
                settings.internalStateChanged(); // notify old values were restored
                GuiLogMessage("Internal state restored.", NotificationLevel.Debug);
            }

            string key1, key2;

            // init
            key1 = settings.internalState.getFormattedKey();
            fialkaCore = new FialkaCore(settings.internalState);
            Output = "";
            ProgressChanged(0, Input.Length);
            GuiLogMessage("Input is: " + Input, NotificationLevel.Debug);

            // encryption
            Output = fialkaCore.encrypt(Input, ProgressChanged);
            OnPropertyChanged("Output");
            GuiLogMessage("Output is: " + Output, NotificationLevel.Debug);
            key2 = settings.internalState.getFormattedKey();

            // finalization
            settings.internalStateChanged();
            ProgressChanged(Input.Length, Input.Length);
            Key = "Before encryption:\r" + key1 + "\r\rAfter encryption:\r" + key2;
            OnPropertyChanged("Key");
        }

        /// <summary>
        /// Called once after workflow execution has stopped.
        /// Based on the settings - the saved internal state can be restored after the execution.
        /// </summary>
        public void PostExecution()
        {
            if (settings.settingsRestore == FialkaSettings.RestoreInitialSettings.AfterExecution || settings.settingsRestore == FialkaSettings.RestoreInitialSettings.WhenInputChanged)
            {
                settings.internalState = savedState;
                settings.internalStateChanged(); // notify old values were restored
                fialkaCore = null;
                GuiLogMessage("Internal state restored.", NotificationLevel.Debug);
            }
        }

        /// <summary>
        /// Triggered time when user clicks stop button.
        /// Shall abort long-running execution.
        /// Based on the settings - the saved internal state can be restored when the STUP button is pressed.
        /// </summary>
        public void Stop()
        {
            if (settings.settingsRestore == FialkaSettings.RestoreInitialSettings.WhenStopped)
            {
                settings.internalState = savedState;
                settings.internalStateChanged(); // notify old values were restored
                fialkaCore = null;
                GuiLogMessage("Internal state restored.", NotificationLevel.Debug);
            }
        }

        /// <summary>
        /// Called once when plugin is loaded into editor workspace.
        /// </summary>
        public void Initialize()
        {
        }

        /// <summary>
        /// Called once when plugin is removed from editor workspace.
        /// </summary>
        public void Dispose()
        {
        }

        #endregion

        #region Event Handling

        public event StatusChangedEventHandler OnPluginStatusChanged;

        public event GuiLogNotificationEventHandler OnGuiLogNotificationOccured;

        public event PluginProgressChangedEventHandler OnPluginProgressChanged;

        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Fires events to indicate log messages.
        /// </summary>
        private void GuiLogMessage(string message, NotificationLevel logLevel)
        {
            EventsHelper.GuiLogMessage(OnGuiLogNotificationOccured, this, new GuiLogEventArgs(message, this, logLevel));
        }

        private void OnPropertyChanged(string name)
        {
            EventsHelper.PropertyChanged(PropertyChanged, this, new PropertyChangedEventArgs(name));
        }

        /// <summary>
        /// Fires events to indicate progress bar changes.
        /// </summary>
        private void ProgressChanged(double value, double max)
        {
            EventsHelper.ProgressChanged(OnPluginProgressChanged, this, new PluginProgressEventArgs(value, max));
        }

        #endregion
    }
}