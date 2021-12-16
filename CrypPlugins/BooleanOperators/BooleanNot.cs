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


namespace CrypTool.Plugins.BooleanOperators
{
    [Author("Nils Kopal", "nils.kopal@CrypTool.org", "Uni Duisburg-Essen", "http://www.uni-duisburg-essen.de")]
    [PluginInfo("BooleanOperators.Properties.Resources", "PluginBN_Caption", "PluginBN_Tooltip", "BooleanOperators/DetailedDescription/doc.xml", "BooleanOperators/icons/not.png")]
    [ComponentCategory(ComponentCategory.ToolsBoolean)]
    public class BooleanNot : ICrypComponent
    {

        private bool input = false;
        private bool output = false;

        private BooleanNotSettings settings;

        public BooleanNot()
        {
            settings = new BooleanNotSettings();
        }

        [PropertyInfo(Direction.InputData, "BN_InputCaption", "BN_InputTooltip")]
        public bool Input
        {
            get => input;
            set
            {
                input = value;
                OnPropertyChange("Input");
            }
        }

        [PropertyInfo(Direction.OutputData, "BN_OutputCaption", "BN_OutputTooltip")]
        public bool Output
        {
            get => output;
            set
            {
                output = value;
                OnPropertyChange("Output");
            }
        }

        public ISettings Settings
        {
            get => settings;
            set => settings = (BooleanNotSettings)value;
        }


        #region IPlugin Members

        public void Dispose()
        {
        }

        public void Execute()
        {
            Output = !Input;
            ProgressChanged(1, 1);
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

        public System.Windows.Controls.UserControl Presentation => null;

        public void Stop()
        {
        }

        #endregion

        #region event handling

        public event GuiLogNotificationEventHandler OnGuiLogNotificationOccured;
        public event PluginProgressChangedEventHandler OnPluginProgressChanged;
        public event StatusChangedEventHandler OnPluginStatusChanged;
        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChange(string propertyname)
        {
            EventsHelper.PropertyChanged(PropertyChanged, this, new PropertyChangedEventArgs(propertyname));
        }

        private void settings_OnPluginStatusChanged(IPlugin sender, StatusEventArgs args)
        {
            if (OnPluginStatusChanged != null)
            {
                OnPluginStatusChanged(this, args);
            }
        }

        private void ProgressChanged(double value, double max)
        {
            EventsHelper.ProgressChanged(OnPluginProgressChanged, this, new PluginProgressEventArgs(value, max));
        }

        private void GuiLogMessage(string p, NotificationLevel notificationLevel)
        {
            EventsHelper.GuiLogMessage(OnGuiLogNotificationOccured, this, new GuiLogEventArgs(p, this, notificationLevel));
        }

        #endregion
    }
}
