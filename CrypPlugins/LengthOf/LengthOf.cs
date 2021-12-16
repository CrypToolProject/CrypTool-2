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
using System.Collections;
using System.ComponentModel;

// This PlugIn accepts every type. For arrays the number of elements is written.
// For everything else the number of characters of the object's string representation is shown.
namespace CrypTool.Plugins.LengthOf
{
    [Author("Christian Arnold", "christian.arnold@stud.uni-due.de", "Uni Duisburg-Essen", "http://www.uni-due.de")]
    [PluginInfo("LengthOf.Properties.Resources", "PluginCaption", "PluginTooltip", "LengthOf/DetailedDescription/doc.xml", "LengthOf/LenOf.png")]
    [ComponentCategory(ComponentCategory.ToolsMisc)]
    public class LengthOf : ICrypComponent
    {
        private object objInput = null;
        private int outputLen = 0;

        #region IPlugin Members

        public void Execute()
        {
            Progress(0.0, 1.0);

            if (ObjInput != null)
            {
                if (ObjInput is Array)
                {
                    OutputLen = (ObjInput as Array).Length;
                    GuiLogMessage("Object is an array with " + OutputLen + " entries.", NotificationLevel.Debug);
                }
                else if (ObjInput is IDictionary)
                {
                    OutputLen = (ObjInput as IDictionary).Count;
                    GuiLogMessage("Object is a dictionary with " + OutputLen + " items.", NotificationLevel.Debug);
                }
                else //no array or dictionary
                {
                    OutputLen = ObjInput.ToString().Length;
                    GuiLogMessage("Object isn't an array or dictionary. Length: " + OutputLen, NotificationLevel.Debug);
                }
            }

            Progress(1.0, 1.0);
        }

        [PropertyInfo(Direction.InputData, "ObjInputCaption", "ObjInputTooltip", true)]
        public object ObjInput
        {
            get => objInput;
            set
            {
                objInput = value;
                OnPropertyChanged("ObjInput");
            }
        }

        [PropertyInfo(Direction.OutputData, "OutputLenCaption", "OutputLenTooltip")]
        public int OutputLen
        {
            get => outputLen;
            set
            {
                outputLen = value;
                OnPropertyChanged("OutputLen");
            }
        }

        public void Dispose()
        {
        }

        public void Initialize()
        {
        }

        public event GuiLogNotificationEventHandler OnGuiLogNotificationOccured;

        public event StatusChangedEventHandler OnPluginStatusChanged;

        public event PluginProgressChangedEventHandler OnPluginProgressChanged;

        private void Progress(double value, double max)
        {
            EventsHelper.ProgressChanged(OnPluginProgressChanged, this, new PluginProgressEventArgs(value, max));
        }

        public void PostExecution()
        {
        }

        public void PreExecution()
        {
        }

        public System.Windows.Controls.UserControl Presentation => null;

        public CrypTool.PluginBase.ISettings Settings => null;

        public void Stop()
        {

        }

        #endregion

        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;

        public void OnPropertyChanged(string name)
        {
            EventsHelper.PropertyChanged(PropertyChanged, this, new PropertyChangedEventArgs(name));
        }

        private void GuiLogMessage(string p, NotificationLevel notificationLevel)
        {
            EventsHelper.GuiLogMessage(OnGuiLogNotificationOccured, this, new GuiLogEventArgs(p, this, notificationLevel));
        }
        #endregion
    }
}
