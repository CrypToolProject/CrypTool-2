/*
   Copyright 2008 Thomas Schmid, University of Siegen

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

namespace IncDec
{
    [Author("Thomas Schmid", "thomas.schmid@CrypTool.org", "Uni Siegen", "http://www.uni-siegen.de")]
    [PluginInfo("IncDec.Properties.Resources", "PluginCaption", "PluginTooltip", "IncDec/DetailedDescription/doc.xml", "IncDec/increment.png", "IncDec/decrement.png")]
    [ComponentCategory(ComponentCategory.ToolsMisc)]
    public class IncDec : ICrypComponent
    {
        private readonly IncDecSettings settings = new IncDecSettings();
        private int input;

        public IncDec()
        {
            settings.PropertyChanged += settings_PropertyChanged;
        }

        private void settings_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "ModeSelect")
            {
                switch (settings.CurrentMode)
                {
                    case IncDecSettings.Operator.Increment:
                        EventsHelper.StatusChanged(OnPluginStatusChanged, this, new StatusEventArgs(StatusChangedMode.ImageUpdate, 0));
                        break;
                    case IncDecSettings.Operator.Decrement:
                        EventsHelper.StatusChanged(OnPluginStatusChanged, this, new StatusEventArgs(StatusChangedMode.ImageUpdate, 1));
                        break;
                    default:
                        break;
                }
            }
        }


        [PropertyInfo(Direction.InputData, "InputCaption", "InputTooltip", false)]
        public int Input
        {
            get => input;
            set
            {
                Progress(50, 100);
                input = value;
                int returnValue = 0;
                switch (settings.CurrentMode)
                {
                    case IncDecSettings.Operator.Increment:
                        returnValue = input + settings.Value;
                        break;
                    case IncDecSettings.Operator.Decrement:
                        returnValue = input - settings.Value;
                        break;
                    default:
                        break;
                }
                output = returnValue;

                OnPropertyChanged("Input");
                OnPropertyChanged("Output");
                Progress(100, 100);
            }
        }

        private int output;
        [PropertyInfo(Direction.OutputData, "OutputCaption", "OutputTooltip", false)]
        public int Output
        {
            get => output;
            set { } // readonly
        }


        #region IPlugin Members

#pragma warning disable 67
        public event StatusChangedEventHandler OnPluginStatusChanged;
        public event GuiLogNotificationEventHandler OnGuiLogNotificationOccured;
        public event PluginProgressChangedEventHandler OnPluginProgressChanged;
#pragma warning restore

        private void Progress(double value, double max)
        {
            EventsHelper.ProgressChanged(OnPluginProgressChanged, this, new PluginProgressEventArgs(value, max));
        }

        public CrypTool.PluginBase.ISettings Settings => settings;

        public UserControl Presentation => null;

        public void PreExecution()
        {
        }

        public void Execute()
        {
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

        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged(string name)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(name));
            }
        }

        #endregion
    }
}
