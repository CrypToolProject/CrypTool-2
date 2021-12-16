/*
   Copyright 2009 Christian Arnold, Uni Duisburg-Essen

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

namespace CrypTool.Plugins.Serializer
{
    [Author("Armin Krauß", "krauss@CrypTool.org", "", "")]
    [PluginInfo("Serializer.Properties.Resources", "PluginCaption", "PluginTooltip", "Serializer/DetailedDescription/doc.xml", new[] { "CrypWin/images/default.png" })]
    [ComponentCategory(ComponentCategory.ToolsDataflow)]
    public class Serializer : ICrypComponent
    {
        #region IPlugin Members

        private Array _objInput;
        private object _objOutput;

        #region In and Out properties

        [PropertyInfo(Direction.InputData, "ObjInputCaption", "ObjInputTooltip", true)]
        public Array ObjInput
        {
            get => _objInput;
            set
            {
                _objInput = value;
                OnPropertyChanged("ObjInput");
                OnPropertyChanged("Length");
            }
        }

        [PropertyInfo(Direction.OutputData, "ObjOutputCaption", "ObjOutputTooltip")]
        public object ObjOutput
        {
            get => _objOutput;
            set
            {
                _objOutput = value;
                OnPropertyChanged("ObjOutput");
            }
        }

        [PropertyInfo(Direction.OutputData, "LengthCaption", "LengthTooltip")]
        public int Length => ObjInput.Length;

        #endregion

        public event StatusChangedEventHandler OnPluginStatusChanged;

        public event GuiLogNotificationEventHandler OnGuiLogNotificationOccured;

        public event PluginProgressChangedEventHandler OnPluginProgressChanged;

        public CrypTool.PluginBase.ISettings Settings
        {
            get => null;
            set { }
        }

        public System.Windows.Controls.UserControl Presentation => null;

        public void PreExecution()
        {
        }

        public void Execute()
        {
            if (ObjInput != null)
            {
                foreach (object obj in ObjInput)
                {
                    ObjOutput = obj;
                }
            }
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

        #endregion

        #region INotifyPropertyChanged Members

        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;

        public void OnPropertyChanged(string name)
        {
            EventsHelper.PropertyChanged(PropertyChanged, this, new PropertyChangedEventArgs(name));
        }

        public event PluginProgressChangedEventHandler OnPluginProcessChanged;

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