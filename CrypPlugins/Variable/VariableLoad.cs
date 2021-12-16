/*                              
   Copyright 2009 Team CrypTool (Sven Rech,Dennis Nolte,Raoul Falk,Nils Kopal), Uni Duisburg-Essen

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

namespace CrypTool.Plugins.Variable
{
    [Author("Sven Rech", "sven.rech@CrypTool.org", "Uni Duisburg-Essen", "http://www.uni-due.de")]
    [PluginInfo("Variable.Properties.Resources", "VariableLoadCaption", "VariableLoadTooltip", "Variable/DetailedDescription/doc.xml", "Variable/loadIcon.png")]
    [ComponentCategory(ComponentCategory.ToolsDataflow)]
    internal class VariableLoad : ICrypComponent
    {
        #region Private variables

        private VariableSettings settings;
        private object loadObject;

        #endregion

        public VariableLoad()
        {
            settings = new VariableSettings();

        }

        public ISettings Settings
        {
            get => settings;
            set
            {
                settings = (VariableSettings)value;
                settings.PropertyChanged += settings_propertyChanged;
            }
        }

        #region Properties

        [PropertyInfo(Direction.OutputData, "VariableLoadObjectCaption", "VariableLoadObjectTooltip")]
        public object VariableLoadObject
        {
            get => loadObject;
            set
            {
                loadObject = value;
                OnPropertyChanged("VariableLoadObject");
            }
        }
        #endregion

        private void onVariableStore(string variable, object input)
        {
            if (variable == settings.VariableName)
            {
                VariableLoadObject = input;
                ProgressChanged(1.0, 1.0);
            }
        }

        #region IPlugin Members

        public event StatusChangedEventHandler OnPluginStatusChanged;

        public System.Windows.Controls.UserControl Presentation => null;

        public void PreExecution()
        {
        }

        public void Execute()
        {
            if (settings.VariableName == "")
            {
                GuiLogMessage("The variable name may not be empty.", NotificationLevel.Error);
            }
        }

        public void PostExecution()
        {
        }

        public void Stop()
        {
        }

        private void settings_propertyChanged(object sender, PropertyChangedEventArgs args)
        {
            Initialize();
        }

        public void Initialize()
        {
            Dispose();
            VariableStore.OnVariableStore += new StoreVariable(onVariableStore);
        }

        public void Dispose()
        {
            VariableStore.OnVariableStore -= onVariableStore;
        }

        public event GuiLogNotificationEventHandler OnGuiLogNotificationOccured;

        private void GuiLogMessage(string p, NotificationLevel notificationLevel)
        {
            EventsHelper.GuiLogMessage(OnGuiLogNotificationOccured, this, new GuiLogEventArgs(p, this, notificationLevel));
        }

        public event PluginProgressChangedEventHandler OnPluginProgressChanged;

        private void ProgressChanged(double value, double max)
        {
            EventsHelper.ProgressChanged(OnPluginProgressChanged, this, new PluginProgressEventArgs(value, max));
        }

        #endregion

        #region INotifyPropertyChanged Members

        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(string p)
        {
            EventsHelper.PropertyChanged(PropertyChanged, this, new PropertyChangedEventArgs(p));
        }

        #endregion


    }
}
