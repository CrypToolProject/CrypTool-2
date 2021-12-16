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
using OxyPlot;
using System;
using System.ComponentModel;
using System.Threading;
using System.Windows.Threading;

namespace CrypTool.Plugins.AudioOutput
{
    [Author("Nils Kopal", "nils.kopal@uni-kassel.de", "University of Kassel", "")]
    [PluginInfo("CrypTool.Plugins.AudioOutput.Properties.Resources", "PluginCaption", "PluginTooltip", null, "AudioOutput/icon.png")]
    [ComponentCategory(ComponentCategory.ToolsDataInputOutput)]
    public class AudioOutput : ICrypComponent
    {
        private WaveOutEvent player;
        private readonly AudioOutputSettings settings = new AudioOutputSettings();
        private BufferedWaveProvider provider;
        private readonly WaveInEvent recorder = new WaveInEvent();
        private readonly AudioOutputPresentation _presentation = new AudioOutputPresentation();

        private int _x = 0;
        private const int STEPS = 8;
        private const int MAX_POINTS = 8000;


        [PropertyInfo(Direction.InputData, "AudioInputCaption", "AudioInputTooltip", true)]
        public byte[] AudioInput
        {
            get;
            set;
        }

        public void PreExecution()
        {
            ClearPresentation();
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

        private void ClearPresentation()
        {
            _x = 0;
            _presentation.Points.Clear();
            //initialize list with 0-value points
            for (int i = -1 * STEPS * MAX_POINTS * 2; i < 0; i += STEPS * 2)
            {
                _presentation.Points.Add(new DataPoint(i, 0));
            }
            _presentation.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
            {
                _presentation.Plot.InvalidatePlot(true);
            }, null);
        }

        public event StatusChangedEventHandler OnPluginStatusChanged;

        public event PluginProgressChangedEventHandler OnPluginProgressChanged;

        public event GuiLogNotificationEventHandler OnGuiLogNotificationOccured;

        public ISettings Settings => settings;

        public System.Windows.Controls.UserControl Presentation => _presentation;

        public void Execute()
        {
            provider.AddSamples(AudioInput, 0, AudioInput.Length);
            for (int i = 0; i < AudioInput.Length - 2; i += 2 * STEPS)
            {
                short value = BitConverter.ToInt16(AudioInput, i);

                _presentation.Points.Add(new DataPoint(_x, value));
                _x += STEPS;
            }
            if (_presentation.Points.Count > MAX_POINTS)
            {
                _presentation.Points.RemoveRange(0, _presentation.Points.Count - MAX_POINTS);
            }

            _presentation.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
            {
                _presentation.Plot.InvalidatePlot(true);
            }, null);

            //Now block until sound has been played
            Thread.Sleep(AudioInput.Length / 2 / 8); //we have 8 samples per millisecond
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
