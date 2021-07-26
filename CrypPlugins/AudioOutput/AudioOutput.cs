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
using System;
using System.ComponentModel;
using CrypTool.PluginBase;
using CrypTool.PluginBase.Miscellaneous;
using NAudio.Wave;

namespace CrypTool.Plugins.AudioOutput
{
    [Author("Nils Kopal", "nils.kopal@uni-kassel.de", "University of Kassel", "")]
    [PluginInfo("CrypTool.Plugins.AudioOutput.Properties.Resources", "PluginCaption", "PluginTooltip", null, "AudioOutput/icon.png")]
    [ComponentCategory(ComponentCategory.ToolsDataInputOutput)]
    public class AudioOutput : ICrypComponent
    {
        private WaveOutEvent player;
        private AudioOutputSettings settings = new AudioOutputSettings();
        private BufferedWaveProvider provider;
        private WaveInEvent recorder = new WaveInEvent();

        [PropertyInfo(Direction.InputData, "AudioInputCaption", "AudioInputTooltip", true)]
        public byte[] AudioInput
        {
            get;
            set;
        }

        public void PreExecution()
        {
            player = new WaveOutEvent();
            provider = new BufferedWaveProvider(recorder.WaveFormat);
            player.DeviceNumber = settings.DeviceChoice;
            provider.BufferDuration = new TimeSpan(0, 0, 0, 0, settings.BufferSize);
            provider.DiscardOnBufferOverflow = true;
            player.Init(provider);
            player.Play();
            
        }

        public void PostExecution()
        {
            
        }

        public event StatusChangedEventHandler OnPluginStatusChanged;

        public event PluginProgressChangedEventHandler OnPluginProgressChanged;

        public event GuiLogNotificationEventHandler OnGuiLogNotificationOccured;

        public ISettings Settings
        {
            get { return settings; }
        }

        public System.Windows.Controls.UserControl Presentation
        {
            get { return null; }
        }

        public void Execute()
        {           
            provider.AddSamples(AudioInput, 0, AudioInput.Length);
        }
     

        public void Stop()
        {
            player.Stop();
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
