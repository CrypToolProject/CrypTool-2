using CrypTool.PluginBase;
using System.Collections.Generic;
using System.ComponentModel;

namespace CrypTool.CubeAttack
{
    public class CubeAttackSettings : ISettings
    {
        #region Public CubeAttack specific interface

        /// <summary>
        /// We use this delegate to send log messages from the settings class to the CubeAttack plugin
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="msg"></param>
        /// <param name="logLevel"></param>
        public delegate void CubeAttackLogMessage(string msg, NotificationLevel logLevel);

        #endregion

        #region Private variables

        private int selectedAction = 0;
        private int publicVar;
        private int secretVar;
        private int maxcube;
        private int constTest = 50;
        private int linTest = 50;
        private string setPublicBits = "0*00*";
        private int outputBit = 1;
        private bool readSuperpolysFromFile;
        private string openFilename;
        private bool enableLogMessages = false;

        private string saveOutputSuperpoly;
        private Matrix saveSuperpolyMatrix;
        private List<List<int>> saveListCubeIndexes;
        private int[] saveOutputBitIndex;
        private int saveCountSuperpoly;
        private Matrix saveMatrixCheckLinearitySuperpolys;
        private int savePublicBitSize;
        private int saveSecretBitSize;

        #endregion


        #region Algorithm settings properties (visible in the settings pane)

        [PropertySaveOrder(1)]
        [ContextMenu("ActionCaption", "ActionTooltip",
            1,
            ContextMenuControlType.ComboBox,
            null,
            "ActionList1", "ActionList2", "ActionList3")]
        [TaskPane("ActionCaption", "ActionTooltip",
            null,
            1,
            false,
            ControlType.ComboBox,
            new string[] { "ActionList1", "ActionList2", "ActionList3" })]
        public int Action
        {
            get => selectedAction;
            set
            {
                if (value != selectedAction)
                {
                    selectedAction = value;
                    OnPropertyChanged("Action");
                }
            }
        }

        [PropertySaveOrder(2)]
        [TaskPane("PublicVarCaption", "PublicVarTooltip",
            null,
            2,
            false,
            ControlType.NumericUpDown,
            ValidationType.RangeInteger,
            1,
            10000)]
        public int PublicVar
        {
            get => publicVar;
            set
            {
                if (value != publicVar)
                {
                    publicVar = value;
                    OnPropertyChanged("PublicVar");
                }
            }
        }

        [PropertySaveOrder(3)]
        [TaskPane("SecretVarCaption", "SecretVarTooltip",
            null,
            3,
            false,
            ControlType.NumericUpDown,
            ValidationType.RangeInteger,
            1,
            10000)]
        public int SecretVar
        {
            get => secretVar;
            set
            {
                if (value != secretVar)
                {
                    secretVar = value;
                    OnPropertyChanged("SecretVar");
                }
            }
        }

        [PropertySaveOrder(4)]
        [TaskPane("MaxCubeCaption", "MaxCubeTooltip",
            null,
            4,
            false,
            ControlType.NumericUpDown,
            ValidationType.RangeInteger,
            1,
            10000)]
        public int MaxCube
        {
            get => maxcube;
            set
            {
                if (value != maxcube)
                {
                    maxcube = value;
                    OnPropertyChanged("MaxCube");
                }
            }
        }

        [PropertySaveOrder(5)]
        [TaskPane("ConstTestCaption", "ConstTestTooltip",
            null,
            5,
            false,
            ControlType.NumericUpDown,
            ValidationType.RangeInteger,
            0,
            100000)]
        public int ConstTest
        {
            get => constTest;
            set
            {
                if (value != constTest)
                {
                    constTest = value;
                    OnPropertyChanged("ConstTest");
                }
            }
        }

        [PropertySaveOrder(6)]
        [TaskPane("LinTestCaption", "LinTestTooltip",
            null,
            6,
            false,
            ControlType.NumericUpDown,
            ValidationType.RangeInteger,
            0,
            100000)]
        public int LinTest
        {
            get => linTest;
            set
            {
                if (value != linTest)
                {
                    linTest = value;
                    OnPropertyChanged("LinTest");
                }
            }
        }

        [PropertySaveOrder(7)]
        [TaskPane("OutputBitCaption", "OutputBitTooltip",
            null,
            7,
            true,
            ControlType.NumericUpDown,
            ValidationType.RangeInteger,
            1,
            10000)]
        public int OutputBit
        {
            get => outputBit;
            set
            {
                if (value != outputBit)
                {
                    outputBit = value;
                    OnPropertyChanged("OutputBit");
                }
            }
        }

        [PropertySaveOrder(8)]
        [TaskPane("SetPublicBitsCaption", "SetPublicBitsTooltip",
            null,
            8,
            false,
            ControlType.TextBox,
            null)]
        public string SetPublicBits
        {
            get => setPublicBits ?? string.Empty;
            set
            {
                if (value != setPublicBits)
                {
                    setPublicBits = value;
                    OnPropertyChanged("SetPublicBits");
                }
            }
        }

        [PropertySaveOrder(9)]
        [ContextMenu("ReadSuperpolysFromFileCaption", "ReadSuperpolysFromFileTooltip",
            9,
            ContextMenuControlType.CheckBox,
            null,
            new string[] { "ReadSuperpolysFromFileList1" })]
        [TaskPane("ReadSuperpolysFromFileCaption", "ReadSuperpolysFromFileTooltip",
            null,
            9,
            false,
            ControlType.CheckBox,
            "",
            null)]
        public bool ReadSuperpolysFromFile
        {
            get => readSuperpolysFromFile;
            set
            {
                if (value != readSuperpolysFromFile)
                {
                    readSuperpolysFromFile = value;
                    OnPropertyChanged("ReadSuperpolysFromFile");
                }
            }
        }

        [PropertySaveOrder(10)]
        [TaskPane("OpenFilenameCaption", "OpenFilenameTooltip",
            null,
            10,
            false,
            ControlType.OpenFileDialog,
            FileExtension = "All Files (*.*)|*.*")]
        public string OpenFilename
        {
            get => openFilename;
            set
            {
                if (value != openFilename)
                {
                    openFilename = value;
                    OnPropertyChanged("OpenFilename");
                }
            }
        }

        [PropertySaveOrder(11)]
        [ContextMenu("EnableLogMessagesCaption", "EnableLogMessagesTooltip",
            11,
            ContextMenuControlType.CheckBox,
            null,
            new string[] { "EnableLogMessagesList1" })]
        [TaskPane("EnableLogMessagesCaption", "EnableLogMessagesTooltip",
            null,
            11,
            false,
            ControlType.CheckBox,
            "",
            null)]
        public bool EnableLogMessages
        {
            get => enableLogMessages;
            set
            {
                if (value != enableLogMessages)
                {
                    enableLogMessages = value;
                    OnPropertyChanged("EnableLogMessages");
                }
            }
        }

        public string SaveOutputSuperpoly
        {
            get => saveOutputSuperpoly;
            set
            {
                if (value != saveOutputSuperpoly)
                {
                    saveOutputSuperpoly = value;
                    OnPropertyChanged("SaveOutputSuperpoly");
                }
            }
        }

        public Matrix SaveSuperpolyMatrix
        {
            get => saveSuperpolyMatrix;
            set
            {
                if (value != saveSuperpolyMatrix)
                {
                    saveSuperpolyMatrix = value;
                    OnPropertyChanged("SaveSuperpolyMatrix");
                }
            }
        }

        public List<List<int>> SaveListCubeIndexes
        {
            get => saveListCubeIndexes;
            set
            {
                if (value != saveListCubeIndexes)
                {
                    saveListCubeIndexes = value;
                    OnPropertyChanged("SaveListCubeIndexes");
                }
            }
        }

        public int[] SaveOutputBitIndex
        {
            get => saveOutputBitIndex;
            set
            {
                if (value != saveOutputBitIndex)
                {
                    saveOutputBitIndex = value;
                    OnPropertyChanged("SaveOutputBitIndex");
                }
            }
        }

        public int SaveCountSuperpoly
        {
            get => saveCountSuperpoly;
            set
            {
                if (value != saveCountSuperpoly)
                {
                    saveCountSuperpoly = value;
                    OnPropertyChanged("SaveCountSuperpoly");
                }
            }
        }

        public Matrix SaveMatrixCheckLinearitySuperpolys
        {
            get => saveMatrixCheckLinearitySuperpolys;
            set
            {
                if (value != saveMatrixCheckLinearitySuperpolys)
                {
                    saveMatrixCheckLinearitySuperpolys = value;
                    OnPropertyChanged("SaveMatrixCheckLinearitySuperpolys");
                }
            }
        }

        public int SavePublicBitSize
        {
            get => savePublicBitSize;
            set
            {
                if (value != savePublicBitSize)
                {
                    savePublicBitSize = value;
                    OnPropertyChanged("SavePublicBitSize");
                }
            }
        }

        public int SaveSecretBitSize
        {
            get => saveSecretBitSize;
            set
            {
                if (value != saveSecretBitSize)
                {
                    OnPropertyChanged("SaveSecretBitSize");
                    saveSecretBitSize = value;
                }
            }
        }

        #endregion

        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;
        public void Initialize()
        {

        }

        protected void OnPropertyChanged(string name)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(name));
            }
        }

        #endregion
    }
}
