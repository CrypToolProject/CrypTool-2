using CrypTool.PluginBase;
using KeySearcher.Properties;
using KeyTextBox;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;

namespace KeySearcher
{
    public class KeySearcherSettings : ISettings
    {
        private readonly KeySearcher keysearcher;
        private int coresUsed;
        private string csvPath = "";
      
        public event TaskPaneAttributeChangedHandler TaskPaneAttributeChanged;

        public KeySearcherSettings(KeySearcher ks)
        {
            keysearcher = ks;

            CoresAvailable.Clear();
            for (int i = -1; i < Environment.ProcessorCount; i++)
            {
                CoresAvailable.Add((i + 1).ToString());
            }

            CoresUsed = Environment.ProcessorCount - 1;
            KeyManager = new SimpleKeyManager("");
        }

        public void Initialize()
        {       
        }

        [TaskPane("KeyCaption", "KeyTooltip", null, 1, false, ControlType.KeyTextBox, true, "KeyManager")]
        public string Key
        {
            get => KeyManager.GetKey();
            set
            {
                KeyManager.SetKey(value);
                OnPropertyChanged("Key");
                //if (!(keysearcher.Pattern != null && keysearcher.Pattern.testWildcardKey(value)))
                //    keysearcher.GuiLogMessage(Resources.Wrong_key_pattern_, NotificationLevel.Error);
            }
        }

        public KeyTextBox.SimpleKeyManager KeyManager { get; private set; }

        [TaskPane("ResetCaption", "ResetTooltip", null, 2, false, ControlType.Button)]
        public void Reset()
        {
            if (keysearcher != null && keysearcher.Pattern != null)
            {
                Key = keysearcher.Pattern.giveInputPattern();
            }
        }

        [TaskPane("CoresUsedCaption", "CoresUsedTooltip", null, 3, false, ControlType.DynamicComboBox, new string[] { "CoresAvailable" })]
        public int CoresUsed
        {
            get => coresUsed;
            set
            {
                if (value != coresUsed)
                {
                    coresUsed = value;
                    OnPropertyChanged("CoresUsed");
                }
            }
        }

        #region crypcloud

        private bool usePeerToPeer;
        [TaskPane("settings__caption_useNetwork", "settings__tooltip_useNetwork", "GroupPeerToPeer", 0, false, ControlType.CheckBox)]
        public bool UsePeerToPeer
        {
            get => usePeerToPeer;
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
            get => numberOfBlocks;
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
        public string EvaluationHost
        {
            get => evaluationHost;
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
        public string EvaluationUser
        {
            get => evaluationUser;
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
        public string EvaluationPassword
        {
            get => evaluationPassword;
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
        public string EvaluationDatabase
        {
            get => evaluationDatabase;
            set
            {
                if (value != evaluationDatabase)
                {
                    evaluationDatabase = value;
                    OnPropertyChanged("EvaluationDatabase");
                }
            }
        }     

        #region Statistic
        /// <summary>
        /// Getter/Setter for the time interval (minutes)
        /// </summary>
        private int updatetime = 30;
        //[TaskPane( "UpdateTimeCaption", "UpdateTimeTooltip", "GroupStatisticPath", 1, false, ControlType.TextBox)]
        public int UpdateTime
        {
            get => updatetime;
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
            get => disableupdate;
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
            get => csvPath;
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
            get => coresAvailable;
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
