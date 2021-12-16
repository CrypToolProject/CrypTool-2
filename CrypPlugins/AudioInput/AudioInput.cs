/*
   Copyright 2015 Nils Kopal, Applied Information Security, University of Kassel

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
using NAudio.Wave;
using System;
using System.ComponentModel;

namespace CrypTool.Plugins.AudioInput
{
    [Author("Nils Kopal", "nils.kopal@uni-kassel.de", "University of Kassel", "")]
    [PluginInfo("CrypTool.Plugins.AudioInput.Properties.Resources", "PluginCaption", "PluginTooltip", null, "AudioInput/icon.png")]
    [ComponentCategory(ComponentCategory.ToolsDataInputOutput)]
    public class AudioInput : ICrypComponent
    {
        private WaveInEvent recorder;
        private readonly AudioInputSettings settings = new AudioInputSettings();

        [PropertyInfo(Direction.OutputData, "AudioOutputCaption", "AudioOutputTooltip")]
        public byte[] AudioOutput
        {
            get;
            set;
        }

        public void PreExecution()
        {
            recorder = new WaveInEvent
            {
                DeviceNumber = settings.DeviceChoice
            };
        }

        public void PostExecution()
        {

        }

        public event StatusChangedEventHandler OnPluginStatusChanged;

        public event PluginProgressChangedEventHandler OnPluginProgressChanged;

        public event GuiLogNotificationEventHandler OnGuiLogNotificationOccured;

        public ISettings Settings => settings;

        public System.Windows.Controls.UserControl Presentation => null;

        public void Execute()
        {
            recorder.DataAvailable += recorder_DataAvailable;
            recorder.StartRecording();
        }

        private void recorder_DataAvailable(object sender, WaveInEventArgs waveInEventArgs)
        {
            byte[] output = new byte[waveInEventArgs.BytesRecorded];
            Array.Copy(waveInEventArgs.Buffer, output, waveInEventArgs.BytesRecorded);
            AudioOutput = output;
            OnPropertyChanged("AudioOutput");
        }

        public void Stop()
        {
            recorder.DataAvailable -= recorder_DataAvailable;
            recorder.StopRecording();
        }

        public void Initialize()
        {

        }

        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;

        public void Dispose()
        {

        }

        private void OnPropertyChanged(string name)
        {
            EventsHelper.PropertyChanged(PropertyChanged, this, new PropertyChangedEventArgs(name));
        }

        private void GuiLogMessage(string p, NotificationLevel notificationLevel)
        {
            EventsHelper.GuiLogMessage(OnGuiLogNotificationOccured, this, new GuiLogEventArgs(p, this, notificationLevel));
        }
    }
}
