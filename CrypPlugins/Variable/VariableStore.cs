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
    internal delegate void StoreVariable(string variable, object input);

    [Author("Sven Rech", "sven.rech@CrypTool.org", "Uni Duisburg-Essen", "http://www.uni-due.de")]
    [PluginInfo("Variable.Properties.Resources", "VariableStoreCaption", "VariableStoreTooltip", "Variable/DetailedDescription/doc.xml", "Variable/storeIcon.png")]
    [ComponentCategory(ComponentCategory.ToolsDataflow)]
    internal class VariableStore : ICrypComponent
    {
        public static event StoreVariable OnVariableStore;

        #region Private variables
        private VariableSettings settings;
        #endregion

        public VariableStore()
        {
            settings = new VariableSettings();
        }

        public ISettings Settings
        {
            get => settings;
            set => settings = (VariableSettings)value;
        }

        #region Properties

        private object storeObject = null;
        [PropertyInfo(Direction.InputData, "VariableStoreObjectCaption", "VariableStoreObjectTooltip")]
        public object VariableStoreObject
        {
            get => storeObject;
            set
            {
                storeObject = value;
                OnPropertyChanged("VariableStoreObject");
            }
        }

        #endregion

        #region IPlugin Members

        public event StatusChangedEventHandler OnPluginStatusChanged;

        public System.Windows.Controls.UserControl Presentation => null;

        public void PreExecution()
        {
            Dispose();
        }

        public void Execute()
        {
            if (settings.VariableName == "")
            {
                GuiLogMessage("The variable name may not be empty.", NotificationLevel.Error);
                return;
            }

            if (storeObject == null)
            {
                GuiLogMessage("Object is null, not passing forward.", NotificationLevel.Info);
                return;
            }

            ProgressChanged(0.5, 1.0);
            if (OnVariableStore != null)
            {
                OnVariableStore(settings.VariableName, storeObject);
            }

            ProgressChanged(1.0, 1.0);
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
