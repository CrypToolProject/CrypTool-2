/*
   Copyright 2009 Sören Rinne, Ruhr-Universität Bochum, Germany

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
//timer
using System.Timers;
// for QuickwatchPresentaton
using System.Windows;
using System.Windows.Controls;
// for mouse click
using System.Windows.Input;
// for setting image uri programmatically

//using System.Windows.Data;
//using System.IO;
//using System.Runtime.CompilerServices;

namespace CrypTool.CLK
{
    [Author("Soeren Rinne", "soeren.rinne@CrypTool.de", "Ruhr-Universitaet Bochum, Chair for System Security", "http://www.trust.rub.de/")]
    [PluginInfo("CrypTool.Plugins.CLK.Properties.Resources", "PluginCaption", "PluginTooltip", "CLK/DetailedDescription/doc.xml", "CLK/icon.png", "CLK/Images/true.png", "CLK/Images/false.png")]
    [ComponentCategory(ComponentCategory.ToolsDataflow)]
    public class CLK : DependencyObject, ICrypComponent
    {
        #region private variables
        private readonly CLKPresentation cLKPresentation;
        private bool output;
        private int roundOutput;
        private object eventInput;
        private int timeout = 2000;
        private int rounds = 10;
        private readonly Timer aTimer = new Timer();
        #endregion private variables

        public int myRounds;
        public DateTime startTime;

        public CLK()
        {
            settings = new CLKSettings();
            settings.PropertyChanged += settings_PropertyChanged;

            cLKPresentation = new CLKPresentation();
            Presentation = cLKPresentation;

            cLKPresentation.CLKButtonImage.MouseLeftButtonUp += cLKButton_MouseLeftButtonUp;
            cLKPresentation.CLKButtonImage.MouseLeftButtonDown += cLKButton_MouseLeftButtonUp;
        }

        private void cLKButton_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            OnPropertyChanged("Output");
        }

        private void settings_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "SetClockToTrue")
            {
                output = Convert.ToBoolean(settings.SetClockToTrue);
                if (output)
                {
                    StatusChanged((int)CLKImage.True);
                    cLKPresentation.setImageTrue();
                }
                else
                {
                    StatusChanged((int)CLKImage.False);
                    cLKPresentation.setImageFalse();
                }
            }
            if (e.PropertyName == "CLKTimeout")
            {
                timeout = settings.CLKTimeout;
            }
            if (e.PropertyName == "Rounds")
            {
                rounds = settings.Rounds;
            }
        }


        #region public interface

        [PropertyInfo(Direction.OutputData, "OutputCaption", "OutputTooltip", false)]
        public bool Output
        {
            get => output;
            set
            {
                if (value != output)
                {
                    output = value;
                    OnPropertyChanged("Output");
                }
            }
        }

        [PropertyInfo(Direction.OutputData, "RoundOutputCaption", "RoundOutputTooltip", false)]
        public int RoundOutput
        {
            get => roundOutput;
            set
            {
                if (value != roundOutput)
                {
                    roundOutput = value;
                    OnPropertyChanged("RoundOutput");
                }
            }
        }

        [PropertyInfo(Direction.InputData, "EventInputCaption", "EventInputTooltip", false)]
        public object EventInput
        {
            get => eventInput;
            set
            {
                eventInput = value;
                OnPropertyChanged("EventInput");
            }
        }


        #endregion public interface

        #region IPlugin Members
        public event StatusChangedEventHandler OnPluginStatusChanged;
        public event PluginProgressChangedEventHandler OnPluginProgressChanged;
        private void ProgressChanged(double value, double max)
        {
            EventsHelper.ProgressChanged(OnPluginProgressChanged, this, new PluginProgressEventArgs(value, max));
        }

        private readonly CLKSettings settings;
        public ISettings Settings => settings;

        public UserControl Presentation { get; private set; }

        public void PreExecution()
        {
            if (Convert.ToBoolean(settings.SetClockToTrue))
            {
                StatusChanged((int)CLKImage.True);
                cLKPresentation.setImageTrue();
            }
            else
            {
                StatusChanged((int)CLKImage.False);
                cLKPresentation.setImageFalse();
            }

            myRounds = settings.Rounds;
            startTime = DateTime.Now;
        }

        public void Execute()
        {
            if (settings.UseEvent)
            {
                if (myRounds != 0)
                {
                    OnPropertyChanged("Output");
                    roundOutput = myRounds;
                    OnPropertyChanged("RoundOutput");
                    myRounds--;
                    ProgressChanged(settings.Rounds - myRounds, settings.Rounds);
                }
                else
                {
                    // stop counter
                    DateTime stopTime = DateTime.Now;
                    // compute overall time
                    TimeSpan duration = stopTime - startTime;
                    GuiLogMessage("Overall time used: " + duration, NotificationLevel.Debug);
                }
            }
            else
            {
                if (settings.CLKTimeout >= 0)
                {                
                    process(settings.CLKTimeout);
                    //change picture
                    if (Convert.ToBoolean(settings.SetClockToTrue))
                    {
                        StatusChanged((int)CLKImage.True);
                    }
                    else
                    {
                        StatusChanged((int)CLKImage.False);
                    }
                }
            }
        }

        private void process(int timeout)
        {
            // check if rounds are more than zero
            if (myRounds != 0)
            {
                // first fire up an event, then get the timer to handle that for us
                OnPropertyChanged("Output");
                roundOutput = myRounds;
                OnPropertyChanged("RoundOutput");
                myRounds--;

                // Hook up the Elapsed event for the timer.
                aTimer.Elapsed += new ElapsedEventHandler(sendCLKSignal);

                // Set the Interval to 'timeout' seconds (in milliseconds).
                aTimer.Interval = timeout;
                aTimer.Enabled = true;

                // Keep the timer alive until the end of Main.
                //GC.KeepAlive(aTimer);
            }
        }

        private void sendCLKSignal(object sender, EventArgs e)
        {
            if (myRounds != 0)
            {
                OnPropertyChanged("Output");
                roundOutput = myRounds;
                OnPropertyChanged("RoundOutput");
                myRounds--;
                ProgressChanged(settings.Rounds - myRounds, settings.Rounds);
            }
            else
            {
                // disable timer
                aTimer.Enabled = false;
                // globally remove timer event
                aTimer.Elapsed -= new ElapsedEventHandler(sendCLKSignal);
            }

        }

        public void PostExecution()
        {
        }

        public void Stop()
        {
            // disable timer
            aTimer.Enabled = false;
            // globally remove timer event
            aTimer.Elapsed -= new ElapsedEventHandler(sendCLKSignal);
        }

        public void Initialize()
        {
            output = Convert.ToBoolean(settings.SetClockToTrue);
            if (output)
            {
                StatusChanged((int)CLKImage.True);
            }
            else
            {
                StatusChanged((int)CLKImage.False);
            }

            settings.CLKTimeout = timeout;
            settings.UpdateTaskPaneVisibility();
        }

        public void Dispose()
        {
        }

        #endregion

        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;

        public event GuiLogNotificationEventHandler OnGuiLogNotificationOccured;
        private void GuiLogMessage(string message, NotificationLevel logLevel)
        {
            EventsHelper.GuiLogMessage(OnGuiLogNotificationOccured, this, new GuiLogEventArgs(message, this, logLevel));
        }

        protected void OnPropertyChanged(string name)
        {
            EventsHelper.PropertyChanged(PropertyChanged, this, new PropertyChangedEventArgs(name));
        }

        private void StatusChanged(int imageIndex)
        {
            EventsHelper.StatusChanged(OnPluginStatusChanged, this, new StatusEventArgs(StatusChangedMode.ImageUpdate, imageIndex));
        }

        #endregion
    }

    internal enum CLKImage
    {
        Default,
        True,
        False
    }
}
