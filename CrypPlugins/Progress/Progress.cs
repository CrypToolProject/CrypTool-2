/*
   Copyright 2011 Matthäus Wander, University of Duisburg-Essen

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
using System.ComponentModel;
using System.Windows.Controls;
using CrypTool.PluginBase;
using CrypTool.PluginBase.Attributes;
using CrypTool.PluginBase.Miscellaneous;

namespace CrypTool.Progress
{
    [Author("Sven Rech", "rech@CrypTool.org", "University of Kassel", "http://www.uni-kassel.de/eecs/fachgebiete/uc")]
    [PluginInfo("CrypTool.Progress.Properties.Resources", "PluginCaption", "PluginTooltip", "Progress/DetailedDescription/doc.xml",
      "Progress/Images/icon.png")]
    [ComponentCategory(ComponentCategory.ToolsMisc)]
    [ComponentVisualAppearance(ComponentVisualAppearance.VisualAppearanceEnum.Opened)]
    [AutoAssumeZeroBeginProgress(false)]
    [AutoAssumeFullEndProgress(false)]
    public class Progress : ICrypComponent
    {
        private double _lastValue = 0;

        private readonly ProgressPresentation _progressPresentation = new ProgressPresentation();

        /// <summary>
        /// Get or set all settings for this algorithm
        /// </summary>
        public ISettings Settings => null;

        [PropertyInfo(Direction.InputData, "ValueCaption", "ValueTooltip", true)]
        public int Value { get; set; }

        [PropertyInfo(Direction.InputData, "MaxCaption", "MaxTooltip", true)]
        public int Max { get; set; }

        public void Initialize()
        {
        }

        public void Dispose()
        {
        }

        /// <summary>
        /// Fire, if progress bar has to be updated
        /// </summary>
        public event PluginProgressChangedEventHandler OnPluginProgressChanged;

        private void ProgressChange(double value, double max)
        {
            EventsHelper.ProgressChanged(OnPluginProgressChanged, this, new PluginProgressEventArgs(value, max));
        }

        /// <summary>
        /// Fire, if new message has to be shown in the status bar
        /// </summary>
        public event GuiLogNotificationEventHandler OnGuiLogNotificationOccured;

        public UserControl Presentation => _progressPresentation;

        public void Stop()
        {
            _progressPresentation.Set(0, 1);
        }

        public void PostExecution()
        {
        }

        public void PreExecution()
        {
            _lastValue = 0;
            _progressPresentation.Set(0, 1);
        }

#pragma warning disable 67
        public event StatusChangedEventHandler OnPluginStatusChanged;
#pragma warning restore

        public void Execute()
        {
            double value = ((double)Value / Max);
            if (Math.Abs(value - _lastValue) > 0.01 || value == 1)
            {
                _progressPresentation.Set(Value, Max);                
                _lastValue = value;
                ProgressChange(Value, Max);
            }            
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public void OnPropertyChanged(string name)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(name));
            }
        }

        private void GuiLogMessage(string msg, NotificationLevel logLevel)
        {
            if (OnGuiLogNotificationOccured != null)
            {
                OnGuiLogNotificationOccured(this, new GuiLogEventArgs(msg, this, logLevel));
            }
        }
    }
}
