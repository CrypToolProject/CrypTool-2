using System;
using CrypTool.PluginBase;
using System.ComponentModel;

namespace WorkspaceManager
{
    class WorkspaceManagerSettings : ISettings
    {
        #region ISettings Members

        private WorkspaceManagerClass WorkspaceManager { get; set; }

        public WorkspaceManagerSettings(WorkspaceManagerClass manager)
        {
            WorkspaceManager = manager;
        }

        public String GuiUpdateInterval
        {
            get
            {
                return CrypTool.PluginBase.Properties.Settings.Default.WorkspaceManager_GuiUpdateInterval;
            }
            set
            {
                CrypTool.PluginBase.Properties.Settings.Default.WorkspaceManager_GuiUpdateInterval = value;
                CrypTool.PluginBase.Properties.Settings.Default.Save();
                OnPropertyChanged("GuiUpdateInterval");
            }
        }

        public String SleepTime
        {
            get
            {
                return CrypTool.PluginBase.Properties.Settings.Default.WorkspaceManager_SleepTime;
            }
            set
            {
                CrypTool.PluginBase.Properties.Settings.Default.WorkspaceManager_SleepTime = value;
                CrypTool.PluginBase.Properties.Settings.Default.Save();
                OnPropertyChanged("SleepTime");
            }
        }        

        public bool BenchmarkPlugins
        {
            get
            {
                return CrypTool.PluginBase.Properties.Settings.Default.WorkspaceManager_BenchmarkPlugins;
            }
            set
            {
                CrypTool.PluginBase.Properties.Settings.Default.WorkspaceManager_BenchmarkPlugins = value;
                CrypTool.PluginBase.Properties.Settings.Default.Save();
                OnPropertyChanged("BenchmarkPlugins");
            }
        }

        public bool SynchronousEvents
        {
            get
            {
                return CrypTool.PluginBase.Properties.Settings.Default.WorkspaceManager_SynchronousEvents;
            }
            set
            {
                CrypTool.PluginBase.Properties.Settings.Default.WorkspaceManager_SynchronousEvents = value;
                CrypTool.PluginBase.Properties.Settings.Default.Save();
                OnPropertyChanged("SynchronousEvents");
            }
        }

        public int LogLevel
        {
            get
            {
                return CrypTool.PluginBase.Properties.Settings.Default.WorkspaceManager_LogLevel;
            }
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
                PropertyChanged(this, new PropertyChangedEventArgs(p));
        }

        #endregion
    }
}
