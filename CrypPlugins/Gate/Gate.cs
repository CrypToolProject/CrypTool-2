/*
   Copyright 2009 Matthäus Wander, Universität Duisburg-Essen

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
using System.Runtime.Remoting.Contexts;

namespace Gate
{
    [Author("Matthäus Wander", "wander@CrypTool.org", "Universität Duisburg-Essen, Fachgebiet Verteilte Systeme", "http://www.vs.uni-due.de")]
    [PluginInfo("Gate.Properties.Resources", "PluginCaption", "PluginTooltip", "Gate/DetailedDescription/doc.xml", "Gate/gate_closed_32.png", "Gate/gate_open_32.png")]
    [Synchronization]
    [ComponentCategory(ComponentCategory.ToolsDataflow)]
    public class Gate : ICrypComponent
    {
        private GateSettings settings = new GateSettings();
        private object input;
        private object output;
        private bool oldControl = false;
        private bool control = false;

        private bool freshInput = false;
        private bool freshControl = false;

        private int _counter = 1;
        private int _maxCounter = 100;

        [PropertyInfo(Direction.InputData, "InputObjectCaption", "InputObjectTooltip", true)]
        public object InputObject
        {
            get => input;
            set
            {
                input = value;
                freshInput = true;
                OnPropertyChanged("InputObject");
            }
        }

        [PropertyInfo(Direction.InputData, "ControlCaption", "ControlTooltip", false)]
        public bool Control
        {
            get => control;
            set
            {
                oldControl = control;
                control = value;
                freshControl = true;
                OnPropertyChanged("Control");
            }
        }

        [PropertyInfo(Direction.OutputData, "OutputObjectCaption", "OutputObjectTooltip")]
        public object OutputObject => output;

        #region IPlugin Members

        public event StatusChangedEventHandler OnPluginStatusChanged;

        public event GuiLogNotificationEventHandler OnGuiLogNotificationOccured;

        public event PluginProgressChangedEventHandler OnPluginProgressChanged;

        public CrypTool.PluginBase.ISettings Settings
        {
            get => settings;
            set => settings = (GateSettings)value;
        }

        public System.Windows.Controls.UserControl Presentation => null;

        public void PreExecution()
        {
            input = null;
            output = null;
            oldControl = false;
            control = false;
            _counter = 1;
            _maxCounter = settings.MaxCounter;
        }

        public void Execute()
        {
            if (freshInput)
            {
                _counter++;
            }

            if (shallFire())
            {
                output = input;
                freshControl = false;
                freshInput = false;
                ProgressChanged(1, 1);
                OnPropertyChanged("OutputObject");

                iconOpen();
            }
            else
            {
                output = null;
                ProgressChanged(0.5, 1);

                iconClosed();
            }
        }

        private bool shallFire()
        {
            switch (settings.Trigger)
            {
                case Trigger.AlwaysOpen:
                    return true;
                case Trigger.AlwaysClosed:
                    return false;
                case Trigger.TrueValue:
                    return freshInput && freshControl && control;
                case Trigger.FalseValue:
                    return freshInput && freshControl && !control;
                case Trigger.AnyEdge:
                    return freshInput && freshControl && control != oldControl;
                case Trigger.PositiveEdge:
                    return freshInput && freshControl && !oldControl && control;
                case Trigger.NegativeEdge:
                    return freshInput && freshControl && oldControl && !control;
                case Trigger.Counter:
                    return freshInput && _counter <= _maxCounter;
                default:
                    return false;
            }
        }

        private void iconOpen()
        {
            iconSet(1);
        }

        private void iconClosed()
        {
            iconSet(0);
        }

        private void iconSet(int index)
        {
            if (OnPluginStatusChanged != null)
            {
                OnPluginStatusChanged(this, new StatusEventArgs(StatusChangedMode.ImageUpdate, index));
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

        #endregion

        private void GuiLogMessage(string message, NotificationLevel logLevel)
        {
            EventsHelper.GuiLogMessage(OnGuiLogNotificationOccured, this, new GuiLogEventArgs(message, this, logLevel));
        }

        private void OnPropertyChanged(string name)
        {
            EventsHelper.PropertyChanged(PropertyChanged, this, new PropertyChangedEventArgs(name));
        }

        private void ProgressChanged(double value, double max)
        {
            EventsHelper.ProgressChanged(OnPluginProgressChanged, this, new PluginProgressEventArgs(value, max));
        }
    }
}
