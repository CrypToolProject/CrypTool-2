/*
   Copyright 2019 Christian Bender christian1.bender@student.uni-siegen.de

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
using DCAPathVisualiser;
using DCAPathVisualiser.Logic;
using DCAPathVisualiser.Properties;
using DCAPathVisualiser.UI;
using Newtonsoft.Json;
using System;
using System.ComponentModel;
using System.Threading;
using System.Windows.Controls;
using System.Windows.Threading;

namespace CrypTool.Plugins.DCAPathVisualiser
{
    [Author("Christian Bender", "christian1.bender@student.uni-siegen.de", null, "http://www.uni-siegen.de")]
    [PluginInfo("DCAPathVisualiser.Properties.Resources", "PluginCaption", "PluginTooltip", "DCAPathVisualiser/userdoc.xml", new[] { "DCAPathVisualiser/Images/IC_PathVisualizer.png" })]
    [ComponentCategory(ComponentCategory.CryptanalysisSpecific)]
    public class DCAPathVisualiser : ICrypComponent
    {
        #region Private Variables

        private readonly DCAPathVisualiserSettings _settings = new DCAPathVisualiserSettings();
        private readonly PathVisualiserPres _pres = new PathVisualiserPres();
        private string _differential;

        #endregion

        public DCAPathVisualiser()
        {
            _settings.PropertyChanged += SettingChangedListener;

            //Check specific algorithm and invoke the selection into the UI class
            if (_settings.CurrentAlgorithm == Algorithms.Cipher1)
            {
                //dispatch action: clear the active grid and add the specific algorithm visualization
                _pres.Dispatcher.Invoke(DispatcherPriority.Send,
                    (SendOrPostCallback)delegate { _pres.CurrentAlgorithm = Algorithms.Cipher1; }, null);
            }
            else if (_settings.CurrentAlgorithm == Algorithms.Cipher2)
            {
                _pres.Dispatcher.Invoke(DispatcherPriority.Send,
                    (SendOrPostCallback)delegate { _pres.CurrentAlgorithm = Algorithms.Cipher2; }, null);
            }
            else if (_settings.CurrentAlgorithm == Algorithms.Cipher3)
            {
                _pres.Dispatcher.Invoke(DispatcherPriority.Send,
                    (SendOrPostCallback)delegate { _pres.CurrentAlgorithm = Algorithms.Cipher3; }, null);
            }
        }

        #region Data Properties

        /// <summary>
        /// Input for the differential
        /// </summary>
        [PropertyInfo(Direction.InputData, "DifferentialInput", "DifferentialInputToolTip", true)]
        public string Differential
        {
            get => _differential;
            set
            {
                _differential = value;
                OnPropertyChanged("Differential");
            }
        }

        #endregion

        #region IPlugin Members

        /// <summary>
        /// Provide plugin-related parameters (per instance) or return null.
        /// </summary>
        public ISettings Settings => _settings;

        /// <summary>
        /// Provide custom presentation to visualize the execution or return null.
        /// </summary>
        public UserControl Presentation => _pres;

        /// <summary>
        /// Called once when workflow execution starts.
        /// </summary>
        public void PreExecution()
        {
            //dispatch action: inform ui that workspace is running
            _pres.Dispatcher.Invoke(DispatcherPriority.Send, (SendOrPostCallback)delegate
            {
                _pres.WorkspaceRunning = true;
                _pres.CurrentAlgorithm = _settings.CurrentAlgorithm;
            }, null);
        }

        /// <summary>
        /// Called every time this plugin is run in the workflow execution.
        /// </summary>
        public void Execute()
        {
            ProgressChanged(0, 1);

            //check for null
            if (Differential == null)
            {
                return;
            }

            DifferentialAttackRoundConfiguration conf = ReadConfiguration(Differential);

            //check component setting
            if (conf.SelectedAlgorithm != _settings.CurrentAlgorithm)
            {
                GuiLogMessage(Resources.WarningWrongAlgorithm, NotificationLevel.Warning);
                ProgressChanged(1, 1);
                return;
            }

            if (conf != null)
            {
                _pres.Dispatcher.Invoke(DispatcherPriority.Send, (SendOrPostCallback)delegate
                {
                    _pres.CurrentConfigurationToDisplay = conf;
                    _pres.RenderView();
                }, null);
            }

            ProgressChanged(1, 1);
        }

        /// <summary>
        /// Called once after workflow execution has stopped.
        /// </summary>
        public void PostExecution()
        {
            //dispatch action: inform ui that workspace is not running
            _pres.Dispatcher.Invoke(DispatcherPriority.Send, (SendOrPostCallback)delegate
            {
                _pres.WorkspaceRunning = false;
            }, null);
        }

        /// <summary>
        /// Triggered time when user clicks stop button.
        /// Shall abort long-running execution.
        /// </summary>
        public void Stop()
        {
        }

        /// <summary>
        /// Called once when plugin is loaded into editor workspace.
        /// </summary>
        public void Initialize()
        {
        }

        /// <summary>
        /// Called once when plugin is removed from editor workspace.
        /// </summary>
        public void Dispose()
        {
        }

        #endregion

        #region methods

        /// <summary>
        /// Reads json string and returns it as object
        /// </summary>
        /// <param name="json"></param>
        /// <returns></returns>
        private DifferentialAttackRoundConfiguration ReadConfiguration(string json)
        {
            DifferentialAttackRoundConfiguration config = null;

            json = json.Replace("DCAPathFinder", "DCAPathVisualiser");

            try
            {
                config = JsonConvert.DeserializeObject<DifferentialAttackRoundConfiguration>(json, new JsonSerializerSettings
                {
                    TypeNameHandling = TypeNameHandling.Auto,
                    NullValueHandling = NullValueHandling.Ignore,
                });

            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }



            return config;
        }

        /// <summary>
        /// Handles changes within the settings class
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SettingChangedListener(object sender, PropertyChangedEventArgs e)
        {
            //Listen for changes of the current chosen algorithm
            if (e.PropertyName == "CurrentAlgorithm")
            {
                //Check specific algorithm and invoke the selection into the UI class
                if (_settings.CurrentAlgorithm == Algorithms.Cipher1)
                {
                    //dispatch action: clear the active grid and add the specific algorithm visualization
                    _pres.Dispatcher.Invoke(DispatcherPriority.Send,
                         (SendOrPostCallback)delegate { _pres.CurrentAlgorithm = Algorithms.Cipher1; }, null);
                }
                else if (_settings.CurrentAlgorithm == Algorithms.Cipher2)
                {
                    _pres.Dispatcher.Invoke(DispatcherPriority.Send,
                         (SendOrPostCallback)delegate { _pres.CurrentAlgorithm = Algorithms.Cipher2; }, null);
                }
                else if (_settings.CurrentAlgorithm == Algorithms.Cipher3)
                {
                    _pres.Dispatcher.Invoke(DispatcherPriority.Send,
                         (SendOrPostCallback)delegate { _pres.CurrentAlgorithm = Algorithms.Cipher3; }, null);
                }
            }
        }

        #endregion

        #region Event Handling

        public event StatusChangedEventHandler OnPluginStatusChanged;

        public event GuiLogNotificationEventHandler OnGuiLogNotificationOccured;

        public event PluginProgressChangedEventHandler OnPluginProgressChanged;

        public event PropertyChangedEventHandler PropertyChanged;

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

        #endregion
    }
}
