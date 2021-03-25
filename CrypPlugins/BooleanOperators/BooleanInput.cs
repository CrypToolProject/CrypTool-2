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

using System;
using System.Windows.Controls;
using System.ComponentModel;
using CrypTool.PluginBase;
using CrypTool.PluginBase.Miscellaneous;
using BooleanOperators;

namespace CrypTool.Plugins.BooleanOperators
{
    [Author("Julian Weyers", "julian.weyers@CrypTool.org", "Uni Duisburg-Essen", "http://www.uni-duisburg-essen.de")]
    [PluginInfo("BooleanOperators.Properties.Resources", "PluginBI_Caption", "PluginBI_Tooltip", "BooleanOperators/DetailedDescription/doc.xml", "BooleanOperators/icons/false.png", "BooleanOperators/icons/true.png")]
    [ComponentCategory(ComponentCategory.ToolsBoolean)]
    public class BooleanInput : ICrypComponent
    {
        private BooleanInputSettings settings;
        private ButtonInputPresentation presentation;

        public BooleanInput()
        {
            this.settings = new BooleanInputSettings();
            this.presentation = new ButtonInputPresentation();

            this.presentation.StatusChanged += new EventHandler(presentation_StatusChanged);
            this.settings.PropertyChanged += settings_OnPropertyChange;
        }

        private void settings_OnPropertyChange(object sender, PropertyChangedEventArgs e)
        {
            Execute();
        }

        private void presentation_StatusChanged(object sender, EventArgs e)
        {
            settings.Value = presentation.Value ? 1 : 0;
            Output = (settings.Value == 1);
            settings_OnPluginStatusChanged(this, new StatusEventArgs(StatusChangedMode.ImageUpdate, settings.Value));
        }

        [PropertyInfo(Direction.OutputData, "BI_OutputCaption", "BI_OutputTooltip", false)]
        public Boolean Output
        {
            get;
            set;
        }

        #region IPlugin Member

        public void Dispose()
        {
        }

        public void Execute()
        {
            Initialize();
            OnPropertyChange("Output");
        }

        public void Initialize()
        {
            Output = (settings.Value == 1);
            presentation.Value = Output;
            presentation.update();
            settings_OnPluginStatusChanged(this, new StatusEventArgs(StatusChangedMode.ImageUpdate, settings.Value));
        }

        public void PostExecution()
        {
        }

        public void PreExecution()
        {
        }

        public UserControl Presentation
        {
            get { return this.presentation; }
            private set { this.presentation = (ButtonInputPresentation)value; }
        }

        public ISettings Settings
        {
            get { return this.settings; }
            set { this.settings = (BooleanInputSettings)value; }
        }

        public void Stop()
        {
        }

        #endregion

        #region event handling

        public event GuiLogNotificationEventHandler OnGuiLogNotificationOccured;
        public event PluginProgressChangedEventHandler OnPluginProgressChanged;
        public event StatusChangedEventHandler OnPluginStatusChanged;
        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChange(String propertyname)
        {
            EventsHelper.PropertyChanged(PropertyChanged, this, new PropertyChangedEventArgs(propertyname));
        }

        private void settings_OnPluginStatusChanged(IPlugin sender, StatusEventArgs args)
        {
            if (OnPluginStatusChanged != null) OnPluginStatusChanged(this, args);
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