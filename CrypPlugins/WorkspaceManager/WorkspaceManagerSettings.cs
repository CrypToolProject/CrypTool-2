/*                              
   Copyright 2010 Nils Kopal, Viktor M.

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
using System.ComponentModel;
using CrypTool.PluginBase;

namespace WorkspaceManager
{
    internal class WorkspaceManagerSettings : ISettings
    {
        #region ISettings Members

        private WorkspaceManagerClass WorkspaceManager { get; set; }

        public WorkspaceManagerSettings(WorkspaceManagerClass manager)
        {
            WorkspaceManager = manager;
        }

        public string GuiUpdateInterval
        {
            get => CrypTool.PluginBase.Properties.Settings.Default.WorkspaceManager_GuiUpdateInterval;
            set
            {
                CrypTool.PluginBase.Properties.Settings.Default.WorkspaceManager_GuiUpdateInterval = value;
                CrypTool.PluginBase.Properties.Settings.Default.Save();
                OnPropertyChanged("GuiUpdateInterval");
            }
        }

        public string SleepTime
        {
            get => CrypTool.PluginBase.Properties.Settings.Default.WorkspaceManager_SleepTime;
            set
            {
                CrypTool.PluginBase.Properties.Settings.Default.WorkspaceManager_SleepTime = value;
                CrypTool.PluginBase.Properties.Settings.Default.Save();
                OnPropertyChanged("SleepTime");
            }
        }

        public bool BenchmarkPlugins
        {
            get => CrypTool.PluginBase.Properties.Settings.Default.WorkspaceManager_BenchmarkPlugins;
            set
            {
                CrypTool.PluginBase.Properties.Settings.Default.WorkspaceManager_BenchmarkPlugins = value;
                CrypTool.PluginBase.Properties.Settings.Default.Save();
                OnPropertyChanged("BenchmarkPlugins");
            }
        }

        public bool SynchronousEvents
        {
            get => CrypTool.PluginBase.Properties.Settings.Default.WorkspaceManager_SynchronousEvents;
            set
            {
                CrypTool.PluginBase.Properties.Settings.Default.WorkspaceManager_SynchronousEvents = value;
                CrypTool.PluginBase.Properties.Settings.Default.Save();
                OnPropertyChanged("SynchronousEvents");
            }
        }

        public int LogLevel
        {
            get => CrypTool.PluginBase.Properties.Settings.Default.WorkspaceManager_LogLevel;
            set
            {
                CrypTool.PluginBase.Properties.Settings.Default.WorkspaceManager_LogLevel = value;
                CrypTool.PluginBase.Properties.Settings.Default.Save();
                OnPropertyChanged("LogLevel");
            }
        }

        #endregion

        #region INotifyPropertyChanged Members

        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;
        public void Initialize()
        {

        }

        private void OnPropertyChanged(string p)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(p));
            }
        }

        #endregion
    }
}
