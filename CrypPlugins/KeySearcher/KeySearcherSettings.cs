using System;
using System.Collections.Generic;
using System.Windows;
using CrypTool.PluginBase;
using System.ComponentModel;
using System.Collections.ObjectModel; 
using KeySearcher.Properties;
using KeyTextBox;
using OpenCLNet;

namespace KeySearcher
{
    public class KeySearcherSettings : ISettings
    {
        private readonly KeySearcher keysearcher;
        private int coresUsed;
        private string csvPath = "";

        public class OpenCLDeviceSettings
        {
            private KeySearcherSettings _settings;
            internal string name;
            internal int index;
            internal int mode;
            internal bool useDevice;

            public bool UseDevice
            {
                get { return useDevice; }
                set
                {
                    if (_settings.OpenCLDevice == index)
                        _settings.UseOpenCL = value;
                    useDevice = value;
                }
            }

            public OpenCLDeviceSettings(KeySearcherSettings settings)
            {
                _settings = settings;
            }
        }

        private List<OpenCLDeviceSettings> deviceSettings = new List<OpenCLDeviceSettings>();
        public List<OpenCLDeviceSettings> DeviceSettings
        {
            get
            {
                return deviceSettings;
            }
        }

        public event TaskPaneAttributeChangedHandler TaskPaneAttributeChanged;

        public KeySearcherSettings(KeySearcher ks, OpenCLManager oclManager)
        {
            keysearcher = ks;
            RefreshDevicesList(oclManager);

            CoresAvailable.Clear();
            for (int i = -1; i < Environment.ProcessorCount; i++)
                CoresAvailable.Add((i + 1).ToString());
            CoresUsed = Environment.ProcessorCount - 1;
            KeyManager = new SimpleKeyManager("");
        }

        private void RefreshDevicesList(OpenCLManager oclManager)
        {
            devicesAvailable.Clear();
            int c = 0;
            if (oclManager != null)
            {
                for (var id = 0; id < OpenCL.GetPlatforms().Length; id++)
                {
                    oclManager.CreateDefaultContext(id, DeviceType.ALL);
                    foreach (var device in oclManager.Context.Devices)
                    {
                        string deviceName = device.Vendor + ":" + device.Name;
                        deviceSettings.Add(new OpenCLDeviceSettings(this) { name = deviceName, index = c, mode = 1, UseDevice = false });
                        devicesAvailable.Add(deviceName);
                        c++;
                    }
                }
            }
            DevicesAvailable = devicesAvailable;    //refresh list
            if (devicesAvailable.Count > 0)
                OpenCLDevice = 0;
            else
                this.openCLDevice = -1;
        }

        public void Initialize()
        {            
            OpenCLGroupVisiblity();
            CrypTool.PluginBase.Properties.Settings.Default.PropertyChanged += delegate
                                                    {
                                                        OpenCLGroupVisiblity();
                                                    };
                       
        }

        [TaskPane("KeyCaption", "KeyTooltip", null, 1, false, ControlType.KeyTextBox, true, "KeyManager")]
        public String Key
        {
            get
            {
                return KeyManager.GetKey();
            }
            set
            {
                KeyManager.SetKey(value);
                OnPropertyChanged("Key");
                //if (!(keysearcher.Pattern != null && keysearcher.Pattern.testWildcardKey(value)))
                //    keysearcher.GuiLogMessage(Resources.Wrong_key_pattern_, NotificationLevel.Error);
            }
        }

        public KeyTextBox.SimpleKeyManager KeyManager { get; private set; }

        [TaskPane( "ResetCaption", "ResetTooltip", null, 2, false, ControlType.Button)]
        public void Reset()
        {
            if (keysearcher != null && keysearcher.Pattern != null)
            {
                Key = keysearcher.Pattern.giveInputPattern();
            }
        }
        
        [TaskPane( "CoresUsedCaption", "CoresUsedTooltip", null, 3, false, ControlType.DynamicComboBox, new string[] { "CoresAvailable" })]
        public int CoresUsed
        {
            get { return this.coresUsed; }
            set
            {
                if (value != this.coresUsed)
                {
                    this.coresUsed = value;
                    OnPropertyChanged("CoresUsed");
                }
            }
        }

        #region crypcloud

        private bool usePeerToPeer;
        [TaskPane("settings__caption_useNetwork", "settings__tooltip_useNetwork", "GroupPeerToPeer", 0, false, ControlType.CheckBox)]
        public bool UsePeerToPeer
        {
            get { return usePeerToPeer; }
            set
            {
                if (value != usePeerToPeer)
                {
                    usePeerToPeer = value;
                    OnPropertyChanged("UsePeerToPeer");
                }
            }
        }

        private int numberOfBlocks;
        [TaskPane("settings__caption_numberOfBlocks", "settings__tooltip_numberOfBlocks", "GroupPeerToPeer", 3, false, ControlType.NumericUpDown, ValidationType.RangeInteger, 1, 128)]
        public int NumberOfBlocks
        {
            get { return numberOfBlocks; }
            set
            {
                if (value != numberOfBlocks)
                {
                    numberOfBlocks = value;
                    OnPropertyChanged("NumberOfBlocks"); 
                }
            }
        }

        #endregion

        private string evaluationHost;
        //[TaskPane( "EvaluationHostCaption", "EvaluationHostTooltip", "GroupEvaluation", 0, false, ControlType.TextBox)]
        public String EvaluationHost
        {
            get
            {
                return evaluationHost;
            }
            set
            {
                if (value != evaluationHost)
                {
                    evaluationHost = value;
                    OnPropertyChanged("EvaluationHost");
                }
            }
        }

        private string evaluationUser;
        //[TaskPane( "EvaluationUserCaption", "EvaluationUserTooltip", "GroupEvaluation", 1, false, ControlType.TextBox)]
        public String EvaluationUser
        {
            get
            {
                return evaluationUser;
            }
            set
            {
                if (value != evaluationUser)
                {
                    evaluationUser = value;
                    OnPropertyChanged("EvaluationUser");
                }
            }
        }

        private string evaluationPassword;
        //[TaskPane( "EvaluationPasswordCaption", "EvaluationPasswordTooltip", "GroupEvaluation", 2, false, ControlType.TextBox)]
        public String EvaluationPassword
        {
            get
            {
                return evaluationPassword;
            }
            set
            {
                if (value != evaluationPassword)
                {
                    evaluationPassword = value;
                    OnPropertyChanged("EvaluationPassword");
                }
            }
        }

        private string evaluationDatabase;
        //[TaskPane( "EvaluationDatabaseCaption", "EvaluationDatabaseTooltip", "GroupEvaluation", 3, false, ControlType.TextBox)]
        public String EvaluationDatabase
        {
            get
            {
                return evaluationDatabase;
            }
            set
            {
                if (value != evaluationDatabase)
                {
                    evaluationDatabase = value;
                    OnPropertyChanged("EvaluationDatabase");
                }
            }
        }

        #region OpenCL

        [TaskPane( "NoOpenCLCaption", "NoOpenCLTooltip", "GroupOpenCL", 1, false, ControlType.TextBoxReadOnly)]
        [DontSave]
        public string NoOpenCL
        {
            get { return Resources.No_OpenCL_Device_available_; }
            set {}
        }

        private void OpenCLGroupVisiblity()
        {
            if (TaskPaneAttributeChanged == null)
                return;

            if (!CrypTool.PluginBase.Properties.Settings.Default.KeySearcher_UseOpenCL)
            {
                TaskPaneAttributeChanged(this, new TaskPaneAttributeChangedEventArgs(new TaskPaneAttribteContainer("OpenCLDevice", Visibility.Collapsed)));
                TaskPaneAttributeChanged(this, new TaskPaneAttributeChangedEventArgs(new TaskPaneAttribteContainer("OpenCLMode", Visibility.Collapsed)));
                TaskPaneAttributeChanged(this, new TaskPaneAttributeChangedEventArgs(new TaskPaneAttribteContainer("UseOpenCL", Visibility.Collapsed)));
                TaskPaneAttributeChanged(this, new TaskPaneAttributeChangedEventArgs(new TaskPaneAttribteContainer("NoOpenCL", Visibility.Collapsed)));
            }
            else
            {
                if (DevicesAvailable.Count == 0)
                {
                    TaskPaneAttributeChanged(this, new TaskPaneAttributeChangedEventArgs(new TaskPaneAttribteContainer("OpenCLDevice", Visibility.Collapsed)));
                    TaskPaneAttributeChanged(this, new TaskPaneAttributeChangedEventArgs(new TaskPaneAttribteContainer("OpenCLMode", Visibility.Collapsed)));
                    TaskPaneAttributeChanged(this, new TaskPaneAttributeChangedEventArgs(new TaskPaneAttribteContainer("UseOpenCL", Visibility.Collapsed)));
                    TaskPaneAttributeChanged(this, new TaskPaneAttributeChangedEventArgs(new TaskPaneAttribteContainer("NoOpenCL", Visibility.Visible)));
                }
                else
                {
                    TaskPaneAttributeChanged(this, new TaskPaneAttributeChangedEventArgs(new TaskPaneAttribteContainer("OpenCLDevice", Visibility.Visible)));
                    TaskPaneAttributeChanged(this, new TaskPaneAttributeChangedEventArgs(new TaskPaneAttribteContainer("OpenCLMode", Visibility.Visible)));
                    TaskPaneAttributeChanged(this, new TaskPaneAttributeChangedEventArgs(new TaskPaneAttribteContainer("UseOpenCL", Visibility.Visible)));
                    TaskPaneAttributeChanged(this, new TaskPaneAttributeChangedEventArgs(new TaskPaneAttribteContainer("NoOpenCL", Visibility.Collapsed)));
                }
            }
        }

        private int openCLDevice;
        [TaskPane( "OpenCLDeviceCaption", "OpenCLDeviceTooltip", "GroupOpenCL", 1, false, ControlType.DynamicComboBox, new string[] { "DevicesAvailable" })]
        [DontSave]
        public int OpenCLDevice
        {
            get { return this.openCLDevice; }
            set
            {
                if (value != this.openCLDevice)
                {
                    this.openCLDevice = value;
                    UseOpenCL = deviceSettings[value].UseDevice;
                    OpenCLMode = deviceSettings[value].mode;
                    OnPropertyChanged("OpenCLDevice");
                }
            }
        }

        [TaskPane( "UseOpenCLCaption", "UseOpenCLTooltip", "GroupOpenCL", 2, false, ControlType.CheckBox)]
        [DontSave]
        public bool UseOpenCL
        {
            get
            {
                if (OpenCLDevice != -1 && deviceSettings.Count > OpenCLDevice)
                    return deviceSettings[OpenCLDevice].UseDevice;
                else
                    return false;
            }
            set
            {
                if (OpenCLDevice != -1 && (deviceSettings.Count > OpenCLDevice) && (value != deviceSettings[OpenCLDevice].UseDevice))
                {
                    deviceSettings[OpenCLDevice].useDevice = value;
                    OnPropertyChanged("UseOpenCL");
                }
            }
        }

        [TaskPane("OpenCLModeCaption", "OpenCLModeTooltip", "GroupOpenCL", 3, false, ControlType.RadioButton, new string[] { "OpenCLModeList1", "OpenCLModeList2", "OpenCLModeList3" })]
        [DontSave]
        public int OpenCLMode
        {
            get
            {
                if (OpenCLDevice != -1 && deviceSettings.Count > OpenCLDevice)
                    return deviceSettings[OpenCLDevice].mode;
                else
                    return 0;
            }
            set
            {
                if (OpenCLDevice != -1 && (deviceSettings.Count > OpenCLDevice) && (value != deviceSettings[OpenCLDevice].mode))
                {
                    if (CrypTool.PluginBase.Properties.Settings.Default.KeySearcher_EnableHighLoad || value != 2)
                        deviceSettings[OpenCLDevice].mode = value;
                    else
                        keysearcher.GuiLogMessage(
                            "Using \"High Load\" is disabled. Please check your CrypTool 2 settings.", NotificationLevel.Error);

                    OnPropertyChanged("OpenCLMode");
                }
            }
        }
        
        private ObservableCollection<string> devicesAvailable = new ObservableCollection<string>();
        [DontSave]
        public ObservableCollection<string> DevicesAvailable
        {
            get { return devicesAvailable; }
            set
            {
                if (value != devicesAvailable)
                {
                    devicesAvailable = value;
                }
                OnPropertyChanged("DevicesAvailable");
            }
        }

        #endregion

        #region Statistic
        /// <summary>
        /// Getter/Setter for the time interval (minutes)
        /// </summary>
        private int updatetime = 30;
        //[TaskPane( "UpdateTimeCaption", "UpdateTimeTooltip", "GroupStatisticPath", 1, false, ControlType.TextBox)]
        public int UpdateTime
        {
            get { return updatetime; }
            set
            {
                if (value != updatetime)
                {
                    updatetime = value;
                    OnPropertyChanged("UpdateTime");
                }
            }
        }

        /// <summary>
        /// Able/Disable for the update time interval
        /// </summary>
        private bool disableupdate = false;
        //[TaskPane( "DisableUpdateCaption", "DisableUpdateTooltip", "GroupStatisticPath", 2, false, ControlType.CheckBox)]
        public bool DisableUpdate
        {
            get { return disableupdate; }
            set
            {
                if (value != disableupdate)
                {
                    disableupdate = value;
                    OnPropertyChanged("DisableUpdate");
                }
            }
        }
         
        /// <summary>
        /// Getter/Setter for the csv file
        /// </summary>
        //[TaskPane( "CsvPathCaption", "CsvPathTooltip", "GroupStatisticPath", 3, false, ControlType.SaveFileDialog, FileExtension = "Comma Seperated Values (*.csv)|*.csv")]
        public string CsvPath
        {
            get { return csvPath; }
            set
            {
                if (value != csvPath)
                {
                    csvPath = value;
                    OnPropertyChanged("CsvPath");
                }
            }
        }

        /// <summary>
        /// Button to "reset" the csv file. That means it will not appear any more in the text field
        /// </summary>
        //[TaskPane( "DefaultPathCaption", "DefaultPathTooltip", "GroupStatisticPath", 4, false, ControlType.Button)]
        public void DefaultPath()
        {
            csvPath = "";
            OnPropertyChanged("CsvPath");
        }       
        #endregion

        private ObservableCollection<string> coresAvailable = new ObservableCollection<string>();
        [DontSave]
        public ObservableCollection<string> CoresAvailable
        {
            get { return coresAvailable; }
            set
            {
                if (value != coresAvailable)
                {
                    coresAvailable = value;
                    OnPropertyChanged("CoresAvailable");
                }
            }
        }

        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;

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
