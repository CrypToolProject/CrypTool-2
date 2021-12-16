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
using CrypTool.PluginBase.Attributes;
using CrypTool.PluginBase.Miscellaneous;
using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace CrypTool.Plugins.Collector
{
    [Author("Armin Krauß", "krauss@CrypTool.org", "", "")]
    [PluginInfo("Collector.Properties.Resources", "PluginCaption", "PluginTooltip", "Collector/DetailedDescription/doc.xml", new[] { "CrypWin/images/default.png" })]
    [AutoAssumeFullEndProgress(false)]
    [ComponentCategory(ComponentCategory.ToolsDataflow)]
    public class Collector : ICrypComponent
    {
        #region IPlugin Members

        private object _objInput;
        private int _size;
        private readonly List<object> _arrayOutput = new List<object>();
        private bool firstrun;

        #region In and Out properties

        [PropertyInfo(Direction.InputData, "ObjInputCaption", "ObjInputTooltip", true)]
        public object ObjInput
        {
            get => _objInput;
            set => _objInput = value;//if (_arrayOutput.Count < Size)//{//    _arrayOutput.Add(value);//    OnPropertyChanged("ObjInput");//    if (_arrayOutput.Count == Size)//        OnPropertyChanged("ArrayOutput");//}
        }

        [PropertyInfo(Direction.InputData, "SizeCaption", "SizeTooltip", true)]
        public int Size
        {
            get => _size;
            set
            {
                _size = value;
                OnPropertyChanged("Size");
            }
        }

        [PropertyInfo(Direction.OutputData, "ArrayOutputCaption", "ArrayOutputTooltip")]
        public Array ArrayOutput => _arrayOutput.ToArray();

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
            _arrayOutput.Clear();
            firstrun = true;
        }

        public void Execute()
        {
            if (firstrun)
            {
                firstrun = false;
                _arrayOutput.Clear();
            }

            if (Size == null)
            {
                GuiLogMessage("Please provide the size of the requested array.", NotificationLevel.Error);
                return;
            }

            if (_arrayOutput.Count < Size)
            {
                _arrayOutput.Add(ObjInput);

                if (_arrayOutput.Count == Size)
                {
                    OnPropertyChanged("ArrayOutput");
                }
            }

            ProgressChanged(_arrayOutput.Count, Size);
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